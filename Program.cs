using FanHubPlus.Data;
using FanHubPlus.Models.Entities;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC services (controllers + Razor views) ----------
builder.Services.AddControllersWithViews();

// ---------- Response compression ----------
// The Misao theme alone ships ~550 KB of CSS + JS. Brotli/GZip takes that
// down to roughly a fifth on the wire, which is the single biggest
// load-time win available and costs nothing at runtime.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",               // the chatbot + bookmark endpoints
        "application/wasm",
        "image/svg+xml",                 // inline icons are text, they compress hard
    });
});

// ---------- EF Core + SQL Server (Code-First) ----------
// The connection string lives in appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- ASP.NET Core Identity ----------
// Handles: password hashing, login/logout, roles, email tokens, reset-password tokens
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password rules => strong passwords for every user (hashed, never stored as plain text)
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true; // must contain a symbol like @ # $ ...

    options.User.RequireUniqueEmail = true; // one account per email

    // Brute-force protection: lock the account for 5 minutes after 5 wrong passwords
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // IMPORTANT: creates tokens for email confirmation + forgot password

// Cookie settings: where to send the user when not logged in / not allowed
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ---------- Generic Repository (Unit of Work = shared scoped DbContext) ----------
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ---------- Application services (Controller -> Service -> Repository -> EF Core) ----------
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<ISupportService, SupportService>();

// Runs automatically on startup: roles, admin user, demo user, 8 categories, demo content
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// ---------- Seed the database (safe to run every time) ----------
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

// ---------- Middleware pipeline (order matters!) ----------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // detailed errors while coding
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // browser must use HTTPS in production (method is "UseHsts")
}

app.UseHttpsRedirection();
app.UseResponseCompression();   // must sit before StaticFiles + the endpoints

// Theme assets are versioned by deployment, app assets (site.css / site.js) by
// their ?v= hash, and user uploads rarely. A short shared cache keeps repeat
// visits at zero extra round trips without ever serving a stale upload.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var path = context.Context.Request.Path.Value ?? string.Empty;
        var isThemeAsset = path.StartsWith("/assets", StringComparison.OrdinalIgnoreCase)
                        || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase);
        context.Context.Response.Headers[HeaderNames.CacheControl] =
            isThemeAsset ? "public,max-age=604800" : "public,max-age=3600";
    },
});

app.UseRouting();

app.UseAuthentication(); // 1) WHO is logged in? (reads the Identity cookie)
app.UseAuthorization();  // 2) Is he allowed?  ([Authorize], [Authorize(Roles="Admin")])

// Admin Area route first, then the normal (default) route
app.MapControllerRoute(
    name: "adminArea",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Friendly error page for 404 / 500 (status code middleware re-executes Home/Error?statusCode=...)
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.Run();
