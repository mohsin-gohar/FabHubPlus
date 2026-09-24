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
public class CharactersController : Controller
{
    private readonly IRepository<CharacterProfile> _characters;
    private readonly IRepository<Category> _categories;
    private readonly IFileUploadService _uploads;

    public CharactersController(IRepository<CharacterProfile> characters,
                                IRepository<Category> categories,
                                IFileUploadService uploads)
    {
        _characters = characters;
        _categories = categories;
        _uploads = uploads;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _characters.Query().Include(c => c.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

        var vm = new AdminListViewModel<CharacterProfile>
        {
            Search = search,
            Page = Math.Max(1, page),
            BasePath = "/Admin/Characters",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderBy(c => c.Name)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await BuildFormAsync(new CharacterFormViewModel()));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CharacterFormViewModel model)
    {
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var entity = new CharacterProfile
        {
            CategoryId = model.CategoryId,
            Name = model.Name,
            Fandom = model.Fandom,
            Bio = model.Bio,
            ImageUrl = model.ImageUrl
        };

        if (model.ImageFile is { Length: > 0 })
        {
            try { entity.ImageUrl = await _uploads.SaveImageAsync(model.ImageFile, "characters"); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View(await BuildFormAsync(model));
            }
        }

        await _characters.AddAsync(entity);
        await _characters.SaveChangesAsync();

        TempData["StatusMessage"] = "Character created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await _characters.GetByIdAsync(id);
        if (e is null) return NotFound();

        return View(await BuildFormAsync(new CharacterFormViewModel
        {
            CharacterId = e.CharacterId,
            CategoryId = e.CategoryId,
            Name = e.Name,
            Fandom = e.Fandom,
            Bio = e.Bio,
            ImageUrl = e.ImageUrl
        }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CharacterFormViewModel model)
    {
        if (id != model.CharacterId) return BadRequest();
        if (!ModelState.IsValid) return View(await BuildFormAsync(model));

        var e = await _characters.GetByIdAsync(id);
        if (e is null) return NotFound();

        e.CategoryId = model.CategoryId;
        e.Name = model.Name;
        e.Fandom = model.Fandom;
        e.Bio = model.Bio;

        if (model.ImageFile is { Length: > 0 })
        {
            try
            {
                var path = await _uploads.SaveImageAsync(model.ImageFile, "characters");
                _uploads.DeleteImage(e.ImageUrl);
                e.ImageUrl = path;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
                return View(await BuildFormAsync(model));
            }
        }

        _characters.Update(e);
        await _characters.SaveChangesAsync();

        TempData["StatusMessage"] = "Character updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _characters.GetByIdAsync(id);
        if (e is null) return NotFound();

        _uploads.DeleteImage(e.ImageUrl);
        _characters.Remove(e);
        await _characters.SaveChangesAsync();

        TempData["StatusMessage"] = "Character deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CharacterFormViewModel> BuildFormAsync(CharacterFormViewModel model)
    {
        model.Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
        return model;
    }
}
