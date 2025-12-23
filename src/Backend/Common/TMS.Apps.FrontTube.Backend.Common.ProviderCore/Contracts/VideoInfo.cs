namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents detailed information about a video.
/// All properties are strongly typed with parsed values.
/// </summary>
public sealed record VideoInfo
{
    /// <summary>
    /// Unique identifier for the video.
    /// </summary>
    public required string VideoId { get; init; }

    /// <summary>
    /// Title of the video.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Plain text description of the video.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// HTML-formatted description of the video.
    /// </summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>
    /// Channel/author information.
    /// </summary>
    public required ChannelInfo Channel { get; init; }

    /// <summary>
    /// When the video was published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// Human-readable published time (e.g., "2 days ago").
    /// </summary>
    public string? PublishedTimeText { get; init; }

    /// <summary>
    /// Duration of the video.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Number of views.
    /// </summary>
    public long ViewCount { get; init; }

    /// <summary>
    /// Number of likes.
    /// </summary>
    public long LikeCount { get; init; }

    /// <summary>
    /// Number of dislikes (if available).
    /// </summary>
    public long? DislikeCount { get; init; }

    /// <summary>
    /// Video keywords/tags.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; init; } = [];

    /// <summary>
    /// Category/genre of the video.
    /// </summary>
    public string? Genre { get; init; }

    /// <summary>
    /// Available thumbnails.
    /// </summary>
    public IReadOnlyList<ThumbnailInfo> Thumbnails { get; init; } = [];

    /// <summary>
    /// Available media streams (adaptive formats - video only or audio only).
    /// </summary>
    public IReadOnlyList<StreamInfo> AdaptiveStreams { get; init; } = [];

    /// <summary>
    /// Available combined streams (video + audio).
    /// </summary>
    public IReadOnlyList<StreamInfo> CombinedStreams { get; init; } = [];

    /// <summary>
    /// Available caption tracks.
    /// </summary>
    public IReadOnlyList<CaptionInfo> Captions { get; init; } = [];

    /// <summary>
    /// URL to the DASH manifest (if available).
    /// </summary>
    public Uri? DashManifestUrl { get; init; }

    /// <summary>
    /// URL to the HLS manifest (if available).
    /// </summary>
    public Uri? HlsManifestUrl { get; init; }

    /// <summary>
    /// Whether the video is a live stream.
    /// </summary>
    public bool IsLive { get; init; }

    /// <summary>
    /// Whether the video is an upcoming premiere.
    /// </summary>
    public bool IsUpcoming { get; init; }

    /// <summary>
    /// Scheduled premiere time (if upcoming).
    /// </summary>
    public DateTimeOffset? PremiereTimestamp { get; init; }

    /// <summary>
    /// Whether the video is family-friendly.
    /// </summary>
    public bool IsFamilyFriendly { get; init; }

    /// <summary>
    /// Whether the video is publicly listed.
    /// </summary>
    public bool IsListed { get; init; }

    /// <summary>
    /// Whether users can rate the video.
    /// </summary>
    public bool AllowRatings { get; init; }

    /// <summary>
    /// Whether this is a paid/premium video.
    /// </summary>
    public bool IsPremium { get; init; }

    /// <summary>
    /// Allowed regions (ISO country codes).
    /// </summary>
    public IReadOnlyList<string> AllowedRegions { get; init; } = [];
}
