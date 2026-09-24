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
public class MerchController : Controller
{
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<Category> _categories;
    private readonly IFileUploadService _uploads;

    public MerchController(IRepository<MerchandiseItem> merch,
                           IRepository<Category> categories,
                           IFileUploadService uploads)
    {
        _merch = merch;
        _categories = categories;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _merch.Query().Include(m => m.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.ToLower().Contains(search.ToLower()));

        var vm = new AdminListViewModel<MerchandiseItem>
        {
            Search = search,
            Page = Math.Max(1, page),
            BasePath = "/Admin/Merch",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderByDescending(m => m.IsUpcoming)
            .ThenBy(m => m.Name)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await BuildFormAsync(new MerchFormViewModel()));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MerchFormViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var entity = new MerchandiseItem
        {
            CategoryId = model.CategoryId,
            Name = model.Name,
            ImageUrl = model.ImageUrl,
            Tag = model.Tag,
            IsUpcoming = model.IsUpcoming,
            ReleaseDate = model.ReleaseDate
        };

        if (model.ImageFile is { Length: > 0 })
        {
            try { entity.ImageUrl = await _uploads.SaveImageAsync(model.ImageFile, "merch"); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View(await BuildFormAsync(model));
            }
        }

        await _merch.AddAsync(entity);
        await _merch.SaveChangesAsync();

        TempData["StatusMessage"] = "Merchandise item created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await _merch.GetByIdAsync(id);
        if (e is null) return NotFound();

        return View(await BuildFormAsync(new MerchFormViewModel
        {
            ItemId = e.ItemId,
            CategoryId = e.CategoryId,
            Name = e.Name,
            ImageUrl = e.ImageUrl,
            Tag = e.Tag,
            IsUpcoming = e.IsUpcoming,
            ReleaseDate = e.ReleaseDate
        }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MerchFormViewModel model)
    {
        if (id != model.ItemId) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var e = await _merch.GetByIdAsync(id);
        if (e is null) return NotFound();

        e.CategoryId = model.CategoryId;
        e.Name = model.Name;
        e.Tag = model.Tag;
        e.IsUpcoming = model.IsUpcoming;
        e.ReleaseDate = model.ReleaseDate;

        if (model.ImageFile is { Length: > 0 })
        {
            try
            {
                var path = await _uploads.SaveImageAsync(model.ImageFile, "merch");
                _uploads.DeleteImage(e.ImageUrl);
                e.ImageUrl = path;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View(await BuildFormAsync(model));
            }
        }

        _merch.Update(e);
        await _merch.SaveChangesAsync();

        TempData["StatusMessage"] = "Merchandise item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _merch.GetByIdAsync(id);
        if (e is null) return NotFound();

        _uploads.DeleteImage(e.ImageUrl);
        _merch.Remove(e);
        await _merch.SaveChangesAsync();

        TempData["StatusMessage"] = "Merchandise item deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<MerchFormViewModel> BuildFormAsync(MerchFormViewModel model)
    {
        model.Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
        return model;
    }
}
