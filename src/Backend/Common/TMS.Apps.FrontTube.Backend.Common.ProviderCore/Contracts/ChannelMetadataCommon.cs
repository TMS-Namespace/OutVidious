using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents basic channel/author information.
/// </summary>
public record ChannelMetadataCommon : ICacheableCommon
{
    public required RemoteIdentityCommon RemoteIdentity { get; init; }

    /// <summary>
    /// Display name of the channel.
    /// </summary>
    public required string Name { get; init; }

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
    public IReadOnlyList<ImageMetadataCommon> Avatars { get; init; } = [];

    public bool IsMetaData => true;

}
