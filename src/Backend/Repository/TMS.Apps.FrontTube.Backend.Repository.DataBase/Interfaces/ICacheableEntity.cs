using System.ComponentModel.DataAnnotations;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Interfaces;

/// <summary>
/// Represents an entity that can be cached.
/// All cacheable database entities must implement this interface.
/// </summary>
public interface ICacheableEntity : IEntity
{
    /// <summary>
    /// XxHash64 hash of the AbsoluteRemoteUrl for unique lookup and cache key.
    /// </summary>
    [Required]
    long Hash { get; set; }

    // /// <summary>
    // /// When this entity was created.
    // /// </summary>
    // DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When this entity was last synchronized from the remote source.
    /// Used for staleness checking.
    /// </summary>
    DateTime? LastSyncedAt { get; set; }

    // /// <summary>
    // /// The original remote URL of this entity.
    // /// </summary>
    [Required]
    string RemoteIdentity { get; set; }
}
