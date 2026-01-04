namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

/// <summary>
/// Represents the various tabs available on a YouTube channel page.
/// </summary>
public enum YouTubeChannelTab
{
    /// <summary>
    /// No specific tab or unknown tab.
    /// </summary>
    None = 0,

    /// <summary>
    /// Featured/Home tab (default channel page).
    /// </summary>
    Featured,

    /// <summary>
    /// Videos tab showing all uploaded videos.
    /// </summary>
    Videos,

    /// <summary>
    /// Shorts tab showing YouTube Shorts content.
    /// </summary>
    Shorts,

    /// <summary>
    /// Live streams tab showing past and current live streams.
    /// </summary>
    Streams,

    /// <summary>
    /// Releases tab (for music channels).
    /// </summary>
    Releases,

    /// <summary>
    /// Playlists tab showing channel playlists.
    /// </summary>
    Playlists,

    /// <summary>
    /// Community tab for channel posts.
    /// </summary>
    Community,

    /// <summary>
    /// Channels tab showing featured/related channels.
    /// </summary>
    Channels,

    /// <summary>
    /// About tab with channel information.
    /// </summary>
    About,

    /// <summary>
    /// Search within channel.
    /// </summary>
    Search,

    /// <summary>
    /// Podcasts tab (for channels with podcasts).
    /// </summary>
    Podcasts,

    /// <summary>
    /// Store tab (for channels with merchandise).
    /// </summary>
    Store
}
