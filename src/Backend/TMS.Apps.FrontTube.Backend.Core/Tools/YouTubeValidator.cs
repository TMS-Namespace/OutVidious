using System.Text.RegularExpressions;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;

/// <summary>
/// Internal utility class for validating and parsing YouTube video and channel IDs/URLs.
/// </summary>
internal static partial class YouTubeValidator
{
    private const string YouTubeVideoBaseUrl = "https://www.youtube.com/watch?v=";
    private const string YouTubeChannelBaseUrl = "https://www.youtube.com/channel/";

    /// <summary>
    /// Validates that a video ID is not null or empty.
    /// </summary>
    /// <param name="videoId">The video ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when video ID is null or empty.</exception>
    internal static void ValidateVideoIdNotEmpty(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            throw new ArgumentException("Video ID cannot be empty.", nameof(videoId));
        }
    }

    /// <summary>
    /// Validates that a channel ID is not null or empty.
    /// </summary>
    /// <param name="channelId">The channel ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when channel ID is null or empty.</exception>
    internal static void ValidateChannelIdNotEmpty(string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel ID cannot be empty.", nameof(channelId));
        }
    }

    /// <summary>
    /// Validates whether a video ID is in the correct format.
    /// </summary>
    /// <param name="videoId">The video identifier to validate.</param>
    /// <returns>True if the video ID format is valid.</returns>
    internal static bool IsValidVideoId(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return false;
        }

        // YouTube video IDs are 11 characters, containing letters, numbers, underscores, and hyphens
        return YoutubeVideoIdRegex().IsMatch(videoId);
    }

    /// <summary>
    /// Validates whether a channel ID is in the correct format.
    /// </summary>
    /// <param name="channelId">The channel identifier to validate.</param>
    /// <returns>True if the channel ID format is valid.</returns>
    internal static bool IsValidChannelId(string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
        {
            return false;
        }

        // YouTube channel IDs are 24 characters starting with "UC"
        // Also accept custom handles starting with "@"
        return YoutubeChannelIdRegex().IsMatch(channelId) || 
               YoutubeChannelHandleRegex().IsMatch(channelId);
    }

    /// <summary>
    /// Extracts the video ID from a YouTube absolute remote URL.
    /// Supports formats: youtube.com/watch?v=ID, youtu.be/ID, youtube.com/embed/ID, etc.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The absolute YouTube video URL.</param>
    /// <returns>The video ID or null if extraction fails.</returns>
    internal static string? ExtractVideoIdFromUrl(Uri absoluteRemoteUrl)
    {
        ArgumentNullException.ThrowIfNull(absoluteRemoteUrl);

        var url = absoluteRemoteUrl.ToString();

        // Try query parameter ?v=
        var match = YoutubeVideoUrlQueryRegex().Match(url);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Try youtu.be short URL
        match = YoutubeShortUrlRegex().Match(url);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Try embed URL
        match = YoutubeEmbedUrlRegex().Match(url);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    /// <summary>
    /// Extracts the channel ID from a YouTube absolute remote URL.
    /// Supports formats: youtube.com/channel/ID, youtube.com/@handle, etc.
    /// </summary>
    /// <param name="absoluteRemoteUrl">The absolute YouTube channel URL.</param>
    /// <returns>The channel ID (or handle) or null if extraction fails.</returns>
    internal static string? ExtractChannelIdFromUrl(Uri absoluteRemoteUrl)
    {
        ArgumentNullException.ThrowIfNull(absoluteRemoteUrl);

        var url = absoluteRemoteUrl.ToString();

        // Try /channel/UC... format
        var match = YoutubeChannelUrlRegex().Match(url);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Try /@handle format
        match = YoutubeHandleUrlRegex().Match(url);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }

    /// <summary>
    /// Builds a canonical YouTube video URL from a video ID.
    /// </summary>
    /// <param name="videoId">The video ID.</param>
    /// <returns>The canonical absolute remote URL.</returns>
    internal static Uri BuildVideoUrl(string videoId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        return new Uri($"{YouTubeVideoBaseUrl}{videoId}");
    }

    /// <summary>
    /// Builds a canonical YouTube channel URL from a channel ID.
    /// </summary>
    /// <param name="channelId">The channel ID (or handle including @).</param>
    /// <returns>The canonical absolute remote URL.</returns>
    internal static Uri BuildChannelUrl(string channelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);

        // If it's a handle, use /@handle format
        if (channelId.StartsWith('@'))
        {
            return new Uri($"https://www.youtube.com/{channelId}");
        }

        return new Uri($"{YouTubeChannelBaseUrl}{channelId}");
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{11}$")]
    private static partial Regex YoutubeVideoIdRegex();

    [GeneratedRegex(@"^UC[a-zA-Z0-9_-]{22}$")]
    private static partial Regex YoutubeChannelIdRegex();

    [GeneratedRegex(@"^@[a-zA-Z0-9_.-]{3,30}$")]
    private static partial Regex YoutubeChannelHandleRegex();

    [GeneratedRegex(@"[?&]v=([a-zA-Z0-9_-]{11})")]
    private static partial Regex YoutubeVideoUrlQueryRegex();

    [GeneratedRegex(@"youtu\.be/([a-zA-Z0-9_-]{11})")]
    private static partial Regex YoutubeShortUrlRegex();

    [GeneratedRegex(@"youtube\.com/embed/([a-zA-Z0-9_-]{11})")]
    private static partial Regex YoutubeEmbedUrlRegex();

    [GeneratedRegex(@"youtube\.com/channel/(UC[a-zA-Z0-9_-]{22})")]
    private static partial Regex YoutubeChannelUrlRegex();

    [GeneratedRegex(@"youtube\.com/(@[a-zA-Z0-9_.-]+)")]
    private static partial Regex YoutubeHandleUrlRegex();
}
