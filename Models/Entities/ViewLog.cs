using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// Records every detail-page view => powers the admin usage stats.
// Polymorphic: ItemType says which table ("Content", "Merchandise", "Article"...)
// and ItemId is the id of the viewed row.
public class ViewLog
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("ViewLogId")
    [Key]
    public int LogId { get; set; }

    [Required]
    [StringLength(50)]
    public string ItemType { get; set; } = string.Empty;

    public int ItemId { get; set; }                      // id of the viewed row

    public string? UserId { get; set; }                  // null => viewer was a visitor

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser? User { get; set; }
}
