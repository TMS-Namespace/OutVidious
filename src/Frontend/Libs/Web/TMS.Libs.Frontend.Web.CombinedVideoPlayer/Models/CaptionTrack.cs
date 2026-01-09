namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Represents a caption/subtitle track for playback.
/// </summary>
public sealed record CaptionTrack
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public string? Language { get; init; }

    public string? Url { get; init; }

    public bool IsDefault { get; init; }
}
