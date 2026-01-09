namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Represents playback statistics when available.
/// </summary>
public sealed record PlaybackStatistics
{
    public long? DroppedFrames { get; init; }

    public long? TotalFrames { get; init; }

    public double? EstimatedBandwidthKbps { get; init; }

    public double? StreamBandwidthKbps { get; init; }

    public double? BufferingSeconds { get; init; }

    public int? Width { get; init; }

    public int? Height { get; init; }
}
