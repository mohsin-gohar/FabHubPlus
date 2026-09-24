using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.ViewModels;

// Admin > Contents list with filters + paged results (generic pager for every admin list)
public class AdminListViewModel<T>
{
    public List<T> Items { get; set; } = new();
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public string BasePath { get; set; } = "/Admin/Contents";

    public string PageUrl(int page)
    {
        var q = new List<string> { $"page={page}" };
        if (!string.IsNullOrWhiteSpace(Search)) q.Add($"search={Uri.EscapeDataString(Search)}");
        return BasePath + "?" + string.Join("&", q);
    }
}

// Admin > Users list row (user + their roles for display)
public class AdminUserRowViewModel
{
    public Models.Entities.ApplicationUser User { get; set; } = null!;
    public IList<string> Roles { get; set; } = new List<string>();
}
