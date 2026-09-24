using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

// Events: calendar list + Leaflet map pins, filterable by city and past/upcoming
public class EventsController : Controller
{
    private readonly IRepository<EventItem> _events;

    public EventsController(IRepository<EventItem> events) => _events = events;

    public async Task<IActionResult> Index(string? city, bool past = false)
    {
        var query = _events.Query().AsQueryable();
        query = past
            ? query.Where(e => e.EventDate < DateTime.UtcNow)
            : query.Where(e => e.EventDate >= DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(e => e.City == city);

        var items = await query.OrderBy(e => e.EventDate).ToListAsync();

        var vm = new EventsViewModel
        {
            Items = items,
            City = city,
            Past = past,
            Cities = await _events.Query()
                .Select(e => e.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync()
        };

        return View(vm);
    }

    // JSON feed consumed by the Leaflet map on the same page
    public async Task<IActionResult> Feed()
    {
        var rows = await _events.Query()
            .Where(e => e.EventDate >= DateTime.UtcNow)
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                title = e.Title,
                city = e.City,
                date = e.EventDate,
                lat = e.Latitude,
                lng = e.Longitude
            })
            .ToListAsync();

        // Date formatting happens client-side (EF cannot translate DateTime.ToString formats)
        var data = rows.Select(r => new
        {
            r.title, r.city, r.lat, r.lng,
            date = r.date.ToString("yyyy-MM-dd")
        });

        return Json(data);
    }
}
