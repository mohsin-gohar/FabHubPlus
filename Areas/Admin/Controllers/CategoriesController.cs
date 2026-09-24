using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IRepository<Category> _categories;

    public CategoriesController(IRepository<Category> categories) => _categories = categories;

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _categories.Query().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

        var vm = new AdminListViewModel<Category>
        {
            Search = search,
            Page = Math.Max(1, page),
            BasePath = "/Admin/Categories",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderBy(c => c.Name)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    [HttpGet] public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _categories.Query().AnyAsync(c => c.Name == model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
            return View(model);
        }

        _categories.Add(new Category
        {
            Name = model.Name,
            Description = model.Description,
            IconUrl = model.IconUrl
        });
        await _categories.SaveChangesAsync();

        TempData["StatusMessage"] = "Category created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _categories.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return View(new CategoryFormViewModel
        {
            CategoryId = entity.CategoryId,
            Name = entity.Name,
            Description = entity.Description,
            IconUrl = entity.IconUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (id != model.CategoryId) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var entity = await _categories.GetByIdAsync(id);
        if (entity is null) return NotFound();

        if (await _categories.Query().AnyAsync(c => c.Name == model.Name && c.CategoryId != id))
        {
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
            return View(model);
        }

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.IconUrl = model.IconUrl;
        _categories.Update(entity);
        await _categories.SaveChangesAsync();

        TempData["StatusMessage"] = "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    // Deleting is blocked by the database when the category still has content
    // (DeleteBehavior.Restrict) - we surface a friendly message instead of a 500.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _categories.GetByIdAsync(id);
        if (entity is null) return NotFound();

        try
        {
            _categories.Remove(entity);
            await _categories.SaveChangesAsync();
            TempData["StatusMessage"] = "Category deleted.";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["StatusError"] =
                "This category still has content/characters/articles/merchandise linked to it. Move them first.";
        }

        return RedirectToAction(nameof(Index));
    }
}
