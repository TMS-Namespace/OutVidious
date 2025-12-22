namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Represents a video watching history entry.
/// </summary>
public class WatchingHistoryEntity
{
    public int Id { get; set; }

    /// <summary>
    /// When the video was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the video was last watched.
    /// </summary>
    public DateTime? LastWatchedAt { get; set; }

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

    // Navigation property
    public VideoEntity Video { get; set; } = null!;
}
