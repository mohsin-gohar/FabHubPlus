using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Services;

/// <summary>
/// One place that decides HOW a media row should be played:
///  - a self-hosted file (.mp4/.webm/.ogg/.m4v, or any site-relative path)
///    => rendered with the HTML5 &lt;video&gt; element, no external service,
///  - an external player (YouTube / Vimeo) => rendered in an &lt;iframe&gt;,
///    with the URL normalised to its /embed/ form (a plain "watch?v=" link
///    cannot be embedded, so it is converted first).
/// </summary>
public static class MediaUrl
{
    private static readonly string[] LocalExtensions =
        { ".mp4", ".webm", ".ogv", ".ogg", ".m4v", ".mov" };

    /// <summary>A media row that can actually be played (video or trailer, with a URL).</summary>
    public static bool IsPlayable(MediaItem? media)
        => media is not null
           && (media.MediaType == MediaType.Video || media.MediaType == MediaType.Trailer)
           && !string.IsNullOrWhiteSpace(media.EmbedUrl);

    /// <summary>The row shown in the big player: an explicit trailer wins over a generic video.</summary>
    public static MediaItem? Primary(IEnumerable<MediaItem>? media)
    {
        if (media is null) return null;

        var playable = media.Where(IsPlayable).ToList();
        return playable.FirstOrDefault(m => m.MediaType == MediaType.Trailer)
               ?? playable.FirstOrDefault();
    }

    /// <summary>True when the browser can play the URL itself instead of loading an iframe.</summary>
    public static bool IsLocalFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        var value = url.Trim();
        if (value.StartsWith('/')) return true;                       // /assets/videos/movie.mp4

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return LocalExtensions.Contains(Path.GetExtension(uri.AbsolutePath),
                                            StringComparer.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <summary>"local" (playable with &lt;video&gt;) or "embed" (needs an &lt;iframe&gt;).</summary>
    public static string Kind(string? url) => IsLocalFile(url) ? "local" : "embed";

    /// <summary>Turns any provider link into one that is safe to drop into an iframe.</summary>
    public static string Normalize(string? url)
    {
        var value = (url ?? string.Empty).Trim();
        if (value.Length == 0) return value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return value; // already a local path

        // youtube.com/watch?v=ID  /  youtu.be/ID  /  youtube.com/shorts/ID
        var isYouTube = uri.Host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase)
                     || uri.Host.EndsWith("youtu.be", StringComparison.OrdinalIgnoreCase)
                     || uri.Host.EndsWith("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase);

        if (isYouTube)
        {
            var host = "www.youtube.com";

            // Pull the video id out of ?v=, of a youtu.be short link or of /shorts/
            string? id = null;
            foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2 && parts[0].Equals("v", StringComparison.OrdinalIgnoreCase))
                {
                    id = Uri.UnescapeDataString(parts[1]);
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(id) && uri.Host.EndsWith("youtu.be", StringComparison.OrdinalIgnoreCase))
                id = uri.AbsolutePath.Trim('/');
            if (string.IsNullOrWhiteSpace(id) && uri.AbsolutePath.Contains("/shorts/"))
                id = uri.AbsolutePath.Split('/').LastOrDefault();

            if (!string.IsNullOrWhiteSpace(id))
                return $"https://{host}/embed/{id}?rel=0";
        }

        // vimeo.com/123456  ->  player.vimeo.com/video/123456
        if (uri.Host.EndsWith("vimeo.com", StringComparison.OrdinalIgnoreCase))
        {
            var id = uri.AbsolutePath.Trim('/');
            if (id.Length > 0) return $"https://player.vimeo.com/video/{id}";
        }

        return value;
    }

    /// <summary>Adds autoplay=1 to an embed URL (only meaningful for iframe players).</summary>
    public static string WithAutoplay(string url)
        => url.Contains('?') ? $"{url}&autoplay=1" : $"{url}?autoplay=1";
}