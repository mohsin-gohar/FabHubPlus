using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContentsController : Controller
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly IContentService _contentService;
    private readonly IFileUploadService _uploads;

    public ContentsController(IRepository<Content> contents,
                              IRepository<Category> categories,
                              IContentService contentService,
                              IFileUploadService uploads)
    {
        _contents = contents;
        _categories = categories;
        _contentService = contentService;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
    {
        var query = _contents.Query().Include(c => c.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.ToLower().Contains(search.ToLower()));
        if (categoryId is > 0)
            query = query.Where(c => c.CategoryId == categoryId);

        var vm = new AdminListViewModel<Content>
        {
            Search = search,
            Page = Math.Max(1, page),
            BasePath = "/Admin/Contents",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        ViewBag.Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
        ViewBag.CategoryId = categoryId;
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await BuildFormAsync(new ContentFormViewModel()));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContentFormViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var entity = new Content
        {
            CategoryId = model.CategoryId,
            Title = model.Title,
            Type = model.Type,
            Genre = model.Genre,
            Description = model.Description,
            ReleaseDate = model.ReleaseDate,
            PopularityScore = model.PopularityScore,
            ThumbnailUrl = model.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow
        };

        if (model.ThumbnailFile is { Length: > 0 })
        {
            try { entity.ThumbnailUrl = await _uploads.SaveImageAsync(model.ThumbnailFile, "content"); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ThumbnailFile), ex.Message);
                return View(await BuildFormAsync(model));
            }
        }

        await _contents.AddAsync(entity);
        await _contents.SaveChangesAsync();

        await _contentService.SyncTagsAsync(entity, model.Tags); // many-to-many tag sync
        TempData["StatusMessage"] = "Content created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _contents.Query()
            .Include(c => c.ContentTags).ThenInclude(ct => ct.Tag)
            .FirstOrDefaultAsync(c => c.ContentId == id);
        if (entity is null) return NotFound();

        var model = new ContentFormViewModel
        {
            ContentId = entity.ContentId,
            CategoryId = entity.CategoryId,
            Title = entity.Title,
            Type = entity.Type,
            Genre = entity.Genre,
            Description = entity.Description,
            ReleaseDate = entity.ReleaseDate,
            PopularityScore = entity.PopularityScore,
            ThumbnailUrl = entity.ThumbnailUrl,
            Tags = string.Join(", ", entity.ContentTags.Select(ct => ct.Tag.Name))
        };

        return View(await BuildFormAsync(model));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContentFormViewModel model)
    {
        if (id != model.ContentId) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var entity = await _contents.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.CategoryId = model.CategoryId;
        entity.Title = model.Title;
        entity.Type = model.Type;
        entity.Genre = model.Genre;
        entity.Description = model.Description;
        entity.ReleaseDate = model.ReleaseDate;
        entity.PopularityScore = model.PopularityScore;

        if (model.ThumbnailFile is { Length: > 0 })
        {
            try
            {
                var path = await _uploads.SaveImageAsync(model.ThumbnailFile, "content");
                _uploads.DeleteImage(entity.ThumbnailUrl);
                entity.ThumbnailUrl = path;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ThumbnailFile), ex.Message);
                return View(await BuildFormAsync(model));
            }
        }

        _contents.Update(entity);
        await _contents.SaveChangesAsync();
        await _contentService.SyncTagsAsync(entity, model.Tags);

        TempData["StatusMessage"] = "Content updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _contents.GetByIdAsync(id);
        if (entity is null) return NotFound();

        _uploads.DeleteImage(entity.ThumbnailUrl);
        _contents.Remove(entity);
        await _contents.SaveChangesAsync();

        TempData["StatusMessage"] = "Content deleted (media rows removed by cascade).";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ContentFormViewModel> BuildFormAsync(ContentFormViewModel model)
    {
        model.Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
        return model;
    }
}
