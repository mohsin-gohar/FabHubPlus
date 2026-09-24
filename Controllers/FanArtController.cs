using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// Fan Art gallery: approved submissions public, own submissions listed, submit form
public class FanArtController : Controller
{
    private readonly IRepository<FanSubmission> _submissions;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISupportService _support;
    private readonly IFileUploadService _uploads;

    public FanArtController(IRepository<FanSubmission> submissions,
                            UserManager<ApplicationUser> userManager,
                            ISupportService support,
                            IFileUploadService uploads)
    {
        _submissions = submissions;
        _userManager = userManager;
        _support = support;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new FanArtViewModel
        {
            Approved = await _submissions.Query()
                .Include(s => s.User)
                .Where(s => s.Status == SubmissionStatus.Approved)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync()
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                vm.MySubmissions = await _submissions.Query()
                    .Where(s => s.UserId == user.Id)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();
            }
        }

        return View(vm);
    }

    // POST /FanArt/Submit - goes to PENDING until an admin approves it
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(FanSubmissionFormViewModel form)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            var vm = await RebuildAsync(form);
            return View(nameof(Index), vm);
        }

        string? imageUrl = null;
        if (form.ImageFile is { Length: > 0 })
        {
            try
            {
                imageUrl = await _uploads.SaveImageAsync(form.ImageFile, "fanart");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(form.ImageFile), ex.Message);
                var vm = await RebuildAsync(form);
                return View(nameof(Index), vm);
            }
        }

        await _support.SubmitFanArtAsync(form, user.Id, imageUrl);
        TempData["StatusMessage"] = "Submitted! An admin will review your work shortly.";
        return RedirectToAction(nameof(Index));
    }

    // Helper: gallery data + the form the user just typed (so validation errors keep input)
    private async Task<FanArtViewModel> RebuildAsync(FanSubmissionFormViewModel form)
    {
        var vm = new FanArtViewModel
        {
            Form = form,
            Approved = await _submissions.Query()
                .Include(s => s.User)
                .Where(s => s.Status == SubmissionStatus.Approved)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync()
        };

        var user = await _userManager.GetUserAsync(User);
        if (user is not null)
            vm.MySubmissions = await _submissions.Query()
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        return vm;
    }
}
