namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents detailed channel information.
/// </summary>
public sealed record ChannelDetails
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
    /// Channel description/about text.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// HTML-formatted description.
    /// </summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>
    /// URL to the channel page.
    /// </summary>
    public Uri? ChannelUrl { get; init; }

    /// <summary>
    /// Subscriber count text (e.g., "1.5M subscribers").
    /// </summary>
    public string? SubscriberCountText { get; init; }

    /// <summary>
    /// Approximate subscriber count.
    /// </summary>
    public long? SubscriberCount { get; init; }

    /// <summary>
    /// Total video count on the channel.
    /// </summary>
    public int? VideoCount { get; init; }

    /// <summary>
    /// Total view count across all videos.
    /// </summary>
    public long? TotalViewCount { get; init; }

    /// <summary>
    /// When the channel was created.
    /// </summary>
    public DateTimeOffset? JoinedAt { get; init; }

    /// <summary>
    /// Channel avatar thumbnails.
    /// </summary>
    public IReadOnlyList<ThumbnailInfo> Avatars { get; init; } = [];

    /// <summary>
    /// Channel banner images.
    /// </summary>
    public IReadOnlyList<ThumbnailInfo> Banners { get; init; } = [];

    /// <summary>
    /// Available tabs for this channel.
    /// </summary>
    public IReadOnlyList<ChannelTab> AvailableTabs { get; init; } = [];

    /// <summary>
    /// Whether the channel is verified.
    /// </summary>
    public bool IsVerified { get; init; }

    /// <summary>
    /// Keywords/tags associated with the channel.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; init; } = [];
}
