using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Events page (list + Leaflet map are rendered from the same Items collection)
public class EventsViewModel
{
    public List<EventItem> Items { get; set; } = new();
    public List<string> Cities { get; set; } = new();
    public string? City { get; set; }
    public bool Past { get; set; }   // false = upcoming only (default)
}
