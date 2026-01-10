using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Cache;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities.Scoped;

/// <summary>
/// Maps videos to local playlists.
/// </summary>
public class ScopedLocalPlaylistVideoMapEntity
{
    public int Id { get; set; }

    /// <summary>
    /// When the video was added to the playlist.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The playlist this video belongs to.
    /// </summary>
    public int LocalPlaylistId { get; set; }

    /// <summary>
    /// The video in the playlist.
    /// </summary>
    public int VideoId { get; set; }

    /// <summary>
    /// Position in the playlist for ordering.
    /// </summary>
    public int Position { get; set; }

    // Navigation properties
    public ScopedLocalPlaylistEntity LocalPlaylist { get; set; } = null!;

    public CacheVideoEntity Video { get; set; } = null!;
}
