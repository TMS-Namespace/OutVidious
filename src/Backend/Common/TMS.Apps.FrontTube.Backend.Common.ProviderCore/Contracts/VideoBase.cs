using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Cache;
using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a compact video summary for lists and grids.
/// Contains only essential information for display in thumbnails.
/// </summary>
public abstract record VideoBase : ICacheableCommon
{
    /// <summary>
    /// Absolute URL to the video on the remote platform (e.g., https://www.youtube.com/watch?v=...).
    /// Used as the unique identifier for hashing and lookups.
    /// </summary>
    public required Uri AbsoluteRemoteUrl { get; init; }

    /// <summary>
    /// Title of the video.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Duration of the video.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Number of views.
    /// </summary>
    public long ViewCount { get; init; }

    /// <summary>
    /// Human-readable view count text (e.g., "1.5M views").
    /// </summary>
    [Obsolete("Use ViewCount and format on client side")]
    public string? ViewCountText { get; init; }

    /// <summary>
    /// Human-readable published time (e.g., "2 days ago").
    /// </summary>
    [Obsolete("Use PublishedAt and format on client side")]
    public string? PublishedAgo { get; init; }

    /// <summary>
    /// When the video was published.
    /// </summary>
    public DateTimeOffset? PublishedAtUtc { get; init; }

    /// <summary>
    /// Channel/author information.
    /// </summary>
    public required ChannelMetadata Channel { get; init; }

    /// <summary>
    /// Available thumbnails.
    /// </summary>
    public IReadOnlyList<ImageMetadata> Thumbnails { get; init; } = [];

    /// <summary>
    /// Whether the video is a live stream.
    /// </summary>
    public bool IsLive { get; init; }

    /// <summary>
    /// Whether the video is an upcoming premiere.
    /// </summary>
    public bool IsUpcoming { get; init; }

    private long? _hash;
    public long Hash => _hash ??= HashHelper.ComputeHash(AbsoluteRemoteUrl.ToString());

    public bool IsMetaData => true;
}
