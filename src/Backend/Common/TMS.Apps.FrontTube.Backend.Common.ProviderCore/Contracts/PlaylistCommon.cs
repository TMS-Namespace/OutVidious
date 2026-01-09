using TMS.Apps.FrontTube.Backend.Common.ProviderCore.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Contracts;

/// <summary>
/// Represents a playlist with full details.
/// </summary>
public sealed record PlaylistCommon : ICommonContract
{
    /// <summary>
    /// Playlist ID.
    /// </summary>
    public required string PlaylistId { get; init; }

    /// <summary>
    /// Playlist title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Playlist description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// HTML formatted description.
    /// </summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>
    /// Author/owner of the playlist.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Author channel ID.
    /// </summary>
    public required string AuthorId { get; init; }

    /// <summary>
    /// Author's thumbnails.
    /// </summary>
    public IReadOnlyList<ImageMetadataCommon> AuthorThumbnails { get; init; } = [];

    /// <summary>
    /// Playlist thumbnail URL.
    /// </summary>
    public string? PlaylistThumbnail { get; init; }

    /// <summary>
    /// Total number of videos in the playlist.
    /// </summary>
    public int VideoCount { get; init; }

    /// <summary>
    /// Total view count across all videos.
    /// </summary>
    public long ViewCount { get; init; }

    /// <summary>
    /// Last updated timestamp (Unix epoch).
    /// </summary>
    public long UpdatedAt { get; init; }

    /// <summary>
    /// Whether the playlist is a mix (auto-generated).
    /// </summary>
    public bool IsMix { get; init; }

    /// <summary>
    /// Videos in the current page.
    /// </summary>
    public IReadOnlyList<PlaylistVideoCommon> Videos { get; init; } = [];

    /// <summary>
    /// Continuation token for fetching more videos.
    /// </summary>
    public string? ContinuationToken { get; init; }

    /// <summary>
    /// Whether there are more videos to load.
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(ContinuationToken);
}

/// <summary>
/// Represents a video within a playlist.
/// </summary>
public sealed record PlaylistVideoCommon : ICommonContract
{
    /// <summary>
    /// Video title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Video ID.
    /// </summary>
    public required string VideoId { get; init; }

    /// <summary>
    /// Video thumbnails.
    /// </summary>
    public IReadOnlyList<ImageMetadataCommon> Thumbnails { get; init; } = [];

    /// <summary>
    /// Video author.
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// Author channel ID.
    /// </summary>
    public required string AuthorId { get; init; }

    /// <summary>
    /// Video duration in seconds.
    /// </summary>
    public int LengthSeconds { get; init; }

    /// <summary>
    /// Index of the video in the playlist.
    /// </summary>
    public int Index { get; init; }
}
