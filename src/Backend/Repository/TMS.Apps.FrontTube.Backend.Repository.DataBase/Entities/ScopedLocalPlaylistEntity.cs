namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

/// <summary>
/// Represents a local playlist scoped to a user.
/// </summary>
public class ScopedLocalPlaylistEntity: TrackableEntitiesBase
{
    //public int Id { get; set; }

    /// <summary>
    /// User who owns this playlist.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// When the playlist was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Playlist name/alias.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Playlist description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is a built-in playlist (e.g., "Watch Later", "Favorites").
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// Sort order for display.
    /// </summary>
    public int SortOrder { get; set; }

    // Navigation properties
    public UserEntity User { get; set; } = null!;

    public ICollection<ScopedLocalPlaylistVideoMapEntity> VideoMappings { get; set; } = [];
}
