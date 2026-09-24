using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// One FAQ row used by our chatbot to answer questions
public class ChatFaq
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("ChatFaqId")
    [Key]
    public int FaqId { get; set; }

    [Required]
    [StringLength(300)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Answer { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Keywords { get; set; }   // comma-separated words matched against the user message
}
