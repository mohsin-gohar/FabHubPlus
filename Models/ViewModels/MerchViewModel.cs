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

// Merchandise detail: the product, its siblings and the fandom rail.
public class MerchDetailViewModel
{
    public MerchandiseItem Item { get; set; } = null!;
    public List<MerchandiseItem> Related { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public bool IsBookmarked { get; set; }
}
