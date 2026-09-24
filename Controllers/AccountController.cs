using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

/// <summary>
/// Authentication + user profile: Register, Login, Logout,
/// Forgot/Reset password (e-mail token flow), Profile with preferences + favourites.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _email;
    private readonly IFileUploadService _uploads;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<UserCategory> _userCategories;

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             IEmailService email,
                             IFileUploadService uploads,
                             IRepository<Category> categories,
                             IRepository<UserCategory> userCategories)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _email = email;
        _uploads = uploads;
        _categories = categories;
        _userCategories = userCategories;
    }

    // ================= REGISTER =================
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Name = model.Name,
            EmailConfirmed = true // demo-friendly: skip inbox confirmation; welcome e-mail still sent
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "User");
        await _signInManager.SignInAsync(user, isPersistent: false);

        await _email.SendAsync(user.Email!, "Welcome to Fan Hub Plus",
            $"<h2>Hello {System.Net.WebUtility.HtmlEncode(user.Name)}!</h2>" +
            "<p>Your Fan Hub Plus account is ready. Explore fandoms, rate content and bookmark your favourites.</p>");

        SavePreferenceCookie(user.DarkMode, user.FontSize);
        TempData["StatusMessage"] = "Welcome to Fan Hub Plus! Your account was created.";
        return RedirectToAction(nameof(Profile));
    }

    // ================= LOGIN / LOGOUT =================
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // PasswordSignInAsync applies the Identity lockout policy automatically
        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null) SavePreferenceCookie(user.DarkMode, user.FontSize);

            return !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
                ? LocalRedirect(model.ReturnUrl)
                : RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty,
            result.IsLockedOut
                ? "Account locked after too many attempts. Try again in 5 minutes."
                : "Invalid login attempt.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete("fhp_pref"); // forget stored display preferences
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    // ================= FORGOT / RESET PASSWORD =================
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ForgotStatus"] = "Please enter a valid e-mail address.";
            return RedirectToAction(nameof(Login));
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        var token = user is null ? null : await _userManager.GeneratePasswordResetTokenAsync(user);

        if (token is not null)
        {
            var link = Url.Action(nameof(ResetPassword), "Account",
                new { token, email = model.Email }, Request.Scheme)!;

            var sent = await _email.SendAsync(model.Email, "Reset your Fan Hub Plus password",
                $"<p>We received a password reset request for Fan Hub Plus.</p>" +
                $"<p><a href='{link}'>Click here to choose a new password</a></p>" +
                "<p>If you did not request this, simply ignore this e-mail.</p>");

            // With SMTP disabled the link is shown on-screen so demos still work offline
            if (!sent) TempData["EmailPreviewLink"] = link;
        }

        // Always the same message (never reveal whether an e-mail exists)
        TempData["ForgotStatus"] =
            "If that e-mail is registered, a reset link has been sent. Check your inbox.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
        => View(new ResetPasswordViewModel { Token = token, Email = email });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid reset request.");
            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        TempData["StatusMessage"] = "Password updated – you can log in now.";
        return RedirectToAction(nameof(Login));
    }

    // ================= PROFILE (name, avatar, theme, favourites) =================
    [HttpGet, Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var favoriteIds = await _userCategories.Query()
            .Where(uc => uc.UserId == user.Id)
            .Select(uc => uc.CategoryId)
            .ToListAsync();

        var vm = new ProfileViewModel
        {
            Name = user.Name,
            CurrentAvatarUrl = user.AvatarUrl,
            DarkMode = user.DarkMode,
            FontSize = user.FontSize,
            FavoriteCategoryIds = favoriteIds,
            AllCategories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        return View(vm);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            model.AllCategories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        user.Name = model.Name;
        user.DarkMode = model.DarkMode;
        user.FontSize = Math.Clamp(model.FontSize, 14, 22);

        if (model.AvatarFile is { Length: > 0 })
        {
            try
            {
                var path = await _uploads.SaveImageAsync(model.AvatarFile, "avatars");
                _uploads.DeleteImage(user.AvatarUrl); // remove the old file first
                user.AvatarUrl = path;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.AvatarFile), ex.Message);
                model.AllCategories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            model.AllCategories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        // ---- Sync favourite categories (many-to-many UserCategory) ----
        var current = await _userCategories.Query()
            .Where(uc => uc.UserId == user.Id)
            .ToListAsync();

        foreach (var stale in current.Where(c => !model.FavoriteCategoryIds.Contains(c.CategoryId)))
            _userCategories.Remove(stale);

        foreach (var id in model.FavoriteCategoryIds)
            if (current.All(c => c.CategoryId != id))
                await _userCategories.AddAsync(new UserCategory { UserId = user.Id, CategoryId = id });

        await _userCategories.SaveChangesAsync();

        SavePreferenceCookie(user.DarkMode, user.FontSize); // sync rendered theme instantly
        TempData["StatusMessage"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    // ================= CHANGE PASSWORD =================
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Profile));

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user); // keep the cookie valid after password change
            TempData["StatusMessage"] = "Password changed successfully.";
        }
        else
        {
            TempData["PasswordError"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }
        return RedirectToAction(nameof(Profile));
    }

    // ---- Stores dark-mode + font-size so the layout can render them without a DB hit ----
    private void SavePreferenceCookie(bool darkMode, int fontSize)
    {
        Response.Cookies.Append("fhp_pref", $"{(darkMode ? 1 : 0)}|{fontSize}",
            new CookieOptions { HttpOnly = false, Expires = DateTimeOffset.UtcNow.AddYears(1) });
    }
}


