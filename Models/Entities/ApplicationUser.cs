using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

/// <summary>
/// Our user table. Inherits all Identity columns (Id, Email, PasswordHash,
/// EmailConfirmed...) and adds Fan Hub Plus specific fields.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;   // display name

    [StringLength(300)]
    public string? AvatarUrl { get; set; }             // optional profile picture

    // ---- Display preferences (saved per user) ----
    public bool DarkMode { get; set; } = false;        // dark theme on/off
    public int FontSize { get; set; } = 16;            // base font size in pixels

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // joined date (for stats)

    // Navigation property: this user's favorite fandom categories
    public ICollection<UserCategory> FavoriteCategories { get; set; } = new List<UserCategory>();
}
