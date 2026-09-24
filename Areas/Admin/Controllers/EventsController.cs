using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EventsController : Controller
{
    private readonly IRepository<EventItem> _events;

    public EventsController(IRepository<EventItem> events) => _events = events;

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _events.Query().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e =>
                e.Title.ToLower().Contains(search.ToLower()) ||
                e.City.ToLower().Contains(search.ToLower()));

        var vm = new AdminListViewModel<EventItem>
        {
            Search = search,
            Page = Math.Max(1, page),
            BasePath = "/Admin/Events",
            TotalItems = await query.CountAsync()
        };
        vm.Items = await query
            .OrderBy(e => e.EventDate)
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    [HttpGet] public IActionResult Create() => View(new EventFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _events.Add(new EventItem
        {
            Title = model.Title,
            City = model.City,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            EventDate = model.EventDate,
            TicketUrl = model.TicketUrl,
            Type = model.Type,
            Story = model.Story
        });
        await _events.SaveChangesAsync();

        TempData["StatusMessage"] = "Event created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await _events.GetByIdAsync(id);
        if (e is null) return NotFound();

        return View(new EventFormViewModel
        {
            EventId = e.EventId,
            Title = e.Title,
            City = e.City,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            EventDate = e.EventDate,
            TicketUrl = e.TicketUrl,
            Type = e.Type,
            Story = e.Story
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (id != model.EventId) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var e = await _events.GetByIdAsync(id);
        if (e is null) return NotFound();

        e.Title = model.Title;
        e.City = model.City;
        e.Latitude = model.Latitude;
        e.Longitude = model.Longitude;
        e.EventDate = model.EventDate;
        e.TicketUrl = model.TicketUrl;
        e.Type = model.Type;
        e.Story = model.Story;

        _events.Update(e);
        await _events.SaveChangesAsync();

        TempData["StatusMessage"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _events.GetByIdAsync(id);
        if (e is null) return NotFound();

        _events.Remove(e);
        await _events.SaveChangesAsync();

        TempData["StatusMessage"] = "Event deleted.";
        return RedirectToAction(nameof(Index));
    }
}
