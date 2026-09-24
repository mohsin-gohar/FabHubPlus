using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// One user's 1-5 star rating of a content item (unique per user+content - see DbContext)
public class Rating
{
    public int RatingId { get; set; }

    public string UserId { get; set; } = string.Empty;   // FK -> ApplicationUser
    public int ContentId { get; set; }                   // FK -> Content

    [Range(1, 5)]
    public int Stars { get; set; }                       // 1..5

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Content Content { get; set; } = null!;
}
