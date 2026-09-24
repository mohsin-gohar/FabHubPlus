using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Data handed to every view through ViewData for the master navbar/footer
public class LayoutViewModel
{
    public string? CurrentUserDisplayName { get; set; }
    public bool IsAdmin { get; set; }
    public bool DarkMode { get; set; }
    public int FontSize { get; set; } = 16;
    public List<Category> NavCategories { get; set; } = new();
}
