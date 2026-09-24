namespace FanHubPlus.Services;

public interface IBookmarkService
{
    // Adds/removes a polymorphic bookmark. Returns true when the item is NOW bookmarked.
    Task<bool> ToggleAsync(string userId, Models.Enums.BookmarkType type, int itemId, string? note = null);
    Task<bool> IsBookmarkedAsync(string userId, Models.Enums.BookmarkType type, int itemId);
}
