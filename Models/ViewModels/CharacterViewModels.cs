using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Characters directory (index)
public class CharactersViewModel
{
    public List<CharacterProfile> Items { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
}

// Character detail page
public class CharacterDetailViewModel
{
    public CharacterProfile Character { get; set; } = null!;
    public List<Content> RelatedContent { get; set; } = new();
    public bool IsBookmarked { get; set; }
}
