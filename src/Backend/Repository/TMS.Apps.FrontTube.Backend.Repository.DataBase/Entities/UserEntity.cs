using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Scoped;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Represents a user of the application.
/// </summary>
public class UserEntity
{
    public int Id { get; set; }

    /// <summary>
    /// User display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<ScopedSubscriptionEntity> Subscriptions { get; set; } = [];

    public ICollection<ScopedLocalPlaylistEntity> LocalPlaylists { get; set; } = [];

    public ICollection<ScopedWatchingHistoryEntity> WatchingHistory { get; set; } = [];

    public ICollection<ScopedGroupEntity> Groups { get; set; } = [];
}
