namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interop;

public sealed record BufferingProgressPayload
{
    public double? BufferedUntilSeconds { get; init; }

    public double? BufferedRatio { get; init; }

    public IReadOnlyList<BufferedRangePayload> Ranges { get; init; } = [];
}
