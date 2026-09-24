using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// A fandom event shown on the Leaflet map + calendar
public class EventItem
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("EventItemId")
    [Key]
    public int EventId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;     // calendar filter by city

    public double Latitude { get; set; }                 // map pin position (GPS)
    public double Longitude { get; set; }

    public DateTime EventDate { get; set; }

    [StringLength(500)]
    public string? TicketUrl { get; set; }               // external ticket link (display only)

    [StringLength(60)]
    public string? Type { get; set; }                    // e.g. "Convention", "Online", "Fan Meet"

    [StringLength(4000)]
    public string? Story { get; set; }                   // storytelling highlight text
}
