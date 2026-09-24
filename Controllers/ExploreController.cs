using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

/// <summary>
/// Explorer: filterable, sortable, paginated content browser + detail page
/// with AJAX star-rating, bookmark toggle and view logging.
/// </summary>
public class ExploreController : Controller
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IContentService _contentService;
    private readonly IBookmarkService _bookmarkService;
    private readonly IStatsService _stats;

    public ExploreController(IRepository<Content> contents,
                             IRepository<Category> categories,
                             UserManager<ApplicationUser> userManager,
                             IContentService contentService,
                             IBookmarkService bookmarkService,
                             IStatsService stats)
    {
        _contents = contents;
        _categories = categories;
        _userManager = userManager;
        _contentService = contentService;
        _bookmarkService = bookmarkService;
        _stats = stats;
    }

    // GET /Explore?search=&categoryId=&type=&sort=&page=
    public async Task<IActionResult> Index(string? search, int? categoryId,
                                           ContentType? type, string? sort, int page = 1)
    {
        var vm = new ExploreViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Type = type,
            Sort = sort ?? "popular",
            Page = Math.Max(1, page),
            Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        var query = _contents.Query().Include(c => c.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(s) ||
                (c.Genre != null && c.Genre.ToLower().Contains(s)) ||
                (c.Description != null && c.Description.ToLower().Contains(s)));
        }

        if (categoryId is > 0) query = query.Where(c => c.CategoryId == categoryId);
        if (type is not null) query = query.Where(c => c.Type == type);

        query = vm.Sort switch
        {
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            "views" => query.OrderByDescending(c => c.ViewCount),
            "title" => query.OrderBy(c => c.Title),
            _ => query.OrderByDescending(c => c.PopularityScore)
        };

        vm.TotalItems = await query.CountAsync();
        vm.Items = await query
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    // GET /Explore/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var content = await _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (content is null) return NotFound();

        // ---- view tracking (counter + log for the admin charts) ----
        var userId = (await _userManager.GetUserAsync(User))?.Id;
        content.ViewCount++;
        await _contents.SaveChangesAsync();
        await _stats.LogViewAsync("Content", content.ContentId, userId);

        var stars = await _contents.Query()
            .Where(c => c.ContentId == id)
            .SelectMany(c => c.Ratings.Select(r => r.Stars))
            .ToListAsync();

        var vm = new ContentDetailViewModel
        {
            Content = content,
            Media = content.MediaItems.OrderBy(m => m.MediaType).ToList(),
            AverageRating = stars.Count == 0 ? 0 : Math.Round(stars.Average(), 1),
            RatingCount = stars.Count,
            Related = await _contents.Query()
                .Where(c => c.CategoryId == content.CategoryId && c.ContentId != id)
                .OrderByDescending(c => c.PopularityScore)
                .Take(4)
                .ToListAsync()
        };

        if (userId is not null)
        {
            vm.MyStars = await _contents.Query()
                .Where(c => c.ContentId == id)
                .SelectMany(c => c.Ratings)
                .Where(r => r.UserId == userId)
                .Select(r => (int?)r.Stars)
                .FirstOrDefaultAsync();

            vm.IsBookmarked = await _bookmarkService
                .IsBookmarkedAsync(userId, BookmarkType.Content, id);
        }

        return View(vm);
    }

    // POST /Explore/Rate  (AJAX, returns JSON)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int contentId, int stars)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Json(new { ok = false, auth = false, message = "Please log in to rate." });

        try
        {
            var (avg, count, myStars) = await _contentService.RateAsync(user.Id, contentId, stars);
            return Json(new { ok = true, avg, count, myStars });
        }
        catch (ArgumentOutOfRangeException)
        {
            return Json(new { ok = false, message = "Stars must be between 1 and 5." });
        }
    }
}
