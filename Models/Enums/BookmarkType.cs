namespace FanHubPlus.Models.Enums;

// What kind of item a bookmark points to
// (Bookmark.ItemId holds the id of that row in the matching table)
public enum BookmarkType
{
    Article,    // row in Articles
    Character,  // row in CharacterProfiles
    Video,      // row in MediaItems
    Merchandise // row in MerchandiseItems
}
