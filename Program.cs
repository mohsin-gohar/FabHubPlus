using FanHubPlus.Data;
using FanHubPlus.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC services (controllers + Razor views) ----------
builder.Services.AddControllersWithViews();

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

// Runs automatically on startup: roles, admin user, demo user, 8 categories
builder.Services.AddScoped<DbSeeder>();

// NOTE (later modules): EmailService, FileService, ChatbotService, StatsService
// will be registered here with builder.Services.AddScoped<...>().

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
app.UseStaticFiles();   // serves wwwroot (css, js, images, uploads)

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

app.Run();
