namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

/// <summary>
/// Describes the supported capabilities of a player implementation.
/// </summary>
public sealed record PlayerCapabilities
{
    public bool SupportsPlayPause { get; init; }

    public bool SupportsSeeking { get; init; }

    public bool SupportsVolume { get; init; }

    public bool SupportsMute { get; init; }

    public bool SupportsPlaybackRate { get; init; }

    public bool SupportsQualitySelection { get; init; }

    public bool SupportsCaptions { get; init; }

    public bool SupportsBufferingInfo { get; init; }

    public bool SupportsStatistics { get; init; }

    public bool SupportsReload { get; init; }

    public static PlayerCapabilities Native => new()
    {
        SupportsPlayPause = true,
        SupportsSeeking = true,
        SupportsVolume = true,
        SupportsMute = true,
        SupportsPlaybackRate = true,
        SupportsQualitySelection = false,
        SupportsCaptions = true,
        SupportsBufferingInfo = true,
        SupportsStatistics = true,
        SupportsReload = true
    };

    public static PlayerCapabilities Dash => new()
    {
        SupportsPlayPause = true,
        SupportsSeeking = true,
        SupportsVolume = true,
        SupportsMute = true,
        SupportsPlaybackRate = true,
        SupportsQualitySelection = true,
        SupportsCaptions = true,
        SupportsBufferingInfo = true,
        SupportsStatistics = true,
        SupportsReload = true
    };

    public static PlayerCapabilities Embedded => new()
    {
        SupportsPlayPause = false,
        SupportsSeeking = false,
        SupportsVolume = false,
        SupportsMute = false,
        SupportsPlaybackRate = false,
        SupportsQualitySelection = false,
        SupportsCaptions = false,
        SupportsBufferingInfo = false,
        SupportsStatistics = false,
        SupportsReload = false
    };
}
