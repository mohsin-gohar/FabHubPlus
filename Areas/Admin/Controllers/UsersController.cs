using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

// User administration: search, promote/demote roles, lock/unlock accounts
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        page = Math.Max(1, page);
        const int pageSize = 10;

        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(s) || u.Name.ToLower().Contains(s));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = new List<AdminUserRowViewModel>();
        foreach (var u in users)
            rows.Add(new AdminUserRowViewModel
            {
                User = u,
                Roles = await _userManager.GetRolesAsync(u)
            });

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalItems = await query.CountAsync();
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(ViewBag.TotalItems / (double)pageSize));
        return View(rows);
    }

    // Promote/demote between User and Admin (admins cannot demote themselves)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var current = await _userManager.GetUserAsync(User);

        if (user.Id == current?.Id)
        {
            TempData["StatusError"] = "You cannot change your own role.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
            await _userManager.AddToRoleAsync(user, "User");
            TempData["StatusMessage"] = $"{user.Email} demoted to User.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["StatusMessage"] = $"{user.Email} promoted to Admin.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Lock (5 min) / unlock an account (brute-force recovery)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (await _userManager.GetLockoutEndDateAsync(user) > DateTimeOffset.UtcNow)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            TempData["StatusMessage"] = $"{user.Email} unlocked.";
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(5));
            TempData["StatusMessage"] = $"{user.Email} locked for 5 minutes.";
        }

        return RedirectToAction(nameof(Index));
    }
}
