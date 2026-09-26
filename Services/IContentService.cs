using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;

namespace FanHubPlus.Services;

public interface IContentService
{
    // Applies a comma-separated tag list to a content item (creates missing tags - many-to-many sync)
    Task SyncTagsAsync(Content content, string? rawTags);

    // Shared "featured" queries used by Home + Explore
    Task<List<Content>> GetTrendingAsync(int count);
    Task<List<MerchandiseItem>> GetFeaturedMerchAsync(int count);
    Task<List<Article>> GetLatestArticlesAsync(int count);
    Task<List<EventItem>> GetUpcomingEventsAsync(int count);

    // Titles that have something to play (self-hosted file or external embed),
    // ready to be dropped into the in-page video modal on the landing page.
    Task<List<TrailerViewModel>> GetTrailersAsync(int count);

    // Rating: one user rates one content once; returns (avg, count, my stars)
    Task<(double avg, int count, int myStars)> RateAsync(string userId, int contentId, int stars);
}
