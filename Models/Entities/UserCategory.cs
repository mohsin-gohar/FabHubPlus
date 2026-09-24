namespace FanHubPlus.Models.Entities;

// Join table for the many-to-many relation "favorite fandoms":
// one user likes many categories, one category is liked by many users.
// Primary key = UserId + CategoryId together (configured in ApplicationDbContext).
public class UserCategory
{
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
