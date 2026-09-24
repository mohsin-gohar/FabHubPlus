using System.Diagnostics;
using FanHubPlus.Models;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

public class HomeController : Controller
{
    private readonly IStatsService _stats;
    private readonly IContentService _content;
    private readonly IRepository<FanHubPlus.Models.Entities.Category> _categories;

    public HomeController(IStatsService stats,
                          IContentService content,
                          IRepository<FanHubPlus.Models.Entities.Category> categories)
    {
        _stats = stats;
        _content = content;
        _categories = categories;
    }

    // Landing page: hero counters + trending content + latest news + upcoming events + merch
    public async Task<IActionResult> Index()
    {
        var (contents, members, events) = await _stats.GetPortalTotalsAsync();

        var vm = new HomeViewModel
        {
            Categories = await _categories.Query()
                .OrderBy(c => c.Name).ToListAsync(),
            Trending = await _content.GetTrendingAsync(6),
            LatestArticles = await _content.GetLatestArticlesAsync(3),
            UpcomingEvents = await _content.GetUpcomingEventsAsync(3),
            FeaturedMerch = await _content.GetFeaturedMerchAsync(4),
            TotalContents = contents,
            TotalMembers = members,
            TotalEvents = events
        };

        return View(vm);
    }

    // Shared error page (exception handler + 404/500 status-code re-execution)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        var feature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        ViewBag.StatusCode = statusCode;
        ViewBag.ErrorPath = feature?.Path;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

