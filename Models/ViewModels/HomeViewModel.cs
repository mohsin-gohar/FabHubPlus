using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Everything the landing page needs in ONE query-set (avoids N+1 queries in the view)
public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Content> Trending { get; set; } = new();
    public List<Article> LatestArticles { get; set; } = new();
    public List<EventItem> UpcomingEvents { get; set; } = new();
    public List<MerchandiseItem> FeaturedMerch { get; set; } = new();

    // Trailers / videos that can be played straight on the landing page
    public List<TrailerViewModel> Trailers { get; set; } = new();

    // Hero counters
    public int TotalContents { get; set; }
    public int TotalMembers { get; set; }
    public int TotalEvents { get; set; }
}
