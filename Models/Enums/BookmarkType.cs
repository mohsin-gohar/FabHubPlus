namespace FanHubPlus.Models.Enums;

// What kind of item a bookmark points to
// (Bookmark.ItemId holds the id of that row in the matching table)
// NOTE: stored as TEXT in the DB (HasConversion<string>), so adding a member
// later does NOT require a schema migration.
public enum BookmarkType
{
    Content,     // row in Contents (Explorer detail page)
    Article,     // row in Articles
    Character,   // row in CharacterProfiles
    Video,       // row in MediaItems
    Merchandise  // row in MerchandiseItems
}
