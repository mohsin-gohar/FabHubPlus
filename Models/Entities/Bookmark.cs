using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.Entities;

// A user's saved item with an optional private note.
// Polymorphic: ItemId points to an Article / Character / Video / Merchandise row -
// we know WHICH table by looking at ItemType.
public class Bookmark
{
    public int BookmarkId { get; set; }

    public string UserId { get; set; } = string.Empty;   // FK -> ApplicationUser

    public BookmarkType ItemType { get; set; }           // which table ItemId points to

    public int ItemId { get; set; }                      // id of the bookmarked row

    [StringLength(1000)]
    public string? Note { get; set; }                    // personal note

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
