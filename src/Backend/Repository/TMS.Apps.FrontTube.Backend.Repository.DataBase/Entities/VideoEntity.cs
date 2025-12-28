using TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Represents a video.
/// </summary>
public class VideoEntity : TrackableEntitiesBase, ICacheableEntity
{
    //public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// XxHash64 hash of the absolute remote URL for unique lookup.
    /// </summary>
    public required long Hash { get; set; }

    /// <summary>
    /// The original YouTube video URL.
    /// </summary>
    public required string AbsoluteRemoteUrl { get; set; }

    /// <summary>
    /// Video title.
    /// </summary>
    public required string Title { get; set; }

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
    public ChannelEntity Channel { get; set; } =null!;

    public ICollection<VideoThumbnailMapEntity> Thumbnails { get; set; } = [];

    public ICollection<CaptionEntity> Captions { get; set; } = [];

    public ICollection<StreamEntity> Streams { get; set; } = [];

    public ICollection<ScopedLocalPlaylistVideoMapEntity> PlaylistMappings { get; set; } = [];

    public ICollection<ScopedWatchingHistoryEntity> WatchingHistory { get; set; } = [];
}
