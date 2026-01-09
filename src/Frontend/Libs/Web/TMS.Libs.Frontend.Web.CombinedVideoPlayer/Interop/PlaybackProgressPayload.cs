namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interop;

public sealed record PlaybackProgressPayload
{
    public double PositionSeconds { get; init; }

    public double DurationSeconds { get; init; }

    public bool IsPaused { get; init; }

    public bool IsBuffering { get; init; }

    public double PlaybackRate { get; init; }

    public double Volume { get; init; }

    public bool IsMuted { get; init; }
}
