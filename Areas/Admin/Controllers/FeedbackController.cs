using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

// Feedback moderation: filter by status, update workflow status, delete
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FeedbackController : Controller
{
    private readonly IRepository<Feedback> _feedbacks;

    public FeedbackController(IRepository<Feedback> feedbacks) => _feedbacks = feedbacks;

    public async Task<IActionResult> Index(FeedbackStatus? status, int page = 1)
    {
        var query = _feedbacks.Query().Include(f => f.User).AsQueryable();
        if (status is not null) query = query.Where(f => f.Status == status);

        var vm = new AdminListViewModel<Feedback>
        {
            Page = Math.Max(1, page),
            BasePath = "/Admin/Feedback",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        return View(vm);
    }

    // Inline status update from the Index page dropdown
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, FeedbackStatus status)
    {
        var f = await _feedbacks.GetByIdAsync(id);
        if (f is null) return NotFound();

        f.Status = status;
        _feedbacks.Update(f);
        await _feedbacks.SaveChangesAsync();

        TempData["StatusMessage"] = "Feedback status updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var f = await _feedbacks.GetByIdAsync(id);
        if (f is null) return NotFound();

        _feedbacks.Remove(f);
        await _feedbacks.SaveChangesAsync();

        TempData["StatusMessage"] = "Feedback deleted.";
        return RedirectToAction(nameof(Index));
    }
}
