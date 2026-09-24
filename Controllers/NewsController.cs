using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// News (IsTimeline=false) + Timeline storytelling view (IsTimeline=true) in one module
public class NewsController : Controller
{
    private readonly IRepository<Article> _articles;
    private readonly IRepository<Category> _categories;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IStatsService _stats;

    public NewsController(IRepository<Article> articles,
                          IRepository<Category> categories,
                          UserManager<ApplicationUser> userManager,
                          IBookmarkService bookmarks,
                          IStatsService stats)
    {
        _articles = articles;
        _categories = categories;
        _userManager = userManager;
        _bookmarks = bookmarks;
        _stats = stats;
    }

    // GET /News?timeline=true|false&categoryId=
    public async Task<IActionResult> Index(bool? timeline, int? categoryId)
    {
        var vm = new NewsViewModel
        {
            IsTimeline = timeline,
            CategoryId = categoryId,
            Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        var query = _articles.Query()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .AsQueryable();

        if (timeline is not null) query = query.Where(a => a.IsTimeline == timeline);
        if (categoryId is > 0) query = query.Where(a => a.CategoryId == categoryId);

        vm.Items = await query.OrderByDescending(a => a.PublishedAt).ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var article = await _articles.Query()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.ArticleId == id);

        if (article is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        await _stats.LogViewAsync("Article", id, user?.Id);

        var vm = new ArticleDetailViewModel
        {
            Article = article,
            Related = await _articles.Query()
                .Where(a => a.CategoryId == article.CategoryId && a.ArticleId != id)
                .OrderByDescending(a => a.PublishedAt)
                .Take(3)
                .ToListAsync(),
            IsBookmarked = user is not null &&
                           await _bookmarks.IsBookmarkedAsync(user.Id, BookmarkType.Article, id)
        };

        return View(vm);
    }
}
