namespace TMS.Apps.Web.OutVidious.Core.Enums;

/// <summary>
/// Represents the video player rendering mode.
/// </summary>
public enum PlayerMode
{
    /// <summary>
    /// Uses native HTML5 video player with direct stream URLs.
    /// Provides more control over playback.
    /// </summary>
    Native,

    /// <summary>
    /// Embeds the Invidious player via iframe.
    /// Less control but uses Invidious's built-in player features.
    /// </summary>
    Embedded
}
