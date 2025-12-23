namespace TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;

/// <summary>
/// Represents basic channel/author information.
/// </summary>
public sealed record ChannelInfo
{
    /// <summary>
    /// Unique identifier for the channel.
    /// </summary>
    public required string ChannelId { get; init; }

    /// <summary>
    /// Display name of the channel.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// URL to the channel page.
    /// </summary>
    public Uri? ChannelUrl { get; init; }

    /// <summary>
    /// Subscriber count text (e.g., "1.5M subscribers").
    /// </summary>
    public string? SubscriberCountText { get; init; }

    /// <summary>
    /// Approximate subscriber count (parsed from text if available).
    /// </summary>
    public long? SubscriberCount { get; init; }

    /// <summary>
    /// Available thumbnails for the channel avatar.
    /// </summary>
    public IReadOnlyList<ThumbnailInfo> Thumbnails { get; init; } = [];
}
