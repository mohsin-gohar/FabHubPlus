namespace FanHubPlus.Models.Entities;

// Join table: many-to-many between Content and Tag (composite key configured in DbContext)
public class ContentTag
{
    public int ContentId { get; set; }
    public int TagId { get; set; }

    public Content Content { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
