using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.Entities;

// One embedded media file attached to a content item (trailer, video, audio)
public class MediaItem
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("MediaItemId")
    [Key]
    public int MediaId { get; set; }

    public int ContentId { get; set; }                   // FK -> Content

    public MediaType MediaType { get; set; }             // Video / Audio / Trailer

    [Required]
    [StringLength(500)]
    public string EmbedUrl { get; set; } = string.Empty; // YouTube / Spotify embed link

    [StringLength(100)]
    public string? Tag { get; set; }                     // admin tag, e.g. "Official Trailer 2"

    // Navigation property
    public Content Content { get; set; } = null!;
}
