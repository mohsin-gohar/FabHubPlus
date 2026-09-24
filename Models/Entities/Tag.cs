using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// A reusable keyword tag for content (e.g. "Ninja", "Space", "Romance")
public class Tag
{
    public int TagId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();
}
