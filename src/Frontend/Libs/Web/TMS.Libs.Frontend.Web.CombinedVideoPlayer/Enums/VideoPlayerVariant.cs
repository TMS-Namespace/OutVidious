namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Enums;

/// <summary>
/// Represents the supported video player variants.
/// </summary>
public enum VideoPlayerVariant
{
    /// <summary>
    /// Native HTML5 video player.
    /// </summary>
    Native,

    /// <summary>
    /// DASH playback with Shaka Player.
    /// </summary>
    Dash,

    /// <summary>
    /// Embedded external player (e.g., Invidious).
    /// </summary>
    Embedded
}
