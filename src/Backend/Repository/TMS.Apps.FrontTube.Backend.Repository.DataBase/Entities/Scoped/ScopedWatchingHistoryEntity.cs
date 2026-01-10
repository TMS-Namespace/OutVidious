using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Scoped;

/// <summary>
/// Represents a video watching history entry scoped to a user.
/// </summary>
public class ScopedWatchingHistoryEntity
{
    public int Id { get; set; }

    /// <summary>
    /// User who owns this history entry.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// When the video was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the video was last watched.
    /// </summary>
    public DateTime? LastWatchedAt { get; set; }

    /// <summary>
    /// The video being watched.
    /// </summary>
    public int VideoId { get; set; }

    /// <summary>
    /// Last watched position in seconds.
    /// </summary>
    public int LastPosition { get; set; }

    /// <summary>
    /// Video duration at time of watching (for percentage calculation).
    /// </summary>
    public int? VideoDuration { get; set; }

    /// <summary>
    /// Whether the video has been marked as watched.
    /// </summary>
    public bool MarkedAsWatched { get; set; }

    /// <summary>
    /// Playback speed used.
    /// </summary>
    public float? PlaybackSpeed { get; set; }

    /// <summary>
    /// Video quality label the user was watching (e.g., "1080p", "720p").
    /// Used to resume at the same quality.
    /// </summary>
    public string? VideoQualityLabel { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;

    public CacheVideoEntity Video { get; set; } = null!;
}
