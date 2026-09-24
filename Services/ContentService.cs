using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Content domain logic shared by Home, Explorer, detail pages and the Rating AJAX endpoint.
/// </summary>
public class ContentService : IContentService
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Tag> _tags;
    private readonly IRepository<ContentTag> _contentTags;
    private readonly IRepository<Rating> _ratings;
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<Article> _articles;
    private readonly IRepository<EventItem> _events;

    public ContentService(IRepository<Content> contents,
                          IRepository<Tag> tags,
                          IRepository<ContentTag> contentTags,
                          IRepository<Rating> ratings,
                          IRepository<MerchandiseItem> merch,
                          IRepository<Article> articles,
                          IRepository<EventItem> events)
    {
        _contents = contents;
        _tags = tags;
        _contentTags = contentTags;
        _ratings = ratings;
        _merch = merch;
        _articles = articles;
        _events = events;
    }

    // ---- Tag synchronisation: "ninja,space" -> ensure Tag rows exist, replace ContentTags ----
    public async Task SyncTagsAsync(Content content, string? rawTags)
    {
        var desired = (rawTags ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .ToList();

        var existingLinks = await _contentTags.Query()
            .Where(ct => ct.ContentId == content.ContentId)
            .Include(ct => ct.Tag)
            .ToListAsync();

        // Remove links whose tag is no longer requested
        foreach (var link in existingLinks.Where(l => !desired.Contains(l.Tag.Name.ToLowerInvariant())))
            _contentTags.Remove(link);

        // Add links for tags that are requested but not yet linked
        foreach (var name in desired)
        {
            if (existingLinks.Any(l => l.Tag.Name.ToLowerInvariant() == name)) continue;

            var tag = await _tags.Query().FirstOrDefaultAsync(t => t.Name.ToLower() == name);
            if (tag is null)
            {
                tag = new Tag { Name = name };
                await _tags.AddAsync(tag);
                await _tags.SaveChangesAsync(); // get TagId immediately
            }

            await _contentTags.AddAsync(new ContentTag { Content = content, Tag = tag });
        }

        await _contentTags.SaveChangesAsync();
    }

    public Task<List<Content>> GetTrendingAsync(int count)
        => _contents.Query()
            .OrderByDescending(c => c.PopularityScore)
            .ThenByDescending(c => c.ViewCount)
            .Take(count)
            .ToListAsync();

    public Task<List<MerchandiseItem>> GetFeaturedMerchAsync(int count)
        => _merch.Query()
            .Where(m => !m.IsUpcoming)
            .OrderByDescending(m => m.ViewCount)
            .Take(count)
            .ToListAsync();

    public Task<List<Article>> GetLatestArticlesAsync(int count)
        => _articles.Query()
            .Include(a => a.Category)
            .OrderByDescending(a => a.PublishedAt)
            .Take(count)
            .ToListAsync();

    public Task<List<EventItem>> GetUpcomingEventsAsync(int count)
        => _events.Query()
            .Where(e => e.EventDate >= DateTime.UtcNow)
            .OrderBy(e => e.EventDate)
            .Take(count)
            .ToListAsync();

    // ---- Rating upsert (unique user+content enforced by DB + service logic) ----
    public async Task<(double avg, int count, int myStars)> RateAsync(string userId, int contentId, int stars)
    {
        if (stars is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(stars));

        var existing = await _ratings.Query()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ContentId == contentId);

        if (existing is null)
            await _ratings.AddAsync(new Rating { UserId = userId, ContentId = contentId, Stars = stars });
        else
        {
            existing.Stars = stars;
            existing.CreatedAt = DateTime.UtcNow;
            _ratings.Update(existing);
        }
        await _ratings.SaveChangesAsync();

        var all = await _ratings.Query()
            .Where(r => r.ContentId == contentId)
            .Select(r => r.Stars)
            .ToListAsync();

        return (all.Count == 0 ? 0 : Math.Round(all.Average(), 1),
                all.Count,
                stars);
    }
}
