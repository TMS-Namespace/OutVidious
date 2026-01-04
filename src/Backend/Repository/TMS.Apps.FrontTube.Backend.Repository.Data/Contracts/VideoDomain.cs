using TMS.Apps.FrontTube.Backend.Repository.Data.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts;

/// <summary>
/// Represents a video.
/// </summary>
public sealed class VideoDomain : ICacheableDomain
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public required RemoteIdentityDomain RemoteIdentity { get; set; }

    /// <summary>
    /// Video title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Plain text description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// HTML-formatted description.
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    public long DurationSeconds { get; set; }

    /// <summary>
    /// Number of views.
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// Number of likes.
    /// </summary>
    public long? LikesCount { get; set; }

    /// <summary>
    /// Number of dislikes (if available).
    /// </summary>
    public long? DislikesCount { get; set; }

    /// <summary>
    /// When the video was published.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Video category/genre.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Keywords/tags (comma-separated).
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Whether the video is a live stream.
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Whether the video is an upcoming premiere.
    /// </summary>
    public bool IsUpcoming { get; set; }

    /// <summary>
    /// Whether this is a short-form video.
    /// </summary>
    public bool IsShort { get; set; }

    /// <summary>
    /// Whether the video has been fully watched.
    /// </summary>
    public bool IsWatched { get; set; }

    // Foreign keys
    public int ChannelId { get; set; }

    // Navigation properties
    public ChannelDomain Channel { get; set; }

    public IReadOnlyList<ImageDomain> Thumbnails { get; set; } = [];

    public ICollection<CaptionDomain> Captions { get; set; } = [];

    public ICollection<StreamDomain> Streams { get; set; } = [];

    public string? FetchingError { get; set; }
}
