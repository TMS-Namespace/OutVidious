using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Enums;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interfaces;

/// <summary>
/// Unified abstraction for controlling a video player variant.
/// </summary>
public interface IPlayer
{
    VideoPlayerVariant Variant { get; }

    PlayerCapabilities Capabilities { get; }

    ValueTask<bool> PlayAsync(CancellationToken cancellationToken);

    ValueTask<bool> PauseAsync(CancellationToken cancellationToken);

    ValueTask<bool> TogglePlayPauseAsync(CancellationToken cancellationToken);

    ValueTask<bool> SeekAsync(TimeSpan position, CancellationToken cancellationToken);

    ValueTask<bool> PlayFromAsync(TimeSpan position, CancellationToken cancellationToken);

    ValueTask<bool> SetVolumeAsync(double volume, CancellationToken cancellationToken);

    ValueTask<bool> SetPlaybackRateAsync(double playbackRate, CancellationToken cancellationToken);

    ValueTask<bool> SetMutedAsync(bool isMuted, CancellationToken cancellationToken);

    ValueTask<bool> SetQualityAsync(VideoQualityOption quality, CancellationToken cancellationToken);

    ValueTask<bool> SetCaptionAsync(string? captionId, CancellationToken cancellationToken);

    ValueTask<bool> ReloadAsync(CancellationToken cancellationToken);

    ValueTask<PlaybackProgress?> GetPlaybackProgressAsync(CancellationToken cancellationToken);

    ValueTask<BufferingProgress?> GetBufferingProgressAsync(CancellationToken cancellationToken);

    ValueTask<PlaybackStatistics?> GetPlaybackStatisticsAsync(CancellationToken cancellationToken);
}
