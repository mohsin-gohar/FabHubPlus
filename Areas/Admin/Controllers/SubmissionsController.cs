using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

// Fan submission moderation queue: approve / reject / delete
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SubmissionsController : Controller
{
    private readonly IRepository<FanSubmission> _submissions;
    private readonly IFileUploadService _uploads;

    public SubmissionsController(IRepository<FanSubmission> submissions,
                                 IFileUploadService uploads)
    {
        _submissions = submissions;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index(SubmissionStatus? status, int page = 1)
    {
        var query = _submissions.Query().Include(s => s.User).AsQueryable();
        if (status is not null) query = query.Where(s => s.Status == status);

        var vm = new AdminListViewModel<FanSubmission>
        {
            Page = Math.Max(1, page),
            BasePath = "/Admin/Submissions",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.PendingCount = await _submissions.Query()
            .CountAsync(s => s.Status == SubmissionStatus.Pending);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, SubmissionStatus status)
    {
        var s = await _submissions.GetByIdAsync(id);
        if (s is null) return NotFound();

        s.Status = status; // Approved => appears instantly in the public gallery
        _submissions.Update(s);
        await _submissions.SaveChangesAsync();

        TempData["StatusMessage"] = $"Submission {status.ToString().ToLower()}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _submissions.GetByIdAsync(id);
        if (s is null) return NotFound();

        _uploads.DeleteImage(s.ImageUrl);
        _submissions.Remove(s);
        await _submissions.SaveChangesAsync();

        TempData["StatusMessage"] = "Submission deleted.";
        return RedirectToAction(nameof(Index));
    }
}
