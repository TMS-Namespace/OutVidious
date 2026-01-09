namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interop;

public sealed record BufferedRangePayload
{
    public double StartSeconds { get; init; }

    public double EndSeconds { get; init; }
}
