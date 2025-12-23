namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Represents a channel subscription scoped to a user.
/// </summary>
public class ScopedSubscriptionEntity
{
    public int Id { get; set; }

    /// <summary>
    /// User who owns this subscription.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// When the subscription was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the subscription was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// The channel being subscribed to.
    /// </summary>
    public int ChannelId { get; set; }

    /// <summary>
    /// Custom alias/name for this channel (user-defined).
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Whether notifications are enabled for this subscription.
    /// </summary>
    public bool NotificationsEnabled { get; set; }

    /// <summary>
    /// Custom notification settings (JSON).
    /// </summary>
    public string? NotificationSettings { get; set; }

    /// <summary>
    /// Default playback speed for videos from this subscription.
    /// </summary>
    public float? DefaultPlaybackSpeed { get; set; }

    /// <summary>
    /// Custom sync interval for fetching new videos (in seconds).
    /// Null means use default interval.
    /// </summary>
    public int? DefaultSyncInterval { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;

    public ChannelEntity Channel { get; set; } = null!;

    public ICollection<ScopedSubscriptionGroupMapEntity> GroupMappings { get; set; } = [];
}
