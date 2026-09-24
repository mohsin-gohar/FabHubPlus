using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.Entities;

// Fan-made content submitted by a user - an admin must approve/reject it
public class FanSubmission
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("FanSubmissionId")
    [Key]
    public int SubmissionId { get; set; }

    public string UserId { get; set; } = string.Empty;   // FK -> ApplicationUser

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(8000)]
    public string Body { get; set; } = string.Empty;     // the fan article / post text

    [StringLength(300)]
    public string? ImageUrl { get; set; }                // optional fan-art picture

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
