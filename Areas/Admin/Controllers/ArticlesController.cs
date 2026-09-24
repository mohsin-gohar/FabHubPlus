using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ArticlesController : Controller
{
    private readonly IRepository<Article> _articles;
    private readonly IRepository<Category> _categories;

    public ArticlesController(IRepository<Article> articles, IRepository<Category> categories)
    {
        _articles = articles;
        _categories = categories;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _articles.Query().Include(a => a.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.ToLower().Contains(search.ToLower()));

        var vm = new AdminListViewModel<Article>
        {
            Search = search,
            Page = Math.Max(1, page),
            BasePath = "/Admin/Articles",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await BuildFormAsync(new ArticleFormViewModel()));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArticleFormViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var userId = await CurrentUserIdAsync();
        _articles.Add(new Article
        {
            CategoryId = model.CategoryId,
            Title = model.Title,
            Body = model.Body,
            PublishedAt = model.PublishedAt,
            IsTimeline = model.IsTimeline,
            AuthorId = userId
        });
        await _articles.SaveChangesAsync();

        TempData["StatusMessage"] = "Article published.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _articles.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return View(await BuildFormAsync(new ArticleFormViewModel
        {
            ArticleId = entity.ArticleId,
            CategoryId = entity.CategoryId,
            Title = entity.Title,
            Body = entity.Body,
            PublishedAt = entity.PublishedAt,
            IsTimeline = entity.IsTimeline
        }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ArticleFormViewModel model)
    {
        if (id != model.ArticleId) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var entity = await _articles.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.CategoryId = model.CategoryId;
        entity.Title = model.Title;
        entity.Body = model.Body;
        entity.PublishedAt = model.PublishedAt;
        entity.IsTimeline = model.IsTimeline;

        _articles.Update(entity);
        await _articles.SaveChangesAsync();

        TempData["StatusMessage"] = "Article updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _articles.GetByIdAsync(id);
        if (entity is null) return NotFound();

        _articles.Remove(entity);
        await _articles.SaveChangesAsync();

        TempData["StatusMessage"] = "Article deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ArticleFormViewModel> BuildFormAsync(ArticleFormViewModel model)
    {
        model.Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
        return model;
    }

    private async Task<string?> CurrentUserIdAsync()
    {
        var mgr = HttpContext.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        return (await mgr.GetUserAsync(User))?.Id;
    }
}
