namespace TMS.Apps.FTube.Backend.DataBase.Entities;

/// <summary>
/// Maps videos to local playlists.
/// </summary>
public class LocalPlaylistVideoMapEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public int LocalPlaylistId { get; set; }

    public int VideoId { get; set; }

    /// <summary>
    /// Position in the playlist for ordering.
    /// </summary>
    public int Position { get; set; }

    // Navigation properties
    public LocalPlaylistEntity LocalPlaylist { get; set; } = null!;

    public VideoEntity Video { get; set; } = null!;
}
