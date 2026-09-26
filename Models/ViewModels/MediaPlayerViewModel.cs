namespace FanHubPlus.Models.ViewModels;

/// <summary>
/// Everything the shared player partial needs to render EITHER a self-hosted
/// video file (&lt;video controls&gt;) or an external embed (&lt;iframe&gt;),
/// so a trailer always plays INSIDE the site.
/// </summary>
public class MediaPlayerViewModel
{
    public string Url { get; set; } = string.Empty;  // local file path OR embed link
    public string? Poster { get; set; }              // still frame shown before playback
    public string Title { get; set; } = string.Empty; // accessible name
    public bool Autoplay { get; set; }
    public bool Loop { get; set; }
    public string? CssClass { get; set; }            // extra classes on the wrapper
}