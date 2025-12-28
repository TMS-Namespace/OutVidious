using System.Text.RegularExpressions;
using System.Web;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore;

/// <summary>
/// Utility for building and parsing canonical YouTube URLs.
/// </summary>
public static partial class YouTubeUrlBuilder
{
    private const string YouTubeVideoBaseUrl = "https://www.youtube.com/watch?v=";
    private const string YouTubeChannelBaseUrl = "https://www.youtube.com/channel/";

    /// <summary>
    /// Builds a canonical YouTube video URL from a video ID.
    /// </summary>
    /// <param name="videoId">The video ID (11 characters).</param>
    /// <returns>The canonical absolute remote URL.</returns>
    public static Uri BuildVideoUrl(string videoId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        return new Uri($"{YouTubeVideoBaseUrl}{videoId}");
    }

    /// <summary>
    /// Builds a canonical YouTube channel URL from a channel ID.
    /// </summary>
    /// <param name="channelId">The channel ID (UC...) or handle (@...).</param>
    /// <returns>The canonical absolute remote URL.</returns>
    public static Uri BuildChannelUrl(string channelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);

        // If it's a handle, use /@handle format
        if (channelId.StartsWith('@'))
        {
            return new Uri($"https://www.youtube.com/{channelId}");
        }

        return new Uri($"{YouTubeChannelBaseUrl}{channelId}");
    }

    /// <summary>
    /// Extracts the video ID from a YouTube video URL.
    /// Supports various URL formats: youtube.com/watch?v=, youtu.be/, youtube.com/embed/, etc.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The video URL as string.</param>
    /// <returns>The video ID or null if not found.</returns>
    public static string? ExtractVideoId(string absoluteRemoteUrl)
    {
        if (string.IsNullOrWhiteSpace(absoluteRemoteUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(absoluteRemoteUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return ExtractVideoId(uri);
    }

    /// <summary>
    /// Extracts the video ID from a YouTube video URL.
    /// Supports various URL formats: youtube.com/watch?v=, youtu.be/, youtube.com/embed/, etc.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The video URL.</param>
    /// <returns>The video ID or null if not found.</returns>
    public static string? ExtractVideoId(Uri absoluteRemoteUrl)
    {
        ArgumentNullException.ThrowIfNull(absoluteRemoteUrl);

        var url = absoluteRemoteUrl.ToString();

        // Try ?v= parameter first (most common)
        var query = HttpUtility.ParseQueryString(absoluteRemoteUrl.Query);
        var videoId = query["v"];
        if (!string.IsNullOrEmpty(videoId) && VideoIdRegex().IsMatch(videoId))
        {
            return videoId;
        }

        // Try youtu.be short URL
        if (absoluteRemoteUrl.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            var path = absoluteRemoteUrl.AbsolutePath.TrimStart('/');
            if (VideoIdRegex().IsMatch(path))
            {
                return path;
            }
        }

        // Try /embed/ or /v/ path
        var embedMatch = EmbedVideoIdRegex().Match(url);
        if (embedMatch.Success)
        {
            return embedMatch.Groups[1].Value;
        }

        return null;
    }

    /// <summary>
    /// Extracts the channel ID from a YouTube channel URL.
    /// Supports /channel/UC..., /@handle, /c/name, /user/name formats.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The channel URL as string.</param>
    /// <returns>The channel ID/handle or null if not found.</returns>
    public static string? ExtractChannelRemoteId(string absoluteRemoteUrl)
    {
        if (string.IsNullOrWhiteSpace(absoluteRemoteUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(absoluteRemoteUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return ExtractChannelId(uri);
    }

    /// <summary>
    /// Extracts the channel ID from a YouTube channel URL.
    /// Supports /channel/UC..., /@handle, /c/name, /user/name formats.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The channel URL.</param>
    /// <returns>The channel ID/handle or null if not found.</returns>
    public static string? ExtractChannelId(Uri absoluteRemoteUrl)
    {
        ArgumentNullException.ThrowIfNull(absoluteRemoteUrl);

        var path = absoluteRemoteUrl.AbsolutePath;

        // /channel/UC... format (canonical)
        var channelMatch = ChannelIdRegex().Match(path);
        if (channelMatch.Success)
        {
            return channelMatch.Groups[1].Value;
        }

        // /@handle format
        var handleMatch = HandleRegex().Match(path);
        if (handleMatch.Success)
        {
            return handleMatch.Groups[1].Value;
        }

        // /c/name or /user/name format (legacy)
        var legacyMatch = LegacyChannelRegex().Match(path);
        if (legacyMatch.Success)
        {
            return legacyMatch.Groups[1].Value;
        }

        return null;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{11}$")]
    private static partial Regex VideoIdRegex();

    [GeneratedRegex(@"/(?:embed|v)/([a-zA-Z0-9_-]{11})")]
    private static partial Regex EmbedVideoIdRegex();

    [GeneratedRegex(@"^/channel/(UC[a-zA-Z0-9_-]+)")]
    private static partial Regex ChannelIdRegex();

    [GeneratedRegex(@"^/(@[a-zA-Z0-9_.-]+)")]
    private static partial Regex HandleRegex();

    [GeneratedRegex(@"^/(?:c|user)/([a-zA-Z0-9_.-]+)")]
    private static partial Regex LegacyChannelRegex();
}
