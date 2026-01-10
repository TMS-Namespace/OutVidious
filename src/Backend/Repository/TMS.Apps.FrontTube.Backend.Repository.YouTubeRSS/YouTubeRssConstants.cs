namespace TMS.Apps.FrontTube.Backend.Repository.YouTubeRSS;

/// <summary>
/// Constants for YouTube RSS feed URL construction.
/// </summary>
internal static class YouTubeRssConstants
{
    /// <summary>
    /// Base URL for YouTube RSS feeds.
    /// </summary>
    public const string RssFeedBaseUrl = "https://www.youtube.com/feeds/videos.xml";

    /// <summary>
    /// Query parameter name for channel ID.
    /// </summary>
    public const string ChannelIdParam = "channel_id";

    /// <summary>
    /// Query parameter name for username (legacy).
    /// </summary>
    public const string UserParam = "user";

    /// <summary>
    /// Query parameter name for playlist ID.
    /// </summary>
    public const string PlaylistIdParam = "playlist_id";

    /// <summary>
    /// YouTube namespace for yt:videoId, yt:channelId elements.
    /// </summary>
    public const string YouTubeNamespace = "http://www.youtube.com/xml/schemas/2015";

    /// <summary>
    /// Media namespace for media:group, media:thumbnail, media:description elements.
    /// </summary>
    public const string MediaNamespace = "http://search.yahoo.com/mrss/";
}
