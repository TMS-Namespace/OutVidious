namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Configurable behavior options for the combined video player.
/// </summary>
public sealed record CombinedVideoPlayerBehaviorOptions
{
    /// <summary>
    /// Default opacity for the paused overlay (0.0 to 1.0).
    /// </summary>
    public const double DefaultPausedOverlayOpacity = 0.45;

    /// <summary>
    /// Default opacity for the play button background (0.0 to 1.0).
    /// Lower values make the button more transparent/faint.
    /// </summary>
    public const double DefaultPlayButtonOpacity = 0.12;

    /// <summary>
    /// Default opacity for the play button background on hover (0.0 to 1.0).
    /// </summary>
    public const double DefaultPlayButtonHoverOpacity = 0.35;

    /// <summary>
    /// Default volume change step for scroll events.
    /// </summary>
    public const double DefaultVolumeStep = 0.05;

    /// <summary>
    /// When true, the poster image is shown until the user explicitly clicks play.
    /// When false (default), the poster is only shown while loading.
    /// Note: If AutoPlay is enabled, the poster will be hidden once the player is ready.
    /// </summary>
    public bool ShowPosterUntilPlay { get; init; } = true;

    /// <summary>
    /// When true, shows the poster image while the player is loading.
    /// </summary>
    public bool ShowPosterWhileLoading { get; init; } = true;

    /// <summary>
    /// When true, shows a loading spinner while the player is loading.
    /// </summary>
    public bool ShowSpinnerWhileLoading { get; init; } = true;

    /// <summary>
    /// When true, shows a semi-transparent overlay with a play button when the video is paused.
    /// </summary>
    public bool ShowPausedOverlay { get; init; } = true;

    /// <summary>
    /// When true, shows error messages as an overlay on the player surface.
    /// </summary>
    public bool ShowErrorOverlay { get; init; } = true;

    /// <summary>
    /// The opacity of the paused overlay (0.0 = fully transparent, 1.0 = fully opaque).
    /// </summary>
    public double PausedOverlayOpacity { get; init; } = DefaultPausedOverlayOpacity;

    /// <summary>
    /// The opacity of the play button background (0.0 = fully transparent, 1.0 = fully opaque).
    /// Lower values make the button appear more faint/subtle.
    /// </summary>
    public double PlayButtonOpacity { get; init; } = DefaultPlayButtonOpacity;

    /// <summary>
    /// The opacity of the play button background on hover (0.0 = fully transparent, 1.0 = fully opaque).
    /// </summary>
    public double PlayButtonHoverOpacity { get; init; } = DefaultPlayButtonHoverOpacity;

    /// <summary>
    /// The amount to change volume per scroll wheel tick (0.0 to 1.0).
    /// </summary>
    public double VolumeStep { get; init; } = DefaultVolumeStep;

    // Note: Invidious embedded player has limited customization options.
    // The following settings are kept for future provider support or custom implementations.
    // Currently, we use autoplay=1 and hide the player behind our poster/overlay until user clicks.
    // This approach works around Invidious limitations (no way to hide title overlay, share button, etc.)
}
