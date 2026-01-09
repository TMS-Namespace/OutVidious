namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Configurable appearance options for the combined video player.
/// </summary>
public sealed record CombinedVideoPlayerAppearanceOptions
{
    public const string DefaultContainerClass = "combined-video-player";
    public const string DefaultSurfaceClass = "combined-video-player__surface";
    public const string DefaultPlayerClass = "combined-video-player__media";
    public const string DefaultPosterClass = "combined-video-player__poster";
    public const string DefaultOverlayClass = "combined-video-player__overlay";
    public const string DefaultSpinnerClass = "combined-video-player__spinner";
    public const string DefaultErrorClass = "combined-video-player__error";
    public const string DefaultInteractionClass = "combined-video-player__interaction";
    public const string DefaultPausedOverlayClass = "combined-video-player__paused-overlay";
    public const string DefaultPlayButtonClass = "combined-video-player__play-button";
    public const string DefaultPlayIconClass = "combined-video-player__play-icon";
    public const string DefaultAspectRatio = "16 / 9";

    public string ContainerClass { get; init; } = DefaultContainerClass;

    public string SurfaceClass { get; init; } = DefaultSurfaceClass;

    public string PlayerClass { get; init; } = DefaultPlayerClass;

    public string PosterClass { get; init; } = DefaultPosterClass;

    public string OverlayClass { get; init; } = DefaultOverlayClass;

    public string SpinnerClass { get; init; } = DefaultSpinnerClass;

    public string ErrorClass { get; init; } = DefaultErrorClass;

    public string InteractionClass { get; init; } = DefaultInteractionClass;

    public string PausedOverlayClass { get; init; } = DefaultPausedOverlayClass;

    public string PlayButtonClass { get; init; } = DefaultPlayButtonClass;

    public string PlayIconClass { get; init; } = DefaultPlayIconClass;

    public string AspectRatio { get; init; } = DefaultAspectRatio;

    public bool UseRoundedCorners { get; init; } = true;

    public string? ContainerStyle { get; init; }

    public string? SurfaceStyle { get; init; }

    public string? PlayerStyle { get; init; }

    public string? PosterStyle { get; init; }
}
