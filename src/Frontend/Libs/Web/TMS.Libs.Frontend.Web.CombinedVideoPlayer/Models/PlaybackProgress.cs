namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Represents playback position and state details.
/// </summary>
public sealed record PlaybackProgress
{
    public required TimeSpan Position { get; init; }

    public required TimeSpan Duration { get; init; }

    public double? ProgressRatio { get; init; }

    public bool IsPaused { get; init; }

    public bool IsBuffering { get; init; }

    public double PlaybackRate { get; init; }

    public double Volume { get; init; }

    public bool IsMuted { get; init; }
}
