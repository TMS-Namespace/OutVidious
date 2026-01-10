namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;

/// <summary>
/// Represents the complete parsed RSS feed from a YouTube channel.
/// </summary>
internal sealed record RssFeedResult
{
    /// <summary>
    /// The channel metadata from the feed header.
    /// </summary>
    public required RssFeedChannel Channel { get; init; }

    /// <summary>
    /// The list of videos from the feed entries.
    /// </summary>
    public IReadOnlyList<RssFeedVideo> Videos { get; init; } = [];
}
