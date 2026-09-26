using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

/// <summary>
/// A "watch it right here" card: a content item plus the media row that plays
/// for it. The URL/kind are pre-resolved so the view and the modal JavaScript
/// never have to re-parse the embed link.
/// </summary>
public class TrailerViewModel
{
    public Content Content { get; set; } = null!;
    public MediaItem Media { get; set; } = null!;

    public string Url { get; set; } = string.Empty;   // playable URL (local file or embed)
    public string Kind { get; set; } = "embed";       // "local" -> <video>, "embed" -> <iframe>
    public string Poster { get; set; } = string.Empty;// still frame / cover art
    public string Label { get; set; } = "Trailer";    // chip text ("Official Trailer", ...)
}