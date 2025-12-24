using System.Text.RegularExpressions;

namespace TMS.Apps.FrontTube.Backend.Core.Tools;

/// <summary>
/// Internal utility class for validating YouTube video and channel IDs.
/// </summary>
internal static partial class YouTubeValidator
{
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

    [GeneratedRegex(@"^[a-zA-Z0-9_-]{11}$")]
    private static partial Regex YoutubeVideoIdRegex();

    [GeneratedRegex(@"^UC[a-zA-Z0-9_-]{22}$")]
    private static partial Regex YoutubeChannelIdRegex();

    [GeneratedRegex(@"^@[a-zA-Z0-9_.-]{3,30}$")]
    private static partial Regex YoutubeChannelHandleRegex();
}
