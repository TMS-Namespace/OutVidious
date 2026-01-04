using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Cache;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents basic channel/author information.
/// </summary>
public record ChannelMetadataCommon : ICacheableCommon
{
    /// <summary>
    /// Absolute URL to the channel on the remote platform (e.g., https://www.youtube.com/channel/...).
    /// Used as the unique identifier for hashing and lookups.
    /// </summary>
    public required Uri AbsoluteRemoteUrl { get; init; }

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

    private long? _hash;
    public long Hash => _hash ??= HashHelper.ComputeHash(AbsoluteRemoteUrl.ToString());

    public bool IsMetaData => true;

}
