using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.Entities;

// A merchandise item - DISPLAY ONLY.
// There is NO payment, cart or checkout (competition rule).
public class MerchandiseItem
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("MerchandiseItemId")
    [Key]
    public int ItemId { get; set; }

    public int CategoryId { get; set; }                  // FK -> Category

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ImageUrl { get; set; }                // product picture

    public MerchTag Tag { get; set; }                    // LimitedEdition / PreOrder / Collectible

    public bool IsUpcoming { get; set; }                 // true => shown in "Upcoming releases"

    public DateTime? ReleaseDate { get; set; }           // drop date for upcoming items

    public int ViewCount { get; set; }                   // view tracking for admin stats

    // Navigation property
    public Category Category { get; set; } = null!;
}
