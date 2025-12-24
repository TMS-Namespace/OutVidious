namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents basic channel/author information.
/// </summary>
public record ChannelMetadata
{
    /// <summary>
    /// Unique identifier for the channel.
    /// </summary>
    public required string RemoteId { get; init; }

    /// <summary>
    /// Display name of the channel.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// URL to the channel page.
    /// </summary>
    [Obsolete("Use RemoteId instead")]
    public Uri? ChannelUrl { get; init; }

    /// <summary>
    /// Subscriber count text (e.g., "1.5M subscribers").
    /// </summary>
    [Obsolete("Use SubscriberCount instead")]
    public string? SubscriberCountText { get; init; }

    /// <summary>
    /// Approximate subscriber count (parsed from text if available).
    /// </summary>
    public long? SubscriberCount { get; init; }

    /// <summary>
    /// Available thumbnails for the channel avatar.
    /// </summary>
    public IReadOnlyList<Image> Avatars { get; init; } = [];
}
