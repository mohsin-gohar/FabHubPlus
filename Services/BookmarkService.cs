using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Bookmark logic lives in a service because BOTH the detail pages (AJAX toggle)
/// and the My Bookmarks page need the exact same rules (unique user+type+item key).
/// </summary>
public class BookmarkService : IBookmarkService
{
    private readonly IRepository<Bookmark> _bookmarks;

    public BookmarkService(IRepository<Bookmark> bookmarks) => _bookmarks = bookmarks;

    public async Task<bool> ToggleAsync(string userId, BookmarkType type, int itemId, string? note = null)
    {
        var existing = await _bookmarks.Query()
            .FirstOrDefaultAsync(b => b.UserId == userId && b.ItemType == type && b.ItemId == itemId);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(note)) // updating a note on an existing bookmark
            {
                existing.Note = note;
                _bookmarks.Update(existing);
                await _bookmarks.SaveChangesAsync();
                return true;
            }

            _bookmarks.Remove(existing);
            await _bookmarks.SaveChangesAsync();
            return false;
        }

        await _bookmarks.AddAsync(new Bookmark
        {
            UserId = userId,
            ItemType = type,
            ItemId = itemId,
            Note = note
        });
        await _bookmarks.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsBookmarkedAsync(string userId, BookmarkType type, int itemId)
        => await _bookmarks.Query()
            .AnyAsync(b => b.UserId == userId && b.ItemType == type && b.ItemId == itemId);
}
