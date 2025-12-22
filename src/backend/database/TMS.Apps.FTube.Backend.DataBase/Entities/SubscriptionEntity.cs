namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Represents a channel subscription.
/// </summary>
public class SubscriptionEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ChannelId { get; set; }

    /// <summary>
    /// Whether notifications are enabled for this subscription.
    /// </summary>
    public bool NotificationsEnabled { get; set; }

    /// <summary>
    /// Custom notification settings (JSON).
    /// </summary>
    public string? NotificationSettings { get; set; }

    // Navigation property
    public ChannelEntity Channel { get; set; } = null!;
}
