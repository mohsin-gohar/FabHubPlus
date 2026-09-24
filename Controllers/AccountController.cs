using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

