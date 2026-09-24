using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.ViewModels;

// Merchandise showcase (DISPLAY ONLY - no cart, no payment)
public class MerchViewModel
{
    public List<MerchandiseItem> Items { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public MerchTag? Tag { get; set; }
    public int? CategoryId { get; set; }
    public bool? Upcoming { get; set; }
}
