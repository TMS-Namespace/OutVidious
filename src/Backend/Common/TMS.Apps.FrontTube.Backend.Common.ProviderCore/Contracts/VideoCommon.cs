namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents detailed information about a video.
/// All properties are strongly typed with parsed values.
/// </summary>
public sealed record VideoCommon : VideoBaseCommon
{
    /// <summary>
    /// Plain text description of the video.
    /// </summary>
    public string DescriptionText { get; init; } = string.Empty;

    /// <summary>
    /// HTML-formatted description of the video.
    /// </summary>
    public string? DescriptionHtml { get; init; }

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
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Category/genre of the video.
    /// </summary>
    public string? Genre { get; init; }

    /// <summary>
    /// Available media streams (adaptive formats - video only or audio only).
    /// </summary>
    public IReadOnlyList<StreamMetadataCommon> AdaptiveStreams { get; init; } = [];

    /// <summary>
    /// Available combined streams (video + audio).
    /// </summary>
    public IReadOnlyList<StreamMetadataCommon> MutexStreams { get; init; } = [];

    /// <summary>
    /// Available caption tracks.
    /// </summary>
    public IReadOnlyList<CaptionMetadataCommon> Captions { get; init; } = [];

    /// <summary>
    /// URL to the DASH manifest (if available).
    /// </summary>
    [Obsolete("use stream information instead")]
    public Uri? DashManifestUrl { get; init; }

    /// <summary>
    /// URL to the HLS manifest (if available).
    /// </summary>
    [Obsolete("use stream information instead")]
    public Uri? HlsManifestUrl { get; init; }

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

    public new bool IsMetaData => false;
}
