using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
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

    // Rating: one user rates one content once; returns (avg, count, my stars)
    Task<(double avg, int count, int myStars)> RateAsync(string userId, int contentId, int stars);
}
