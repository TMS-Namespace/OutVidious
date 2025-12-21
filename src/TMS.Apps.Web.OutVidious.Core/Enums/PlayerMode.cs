namespace TMS.Apps.Web.OutVidious.Core.Enums;

/// <summary>
/// Represents the video player rendering mode.
/// </summary>
public enum PlayerMode
{
    /// <summary>
    /// Uses native HTML5 video player with direct stream URLs.
    /// Limited to combined audio+video formats (up to ~720p).
    /// </summary>
    Native,

    /// <summary>
    /// Uses Shaka Player with DASH manifest for adaptive streaming.
    /// Supports higher quality (1080p, 1440p, 4K) with separate audio/video streams.
    /// </summary>
    Dash,

    /// <summary>
    /// Embeds the Invidious player via iframe.
    /// Less control but uses Invidious's built-in player features.
    /// </summary>
    Embedded
}
