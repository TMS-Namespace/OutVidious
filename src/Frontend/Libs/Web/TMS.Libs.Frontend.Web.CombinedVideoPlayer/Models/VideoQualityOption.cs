namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Represents a selectable quality option for video playback.
/// </summary>
public sealed record VideoQualityOption
{
    public required string Label { get; init; }

    public int? Height { get; init; }

    public int? Width { get; init; }

    public int? BitrateKbps { get; init; }

    public bool IsAuto { get; init; }
}
