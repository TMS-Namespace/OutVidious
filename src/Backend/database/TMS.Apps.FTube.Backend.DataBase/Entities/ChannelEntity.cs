namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Represents a YouTube channel.
/// </summary>
public class ChannelEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastSyncedAt { get; set; }

    /// <summary>
    /// YouTube channel ID (e.g., "UC...").
    /// </summary>
    public required string RemoteId { get; set; }

    /// <summary>
    /// Channel display name/title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Local alias for the channel.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Channel description/about text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// HTML-formatted description.
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// Channel handle (e.g., "@ChannelName").
    /// </summary>
    public string? Handle { get; set; }

    /// <summary>
    /// Subscriber count.
    /// </summary>
    public long? SubscriberCount { get; set; }

    /// <summary>
    /// Total video count.
    /// </summary>
    public int? VideoCount { get; set; }

    /// <summary>
    /// Total view count across all videos.
    /// </summary>
    public long? TotalViewCount { get; set; }

    /// <summary>
    /// When the channel was created on YouTube.
    /// </summary>
    public DateTime? JoinedAt { get; set; }

    /// <summary>
    /// Whether the channel is verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Keywords/tags associated with the channel (comma-separated).
    /// </summary>
    public string? Keywords { get; set; }

    // Navigation properties
    public ICollection<VideoEntity> Videos { get; set; } = [];

    public ICollection<ChannelAvatarMapEntity> Avatars { get; set; } = [];

    public ICollection<ChannelBannerMapEntity> Banners { get; set; } = [];

    public ICollection<ScopedSubscriptionEntity> Subscriptions { get; set; } = [];
}
