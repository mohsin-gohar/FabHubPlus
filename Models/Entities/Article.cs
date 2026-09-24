using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// A featured article written by an admin (rich text or timeline story)
public class Article
{
    public int ArticleId { get; set; }

    public int CategoryId { get; set; }                  // FK -> Category

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;     // rich HTML text from the editor
                                                          // (only admins can write => trusted input)
    public string? AuthorId { get; set; }                // FK -> ApplicationUser (nullable)

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public bool IsTimeline { get; set; }                 // true => render as a timeline story

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ApplicationUser? Author { get; set; }
}
