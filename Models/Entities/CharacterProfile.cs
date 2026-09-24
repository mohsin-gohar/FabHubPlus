using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// A character card (name, fandom, short bio, picture)
public class CharacterProfile
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("CharacterProfileId")
    [Key]
    public int CharacterId { get; set; }

    public int CategoryId { get; set; }                  // FK -> Category

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Fandom { get; set; }                  // e.g. "Naruto"

    [StringLength(2000)]
    public string? Bio { get; set; }                     // short biography

    [StringLength(300)]
    public string? ImageUrl { get; set; }                // character picture

    // Navigation property
    public Category Category { get; set; } = null!;
}
