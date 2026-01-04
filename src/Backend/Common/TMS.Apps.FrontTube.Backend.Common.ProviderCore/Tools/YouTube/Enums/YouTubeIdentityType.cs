namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

/// <summary>
/// Represents the type of YouTube identity parsed from a URL or ID string.
/// </summary>
public enum YouTubeIdentityType
{
    /// <summary>
    /// The identity type could not be recognized.
    /// </summary>
    Unrecognized = 0,

    /// <summary>
    /// A raw video ID (11 characters).
    /// </summary>
    VideoId,

    /// <summary>
    /// Standard video watch URL (youtube.com/watch?v=...).
    /// </summary>
    VideoWatch,

    /// <summary>
    /// Short video URL (youtu.be/...).
    /// </summary>
    VideoShortUrl,

    /// <summary>
    /// Embedded video URL (youtube.com/embed/...).
    /// </summary>
    VideoEmbed,

    /// <summary>
    /// Legacy embedded video URL (youtube.com/v/...).
    /// </summary>
    VideoLegacyEmbed,

    /// <summary>
    /// YouTube Shorts video (youtube.com/shorts/...).
    /// </summary>
    VideoShorts,

    /// <summary>
    /// YouTube Live stream (youtube.com/live/...).
    /// </summary>
    VideoLive,

    /// <summary>
    /// Video in a playlist context (youtube.com/watch?v=...&list=...).
    /// </summary>
    VideoInPlaylist,

    /// <summary>
    /// oEmbed URL format (youtube.com/oembed?url=...).
    /// </summary>
    VideoOEmbed,

    /// <summary>
    /// Attribution link format (youtube.com/attribution_link?...).
    /// </summary>
    VideoAttributionLink,

    /// <summary>
    /// YouTube-nocookie embed domain (youtube-nocookie.com/embed/...).
    /// </summary>
    VideoNoCookieEmbed,

    /// <summary>
    /// A raw channel ID (UC... format, 24 characters).
    /// </summary>
    ChannelId,

    /// <summary>
    /// Channel URL by ID (youtube.com/channel/UC...).
    /// </summary>
    ChannelById,

    /// <summary>
    /// Channel URL by handle (youtube.com/@...).
    /// </summary>
    ChannelByHandle,

    /// <summary>
    /// Legacy channel URL by custom name (youtube.com/c/...).
    /// </summary>
    ChannelByCustomName,

    /// <summary>
    /// Legacy channel URL by username (youtube.com/user/...).
    /// </summary>
    ChannelByUsername,

    /// <summary>
    /// Channel tab URL by handle (youtube.com/@.../videos, etc.).
    /// </summary>
    ChannelTabByHandle,

    /// <summary>
    /// Channel tab URL by ID (youtube.com/channel/UC.../videos, etc.).
    /// </summary>
    ChannelTabById,

    /// <summary>
    /// A raw playlist ID (PL... or other prefixes, typically 34 characters).
    /// </summary>
    PlaylistId,

    /// <summary>
    /// Playlist URL (youtube.com/playlist?list=...).
    /// </summary>
    Playlist,

    /// <summary>
    /// YouTube Music URL (music.youtube.com/...).
    /// </summary>
    YouTubeMusic,

    /// <summary>
    /// YouTube thumbnail image URL (i.ytimg.com/vi/...).
    /// </summary>
    ThumbnailImage,

    /// <summary>
    /// YouTube API URL (youtube.googleapis.com/...).
    /// </summary>
    ApiUrl,

    /// <summary>
    /// YouTube feed or subscription URL.
    /// </summary>
    Feed
}
