namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Represents a buffered time range.
/// </summary>
public sealed record BufferedRange
{
    public required TimeSpan Start { get; init; }

    public required TimeSpan End { get; init; }
}
