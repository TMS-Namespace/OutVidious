namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Represents a local playlist.
/// </summary>
public class LocalPlaylistEntity
{
    public int Id { get; set; }

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
    public ICollection<LocalPlaylistVideoMapEntity> VideoMappings { get; set; } = [];
}
