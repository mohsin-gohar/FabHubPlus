namespace FanHubPlus.Models.ViewModels;

// Footer / XML sitemap: hierarchical link list generated from the live database
public class SitemapViewModel
{
    public List<SitemapSection> Sections { get; set; } = new();
}

public class SitemapSection
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<SitemapLink> Children { get; set; } = new();
}

public class SitemapLink
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
