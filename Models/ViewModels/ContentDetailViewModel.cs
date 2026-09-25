using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Content detail page: main item + media embeds + rating state + related row
public class ContentDetailViewModel
{
    public Content Content { get; set; } = null!;
    public List<MediaItem> Media { get; set; } = new();
    public List<Content> Related { get; set; } = new();

    // Fandom list for the sticky sidebar (read-only navigation aid)
    public List<Category> Categories { get; set; } = new();

    // Rating summary
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? MyStars { get; set; }          // current user's rating (null = not rated)
    public bool IsBookmarked { get; set; }
}
