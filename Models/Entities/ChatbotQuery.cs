using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// Chat history: every question the visitor/user asked + the answer we gave
public class ChatbotQuery
{
    // [Key] needed: EF only auto-detects "Id" or the FULL type name ("ChatbotQueryId")
    [Key]
    public int QueryId { get; set; }

    public string? UserId { get; set; }                  // null => asked by a visitor

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;  // user's question

    [Required]
    [StringLength(4000)]
    public string Response { get; set; } = string.Empty; // chatbot's answer

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser? User { get; set; }
}
