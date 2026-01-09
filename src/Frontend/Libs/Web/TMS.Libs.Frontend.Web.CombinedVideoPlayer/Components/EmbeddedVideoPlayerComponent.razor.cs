using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Enums;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interfaces;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Components;

/// <summary>
/// Embedded external player implementation.
/// </summary>
public partial class EmbeddedVideoPlayerComponent : ComponentBase, IPlayer
{
    private ILogger<EmbeddedVideoPlayerComponent> Logger { get; set; } = NullLogger<EmbeddedVideoPlayerComponent>.Instance;
    private ILoggerFactory? _lastLoggerFactory;

    [Parameter]
    public ILoggerFactory? LoggerFactory { get; set; }

    [Parameter]
    public string? EmbedUrl { get; set; }

    [Parameter]
    public string? PlayerClass { get; set; }

    [Parameter]
    public string? PlayerStyle { get; set; }

    [Parameter]
    public string FrameTitle { get; set; } = CombinedVideoPlayerText.DefaultFrameTitle;

    [Parameter]
    public EventCallback PlayerReady { get; set; }

    [Parameter]
    public EventCallback<string> PlayerError { get; set; }

    public VideoPlayerVariant Variant => VideoPlayerVariant.Embedded;

    public PlayerCapabilities Capabilities => PlayerCapabilities.Embedded;

    private string FrameSource => EmbedUrl ?? string.Empty;

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_lastLoggerFactory, LoggerFactory))
        {
            _lastLoggerFactory = LoggerFactory;
            Logger = (LoggerFactory ?? NullLoggerFactory.Instance).CreateLogger<EmbeddedVideoPlayerComponent>();
        }
    }

    private async Task OnFrameLoadedAsync()
    {
        await PlayerReady.InvokeAsync();
    }

    public ValueTask<bool> PlayAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Play is not supported in embedded mode.", nameof(PlayAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> PauseAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Pause is not supported in embedded mode.", nameof(PauseAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> TogglePlayPauseAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Toggle is not supported in embedded mode.", nameof(TogglePlayPauseAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> SeekAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Seek is not supported in embedded mode.", nameof(SeekAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> PlayFromAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Play-from is not supported in embedded mode.", nameof(PlayFromAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> SetVolumeAsync(double volume, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Volume is not supported in embedded mode.", nameof(SetVolumeAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> SetPlaybackRateAsync(double playbackRate, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Playback rate is not supported in embedded mode.", nameof(SetPlaybackRateAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> SetMutedAsync(bool isMuted, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Mute is not supported in embedded mode.", nameof(SetMutedAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> SetQualityAsync(VideoQualityOption quality, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Quality selection is not supported in embedded mode.", nameof(SetQualityAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> SetCaptionAsync(string? captionId, CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Captions are not supported in embedded mode.", nameof(SetCaptionAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<bool> ReloadAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("[{MethodName}] Reload is not supported in embedded mode.", nameof(ReloadAsync));
        return new ValueTask<bool>(false);
    }

    public ValueTask<PlaybackProgress?> GetPlaybackProgressAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<PlaybackProgress?>((PlaybackProgress?)null);
    }

    public ValueTask<BufferingProgress?> GetBufferingProgressAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<BufferingProgress?>((BufferingProgress?)null);
    }

    public ValueTask<PlaybackStatistics?> GetPlaybackStatisticsAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<PlaybackStatistics?>((PlaybackStatistics?)null);
    }
}
