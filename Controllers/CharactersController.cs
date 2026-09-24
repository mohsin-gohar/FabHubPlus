using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// Characters directory: search + category filter + detail page with related content
public class CharactersController : Controller
{
    private readonly IRepository<CharacterProfile> _characters;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Content> _contents;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IStatsService _stats;

    public CharactersController(IRepository<CharacterProfile> characters,
                                IRepository<Category> categories,
                                IRepository<Content> contents,
                                UserManager<ApplicationUser> userManager,
                                IBookmarkService bookmarks,
                                IStatsService stats)
    {
        _characters = characters;
        _categories = categories;
        _contents = contents;
        _userManager = userManager;
        _bookmarks = bookmarks;
        _stats = stats;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var vm = new CharactersViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        var query = _characters.Query().Include(c => c.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.Fandom != null && c.Fandom.ToLower().Contains(s)) ||
                (c.Bio != null && c.Bio.ToLower().Contains(s)));
        }
        if (categoryId is > 0) query = query.Where(c => c.CategoryId == categoryId);

        vm.Items = await query.OrderBy(c => c.Name).ToListAsync();
        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var character = await _characters.Query()
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.CharacterId == id);

        if (character is null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        await _stats.LogViewAsync("Character", id, user?.Id);

        var vm = new CharacterDetailViewModel
        {
            Character = character,
            RelatedContent = await _contents.Query()
                .Where(c => c.CategoryId == character.CategoryId)
                .OrderByDescending(c => c.PopularityScore)
                .Take(4)
                .ToListAsync(),
            IsBookmarked = user is not null &&
                           await _bookmarks.IsBookmarkedAsync(user.Id, BookmarkType.Character, id)
        };

        return View(vm);
    }
}
