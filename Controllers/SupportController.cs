using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// Support: FAQ accordion + feedback form (open to visitors - no login required)
public class SupportController : Controller
{
    private readonly IRepository<ChatFaq> _faqs;
    private readonly ISupportService _support;
    private readonly UserManager<ApplicationUser> _userManager;

    public SupportController(IRepository<ChatFaq> faqs,
                             ISupportService support,
                             UserManager<ApplicationUser> userManager)
    {
        _faqs = faqs;
        _support = support;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new SupportViewModel
        {
            Faqs = await _faqs.Query()
                .OrderBy(f => f.Question)
                .Select(f => new FaqItemViewModel
                {
                    FaqId = f.FaqId,
                    Question = f.Question,
                    Answer = f.Answer
                })
                .ToListAsync()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Feedback(FeedbackFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            var vm = await RebuildAsync(form);
            return View(nameof(Index), vm);
        }

        var user = await _userManager.GetUserAsync(User); // null => visitor feedback
        await _support.SaveFeedbackAsync(form, user);

        var done = await RebuildAsync(null);
        done.FeedbackSent = true;
        return View(nameof(Index), done);
    }

    private async Task<SupportViewModel> RebuildAsync(FeedbackFormViewModel? form)
    {
        return new SupportViewModel
        {
            FeedbackForm = form ?? new FeedbackFormViewModel(),
            Faqs = await _faqs.Query()
                .OrderBy(f => f.Question)
                .Select(f => new FaqItemViewModel
                {
                    FaqId = f.FaqId,
                    Question = f.Question,
                    Answer = f.Answer
                })
                .ToListAsync()
        };
    }
}
