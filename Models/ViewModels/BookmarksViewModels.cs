using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.ViewModels;

// My Bookmarks page: polymorphic bookmarks resolved to real titles + links
public class BookmarkGroupViewModel
{
    public BookmarkType Type { get; set; }
    public List<BookmarkEntryViewModel> Items { get; set; } = new();
}

public class BookmarkEntryViewModel
{
    public int BookmarkId { get; set; }
    public int ItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }   // category / fandom / author
    public string LinkUrl { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MyBookmarksViewModel
{
    public List<BookmarkGroupViewModel> Groups { get; set; } = new();
}
