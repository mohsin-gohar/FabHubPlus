using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace FanHubPlus.Models.ViewModels;

// ---- Form models for the Admin create/edit screens ----
// (entities can't bind IFormFile uploads, so forms use dedicated view models)

public class ContentFormViewModel
{
    public int ContentId { get; set; }
    public int CategoryId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public ContentType Type { get; set; } = ContentType.Movie;

    [StringLength(100)]
    public string? Genre { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Release date")]
    [DataType(DataType.Date)]
    public DateTime? ReleaseDate { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Popularity score")]
    public int PopularityScore { get; set; }

    public string? ThumbnailUrl { get; set; }        // current image (kept when no new upload)
    public IFormFile? ThumbnailFile { get; set; }    // optional replacement

    [StringLength(300)]
    [Display(Name = "Tags (comma separated)")]
    public string Tags { get; set; } = string.Empty;

    public List<Category> Categories { get; set; } = new();
}

public class ArticleFormViewModel
{
    public int ArticleId { get; set; }
    public int CategoryId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty; // admin-only rich HTML

    [Display(Name = "Publish date")]
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Timeline story (renders in the Timeline view)")]
    public bool IsTimeline { get; set; }

    public List<Category> Categories { get; set; } = new();
}

public class EventFormViewModel
{
    public int EventId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(-90, 90)] public double Latitude { get; set; }
    [Range(-180, 180)] public double Longitude { get; set; }

    [Display(Name = "Date & time")] public DateTime EventDate { get; set; } = DateTime.UtcNow;

    [StringLength(500), Display(Name = "Ticket URL")]
    public string? TicketUrl { get; set; }

    [StringLength(60)] public string? Type { get; set; }

    [StringLength(4000)] public string? Story { get; set; }
}

public class MerchFormViewModel
{
    public int ItemId { get; set; }
    public int CategoryId { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }

    public MerchTag Tag { get; set; } = MerchTag.None;
    public bool IsUpcoming { get; set; }

    [Display(Name = "Release / drop date")]
    [DataType(DataType.Date)]
    public DateTime? ReleaseDate { get; set; }

    public List<Category> Categories { get; set; } = new();
}

public class CharacterFormViewModel
{
    public int CharacterId { get; set; }
    public int CategoryId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)] public string? Fandom { get; set; }
    [StringLength(2000)] public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }

    public List<Category> Categories { get; set; } = new();
}

public class CategoryFormViewModel
{
    public int CategoryId { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)] public string? Description { get; set; }

    [StringLength(300), Display(Name = "Icon image URL (optional)")]
    public string? IconUrl { get; set; }
}
