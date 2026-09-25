using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// News & Timeline index. IsTimeline=null => all, true => timeline stories, false => news posts
public class NewsViewModel
{
    public List<Article> Items { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public bool? IsTimeline { get; set; }
    public int? CategoryId { get; set; }
}

// News detail page
public class ArticleDetailViewModel
{
    public Article Article { get; set; } = null!;
    public List<Article> Related { get; set; } = new();
    public bool IsBookmarked { get; set; }
    public List<Category> Categories { get; set; } = new();
}
