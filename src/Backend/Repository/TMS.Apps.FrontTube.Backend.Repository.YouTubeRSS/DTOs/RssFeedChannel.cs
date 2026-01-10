namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS.DTOs;

/// <summary>
/// Represents channel metadata from the YouTube RSS feed header.
/// </summary>
internal sealed record RssFeedChannel
{
    /// <summary>
    /// The YouTube channel ID.
    /// </summary>
    public required string ChannelId { get; init; }

    /// <summary>
    /// The name/title of the channel.
    /// </summary>
    public required string ChannelName { get; init; }

    /// <summary>
    /// The absolute URL to the channel on YouTube.
    /// </summary>
    public required string ChannelUrl { get; init; }

    /// <summary>
    /// When the feed was last updated (UTC).
    /// </summary>
    public DateTimeOffset? UpdatedAtUtc { get; init; }
}
