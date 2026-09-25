using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// Merchandise showcase - DISPLAY ONLY (no cart, no checkout, no payment fields)
public class MerchController : Controller
{
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<Category> _categories;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IStatsService _stats;

    public MerchController(IRepository<MerchandiseItem> merch,
                           IRepository<Category> categories,
                           UserManager<ApplicationUser> userManager,
                           IBookmarkService bookmarks,
                           IStatsService stats)
    {
        _merch = merch;
        _categories = categories;
        _userManager = userManager;
        _bookmarks = bookmarks;
        _stats = stats;
    }

    public async Task<IActionResult> Index(MerchTag? tag, int? categoryId, bool? upcoming)
    {
        var vm = new MerchViewModel
        {
            Tag = tag,
            CategoryId = categoryId,
            Upcoming = upcoming,
            Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        var query = _merch.Query().Include(m => m.Category).AsQueryable();

        if (tag is not null) query = query.Where(m => m.Tag == tag);
        if (categoryId is > 0) query = query.Where(m => m.CategoryId == categoryId);
        if (upcoming is not null) query = query.Where(m => m.IsUpcoming == upcoming);

        vm.Items = await query
            .OrderByDescending(m => m.IsUpcoming) // upcoming first
            .ThenBy(m => m.ReleaseDate)
            .ToListAsync();

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _merch.Query()
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.ItemId == id);

        if (item is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        item.ViewCount++;                 // view tracking for admin stats
        await _merch.SaveChangesAsync();
        await _stats.LogViewAsync("Merchandise", id, user?.Id);

        ViewBag.IsBookmarked = user is not null &&
            await _bookmarks.IsBookmarkedAsync(user.Id, BookmarkType.Merchandise, id);

        var vm = new MerchDetailViewModel
        {
            Item = item,
            IsBookmarked = (bool)ViewBag.IsBookmarked,
            Related = await _merch.Query()
                .Include(m => m.Category)
                .Where(m => m.CategoryId == item.CategoryId && m.ItemId != id)
                .OrderByDescending(m => m.ViewCount)
                .Take(4)
                .ToListAsync(),
            Categories = await _categories.Query()
                .OrderBy(c => c.Name)
                .ToListAsync(),
        };

        return View(vm);
    }
}
