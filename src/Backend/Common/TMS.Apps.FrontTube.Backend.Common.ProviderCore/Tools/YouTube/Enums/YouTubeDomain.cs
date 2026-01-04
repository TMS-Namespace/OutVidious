namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools.YouTube.Enums;

/// <summary>
/// Represents the various YouTube domains.
/// </summary>
public enum YouTubeDomain
{
    /// <summary>
    /// Unknown or unrecognized domain.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Standard youtube.com domain.
    /// </summary>
    YouTube,

    /// <summary>
    /// Mobile youtube.com (m.youtube.com).
    /// </summary>
    YouTubeMobile,

    /// <summary>
    /// Short URL domain (youtu.be).
    /// </summary>
    YouTuBe,

    /// <summary>
    /// Privacy-enhanced embed domain (youtube-nocookie.com).
    /// </summary>
    YouTubeNoCookie,

    /// <summary>
    /// YouTube Music domain (music.youtube.com).
    /// </summary>
    YouTubeMusic,

    /// <summary>
    /// YouTube API domain (youtube.googleapis.com).
    /// </summary>
    YouTubeApi,

    /// <summary>
    /// YouTube image CDN domain (i.ytimg.com).
    /// </summary>
    YouTubeImages,

    /// <summary>
    /// YouTube image CDN alternate (img.youtube.com).
    /// </summary>
    YouTubeImagesAlt
}
