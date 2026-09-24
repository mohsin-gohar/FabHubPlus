using FanHubPlus.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace FanHubPlus.Data;

/// <summary>
/// Runs automatically on app startup (called from Program.cs).
/// It only inserts data that is MISSING, so it is safe to run many times.
/// Seeds: 2 roles, 1 admin, 1 demo user, the 8 fandom categories.
/// </summary>
public class DbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public DbSeeder(RoleManager<IdentityRole> roleManager,
                    UserManager<ApplicationUser> userManager,
                    ApplicationDbContext db)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
    }

    public async Task SeedAsync()
    {
        // ---------- 1) Roles: Admin and User ----------
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // ---------- 2) Admin account (for the Admin panel) ----------
        await EnsureUserAsync("admin@fanhubplus.com", "Admin@123", "Site Admin", "Admin");

        // ---------- 3) Demo normal user (for testing + demo video) ----------
        await EnsureUserAsync("user@fanhubplus.com", "User@123", "Demo Fan", "User");

        // ---------- 4) The 8 fandom categories ----------
        if (!_db.Categories.Any())
        {
            _db.Categories.AddRange(
                new Category { Name = "Anime",    Description = "Japanese animation - series, movies and OVAs." },
                new Category { Name = "Gaming",   Description = "Video games, esports and game franchises." },
                new Category { Name = "Movies",   Description = "Cinematic universes, franchises and blockbusters." },
                new Category { Name = "TV Shows", Description = "Binge-worthy television series and dramas." },
                new Category { Name = "K-Pop",    Description = "Korean pop groups, albums and comebacks." },
                new Category { Name = "Comics",   Description = "Western comics, superheroes and graphic novels." },
                new Category { Name = "Manga",    Description = "Japanese manga series and light novels." },
                new Category { Name = "Cosplay",  Description = "Cosplay culture, costumes and conventions." }
            );
            await _db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Creates a user with password + role only if the email is not registered yet.
    /// Passwords satisfy the Identity rules in Program.cs (8 chars, upper, lower, digit, symbol).
    /// </summary>
    private async Task EnsureUserAsync(string email, string password, string name, string role)
    {
        if (await _userManager.FindByEmailAsync(email) != null)
            return; // already exists

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = name,
            EmailConfirmed = true // seed accounts skip email confirmation (easy demo login)
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not seed user {email}: {errors}");
        }
    }
}
