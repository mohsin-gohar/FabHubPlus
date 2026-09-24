using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.ViewModels;

// Explorer: combined filter + paged result set
public class ExploreViewModel
{
    // ---- filters (bound from the query string) ----
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public ContentType? Type { get; set; }
    public string Sort { get; set; } = "popular"; // popular | newest | views | title
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    // ---- results ----
    public List<Content> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

    // ---- filter dropdown data ----
    public List<Category> Categories { get; set; } = new();

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    // Builds /Explore?... preserving every active filter when paging
    public string PageUrl(int page)
    {
        var q = new List<string> { $"page={page}" };
        if (!string.IsNullOrWhiteSpace(Search)) q.Add($"search={Uri.EscapeDataString(Search)}");
        if (CategoryId is > 0) q.Add($"categoryId={CategoryId}");
        if (Type is not null) q.Add($"type={Type}");
        q.Add($"sort={Sort}");
        return "?" + string.Join("&", q);
    }
}
