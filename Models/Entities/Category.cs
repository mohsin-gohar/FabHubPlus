using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// One fandom category (Anime, Gaming, Movies, TV Shows, K-Pop, Comics, Manga, Cosplay)
public class Category
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? IconUrl { get; set; }   // small icon image for the category card

    // Navigation properties (filled by EF Core)
    public ICollection<UserCategory> FanUsers { get; set; } = new List<UserCategory>();
    public ICollection<Content> Contents { get; set; } = new List<Content>();
    public ICollection<CharacterProfile> Characters { get; set; } = new List<CharacterProfile>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<MerchandiseItem> Merchandise { get; set; } = new List<MerchandiseItem>();
}
