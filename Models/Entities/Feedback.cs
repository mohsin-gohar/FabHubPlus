using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.Entities;

// Feedback from users: bug / suggestion / query.
// UserId is nullable => even a visitor can send feedback.
public class Feedback
{
    public int FeedbackId { get; set; }

    public string? UserId { get; set; }                  // FK -> ApplicationUser (nullable)

    public FeedbackType Type { get; set; }               // Bug / Suggestion / Query

    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser? User { get; set; }
}
