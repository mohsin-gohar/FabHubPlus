using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FanHubPlus.Models.ViewModels;

// ---------- Registration ----------
public class RegisterViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Display name")]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ---------- Login ----------
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

// ---------- Forgot / Reset password ----------
public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}

// ---------- Profile (display settings + favourite fandoms) ----------
public class ProfileViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Display name")]
    public string Name { get; set; } = string.Empty;

    public string? CurrentAvatarUrl { get; set; }

    [Display(Name = "New avatar (jpg/png/webp, max 2 MB)")]
    public IFormFile? AvatarFile { get; set; }

    [Display(Name = "Dark mode")]
    public bool DarkMode { get; set; }

    [Range(14, 22)]
    [Display(Name = "Base font size (px)")]
    public int FontSize { get; set; } = 16;

    public List<int> FavoriteCategoryIds { get; set; } = new();
    public List<Category> AllCategories { get; set; } = new();
}

// ---------- Change password (embedded in Profile page) ----------
public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string OldPassword { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
