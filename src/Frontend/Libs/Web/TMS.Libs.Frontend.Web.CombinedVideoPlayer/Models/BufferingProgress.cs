namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Represents buffering status for playback.
/// </summary>
public sealed record BufferingProgress
{
    public TimeSpan? BufferedUntil { get; init; }

    public double? BufferedRatio { get; init; }

    public IReadOnlyList<BufferedRange> Ranges { get; init; } = [];
}
