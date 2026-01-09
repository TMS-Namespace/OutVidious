using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Enums;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interfaces;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Components;

/// <summary>
/// Coordinator component that renders a unified player experience across variants.
/// </summary>
public partial class CombinedVideoPlayerComponent : ComponentBase, IPlayer, IDisposable
{
    private const double DefaultVolume = 1.0;
    private const double DefaultPlaybackRate = 1.0;

    private NativeVideoPlayerComponent? _nativePlayer;
    private DashVideoPlayerComponent? _dashPlayer;
    private EmbeddedVideoPlayerComponent? _embeddedPlayer;
    private string? _lastSourceSignature;
    private bool _isLoading;
    private bool _isReady;
    private bool _isPaused = true;
    private bool _hasStartedPlayback;
    private string? _playerErrorMessage;
    private CancellationTokenSource? _cts;
    private ILoggerFactory? _lastLoggerFactory;

    private ILogger<CombinedVideoPlayerComponent> Logger { get; set; } = NullLogger<CombinedVideoPlayerComponent>.Instance;

    [Parameter]
    public ILoggerFactory? LoggerFactory { get; set; }

    [Parameter]
    public VideoPlayerVariant Variant { get; set; } = VideoPlayerVariant.Native;

    [Parameter]
    public string? StreamUrl { get; set; }

    [Parameter]
    public string? DashManifestUrl { get; set; }

    [Parameter]
    public string? EmbedUrl { get; set; }

    [Parameter]
    public string? PosterUrl { get; set; }

    [Parameter]
    public bool AutoPlay { get; set; }

    [Parameter]
    public bool IsMuted { get; set; }

    [Parameter]
    public double Volume { get; set; } = DefaultVolume;

    [Parameter]
    public double PlaybackRate { get; set; } = DefaultPlaybackRate;

    [Parameter]
    public bool ShowNativeControls { get; set; } = true;

    [Parameter]
    public TimeSpan? StartPosition { get; set; }

    [Parameter]
    public TimeSpan StatisticsUpdateInterval { get; set; } = TimeSpan.FromSeconds(CombinedVideoPlayerConstants.DefaultStatisticsUpdateIntervalSeconds);

    [Parameter]
    public IReadOnlyList<CaptionTrack> Captions { get; set; } = [];

    [Parameter]
    public string? SelectedCaptionId { get; set; }

    [Parameter]
    public IReadOnlyList<VideoQualityOption> AvailableQualities { get; set; } = [];

    [Parameter]
    public VideoQualityOption? SelectedQuality { get; set; }

    [Parameter]
    public CombinedVideoPlayerAppearanceOptions Appearance { get; set; } = new();

    [Parameter]
    public CombinedVideoPlayerBehaviorOptions Behavior { get; set; } = new();

    [Parameter]
    public string FrameTitle { get; set; } = CombinedVideoPlayerText.DefaultFrameTitle;

    [Parameter]
    public EventCallback PlayerReady { get; set; }

    [Parameter]
    public EventCallback<PlaybackProgress> PlaybackProgressChanged { get; set; }

    [Parameter]
    public EventCallback<BufferingProgress> BufferingProgressChanged { get; set; }

    [Parameter]
    public EventCallback<PlaybackStatistics> PlaybackStatisticsChanged { get; set; }

    [Parameter]
    public EventCallback<bool> LoadingStateChanged { get; set; }

    [Parameter]
    public EventCallback<bool> BufferingStateChanged { get; set; }

    [Parameter]
    public EventCallback<string> PlayerError { get; set; }

    [Parameter]
    public EventCallback<PlayerMouseEventArgs> SurfaceClick { get; set; }

    [Parameter]
    public EventCallback<PlayerMouseEventArgs> SurfaceDoubleClick { get; set; }

    [Parameter]
    public EventCallback<PlayerMouseEventArgs> SurfaceContextMenu { get; set; }

    [Parameter]
    public EventCallback<PlayerWheelEventArgs> SurfaceWheel { get; set; }

    [Parameter]
    public EventCallback PlaybackStarted { get; set; }

    [Parameter]
    public EventCallback PlaybackPaused { get; set; }

    public PlayerCapabilities Capabilities => Variant switch
    {
        VideoPlayerVariant.Native => PlayerCapabilities.Native,
        VideoPlayerVariant.Dash => PlayerCapabilities.Dash,
        _ => PlayerCapabilities.Embedded
    };

    private IPlayer? ActivePlayer => Variant switch
    {
        VideoPlayerVariant.Native => _nativePlayer,
        VideoPlayerVariant.Dash => _dashPlayer,
        VideoPlayerVariant.Embedded => _embeddedPlayer,
        _ => null
    };

    private string ContainerClass => Appearance.ContainerClass;

    private string SurfaceClass => Appearance.UseRoundedCorners
        ? $"{Appearance.SurfaceClass} rounded"
        : Appearance.SurfaceClass;

    private string SurfaceStyle => BuildSurfaceStyle();

    private string PausedOverlayStyle => $"--combined-video-player-paused-overlay: rgba(0, 0, 0, {Behavior.PausedOverlayOpacity})";

    private string PlayButtonStyle => BuildPlayButtonStyle();

    /// <summary>
    /// Gets the modified embed URL with Invidious-specific parameters applied based on behavior options.
    /// </summary>
    private string? ModifiedEmbedUrl => BuildModifiedEmbedUrl();

    private bool IsLoading => _isLoading;

    private bool IsReady => _isReady;

    private bool IsPaused => _isPaused;

    private bool IsEmbedded => Variant == VideoPlayerVariant.Embedded;

    /// <summary>
    /// Show poster when: loading with poster enabled, OR ready but not yet started playback (if ShowPosterUntilPlay is true).
    /// For embedded players, we hide poster once playback has started (user clicked).
    /// </summary>
    private bool ShouldShowPoster =>
        !string.IsNullOrWhiteSpace(PosterUrl) &&
        ((Behavior.ShowPosterWhileLoading && IsLoading) ||
         (Behavior.ShowPosterUntilPlay && IsReady && !_hasStartedPlayback && !AutoPlay));

    /// <summary>
    /// Show paused overlay when: ready, paused, not loading, has started playback, and overlay is enabled.
    /// Note: For embedded players, we cannot track pause state, so overlay is only shown before first play.
    /// </summary>
    private bool ShouldShowPausedOverlay =>
        Behavior.ShowPausedOverlay &&
        IsReady &&
        IsPaused &&
        !IsLoading &&
        _hasStartedPlayback &&
        !IsEmbedded;

    /// <summary>
    /// Show the play button overlay when: ready, not loading, not playing (either never started or paused).
    /// This covers both the "ready to play" state and the "paused" state.
    /// For embedded players, only show before first play (can't track pause state).
    /// </summary>
    private bool ShouldShowPlayButton =>
        IsReady &&
        !IsLoading &&
        ((IsEmbedded && !_hasStartedPlayback) ||
         (!IsEmbedded && (IsPaused || !_hasStartedPlayback)));

    private string? MissingSourceMessage => Variant switch
    {
        VideoPlayerVariant.Native => string.IsNullOrWhiteSpace(StreamUrl)
            ? CombinedVideoPlayerText.MissingSourceMessage
            : null,
        VideoPlayerVariant.Dash => string.IsNullOrWhiteSpace(DashManifestUrl)
            ? CombinedVideoPlayerText.MissingDashManifestMessage
            : null,
        VideoPlayerVariant.Embedded => string.IsNullOrWhiteSpace(EmbedUrl)
            ? CombinedVideoPlayerText.MissingEmbedMessage
            : null,
        _ => CombinedVideoPlayerText.MissingSourceMessage
    };

    private string? PlayerErrorMessage => _playerErrorMessage;

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_lastLoggerFactory, LoggerFactory))
        {
            _lastLoggerFactory = LoggerFactory;
            Logger = (LoggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CombinedVideoPlayerComponent>();
        }

        var sourceSignature = GetSourceSignature();
        if (!string.Equals(_lastSourceSignature, sourceSignature, StringComparison.Ordinal))
        {
            _lastSourceSignature = sourceSignature;
            _isLoading = !string.IsNullOrWhiteSpace(sourceSignature);
            _isReady = false;
            _isPaused = true;
            _hasStartedPlayback = false;
            _playerErrorMessage = null;
            _ = NotifyLoadingStateChangedAsync(_isLoading, _cts?.Token ?? CancellationToken.None);
        }
    }

    private string? GetSourceSignature()
    {
        return Variant switch
        {
            VideoPlayerVariant.Native => StreamUrl,
            VideoPlayerVariant.Dash => DashManifestUrl,
            VideoPlayerVariant.Embedded => EmbedUrl,
            _ => StreamUrl
        };
    }

    private string BuildSurfaceStyle()
    {
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(Appearance.SurfaceStyle))
        {
            segments.Add(Appearance.SurfaceStyle);
        }

        if (!string.IsNullOrWhiteSpace(Appearance.AspectRatio))
        {
            segments.Add($"--combined-video-player-aspect-ratio: {Appearance.AspectRatio}");
        }

        return string.Join("; ", segments);
    }

    private string BuildPlayButtonStyle()
    {
        var bgOpacity = Behavior.PlayButtonOpacity;
        var hoverOpacity = Behavior.PlayButtonHoverOpacity;

        return $"--combined-video-player-play-button-bg: rgba(255, 255, 255, {bgOpacity}); " +
               $"--combined-video-player-play-button-hover-bg: rgba(255, 255, 255, {hoverOpacity})";
    }

    /// <summary>
    /// Builds the modified embed URL with Invidious-specific parameters applied.
    /// Note: Invidious has limited support for customization parameters.
    /// We keep autoplay=1 but hide the player behind our poster/overlay until user clicks.
    /// </summary>
    private string? BuildModifiedEmbedUrl()
    {
        if (string.IsNullOrWhiteSpace(EmbedUrl))
        {
            return null;
        }

        try
        {
            var uriBuilder = new UriBuilder(EmbedUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            // Keep autoplay=1 (video plays behind our poster/overlay)
            // When user clicks our overlay, we just hide it to reveal the already-playing video
            // This avoids Invidious showing its own play button UI
            query["autoplay"] = "1";

            // local=true ensures streams are proxied through Invidious (avoids CORS)
            query["local"] = "true";

            // rel=0 disables related videos at end
            query["rel"] = "0";

            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[{MethodName}] Failed to parse embed URL '{Url}'. Using original.", nameof(BuildModifiedEmbedUrl), EmbedUrl);
            return EmbedUrl;
        }
    }

    private Task OnPlayerReadyAsync()
    {
        _isLoading = false;
        _isReady = true;
        _playerErrorMessage = null;
        _ = NotifyLoadingStateChangedAsync(false, _cts?.Token ?? CancellationToken.None);
        return PlayerReady.InvokeAsync();
    }

    private async Task OnPlaybackProgressChangedAsync(PlaybackProgress progress)
    {
        var wasPaused = _isPaused;
        _isPaused = progress.IsPaused;

        if (!progress.IsPaused && !_hasStartedPlayback)
        {
            _hasStartedPlayback = true;
        }

        if (wasPaused && !_isPaused)
        {
            await PlaybackStarted.InvokeAsync();
        }
        else if (!wasPaused && _isPaused)
        {
            await PlaybackPaused.InvokeAsync();
        }

        await PlaybackProgressChanged.InvokeAsync(progress);
    }

    private Task OnBufferingProgressChangedAsync(BufferingProgress progress)
    {
        return BufferingProgressChanged.InvokeAsync(progress);
    }

    private Task OnBufferingStateChangedAsync(bool isBuffering)
    {
        return BufferingStateChanged.InvokeAsync(isBuffering);
    }

    private Task OnPlaybackStatisticsChangedAsync(PlaybackStatistics statistics)
    {
        return PlaybackStatisticsChanged.InvokeAsync(statistics);
    }

    private async Task OnPlayerErrorAsync(string errorMessage)
    {
        _playerErrorMessage = errorMessage;
        _isLoading = false;
        _ = NotifyLoadingStateChangedAsync(false, _cts?.Token ?? CancellationToken.None);
        await PlayerError.InvokeAsync(errorMessage);
    }

    private async Task OnSurfaceClickAsync(MouseEventArgs e)
    {
        await SurfaceClick.InvokeAsync(new PlayerMouseEventArgs
        {
            ClientX = e.ClientX,
            ClientY = e.ClientY,
            OffsetX = e.OffsetX,
            OffsetY = e.OffsetY,
            Button = (int)e.Button,
            CtrlKey = e.CtrlKey,
            ShiftKey = e.ShiftKey,
            AltKey = e.AltKey
        });
    }

    private async Task OnOverlayClickAsync(MouseEventArgs e)
    {
        // For embedded players, we can't control playback or track state,
        // so we just mark as started and hide the overlay/poster
        if (IsEmbedded)
        {
            _hasStartedPlayback = true;
            _isPaused = false;
            await PlaybackStarted.InvokeAsync();
            StateHasChanged();
        }

        await OnSurfaceClickAsync(e);
    }

    private Task OnSurfaceDoubleClickAsync(MouseEventArgs e)
    {
        return SurfaceDoubleClick.InvokeAsync(new PlayerMouseEventArgs
        {
            ClientX = e.ClientX,
            ClientY = e.ClientY,
            OffsetX = e.OffsetX,
            OffsetY = e.OffsetY,
            Button = (int)e.Button,
            CtrlKey = e.CtrlKey,
            ShiftKey = e.ShiftKey,
            AltKey = e.AltKey
        });
    }

    private Task OnSurfaceContextMenuAsync(MouseEventArgs e)
    {
        return SurfaceContextMenu.InvokeAsync(new PlayerMouseEventArgs
        {
            ClientX = e.ClientX,
            ClientY = e.ClientY,
            OffsetX = e.OffsetX,
            OffsetY = e.OffsetY,
            Button = (int)e.Button,
            CtrlKey = e.CtrlKey,
            ShiftKey = e.ShiftKey,
            AltKey = e.AltKey
        });
    }

    private Task OnSurfaceWheelAsync(WheelEventArgs e)
    {
        return SurfaceWheel.InvokeAsync(new PlayerWheelEventArgs
        {
            ClientX = e.ClientX,
            ClientY = e.ClientY,
            DeltaX = e.DeltaX,
            DeltaY = e.DeltaY,
            CtrlKey = e.CtrlKey,
            ShiftKey = e.ShiftKey,
            AltKey = e.AltKey
        });
    }

    private async Task NotifyLoadingStateChangedAsync(bool isLoading, CancellationToken cancellationToken)
    {
        if (!LoadingStateChanged.HasDelegate)
        {
            return;
        }

        try
        {
            await LoadingStateChanged.InvokeAsync(isLoading);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to notify loading state '{LoadingState}'.",
                nameof(NotifyLoadingStateChangedAsync),
                isLoading);
        }
    }

    public async ValueTask<bool> PlayAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for play.", nameof(PlayAsync));
            return false;
        }

        return await player.PlayAsync(cancellationToken);
    }

    public async ValueTask<bool> PauseAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for pause.", nameof(PauseAsync));
            return false;
        }

        return await player.PauseAsync(cancellationToken);
    }

    public async ValueTask<bool> TogglePlayPauseAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for toggle.", nameof(TogglePlayPauseAsync));
            return false;
        }

        return await player.TogglePlayPauseAsync(cancellationToken);
    }

    public async ValueTask<bool> SeekAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for seek.", nameof(SeekAsync));
            return false;
        }

        return await player.SeekAsync(position, cancellationToken);
    }

    public async ValueTask<bool> PlayFromAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for play-from.", nameof(PlayFromAsync));
            return false;
        }

        return await player.PlayFromAsync(position, cancellationToken);
    }

    public async ValueTask<bool> SetVolumeAsync(double volume, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for volume change.", nameof(SetVolumeAsync));
            return false;
        }

        return await player.SetVolumeAsync(volume, cancellationToken);
    }

    public async ValueTask<bool> SetPlaybackRateAsync(double playbackRate, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for playback rate change.", nameof(SetPlaybackRateAsync));
            return false;
        }

        return await player.SetPlaybackRateAsync(playbackRate, cancellationToken);
    }

    public async ValueTask<bool> SetMutedAsync(bool isMuted, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for mute change.", nameof(SetMutedAsync));
            return false;
        }

        return await player.SetMutedAsync(isMuted, cancellationToken);
    }

    public async ValueTask<bool> SetQualityAsync(VideoQualityOption quality, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for quality change.", nameof(SetQualityAsync));
            return false;
        }

        return await player.SetQualityAsync(quality, cancellationToken);
    }

    public async ValueTask<bool> SetCaptionAsync(string? captionId, CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for caption change.", nameof(SetCaptionAsync));
            return false;
        }

        return await player.SetCaptionAsync(captionId, cancellationToken);
    }

    public async ValueTask<bool> ReloadAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for reload.", nameof(ReloadAsync));
            return false;
        }

        return await player.ReloadAsync(cancellationToken);
    }

    public async ValueTask<PlaybackProgress?> GetPlaybackProgressAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for progress.", nameof(GetPlaybackProgressAsync));
            return null;
        }

        return await player.GetPlaybackProgressAsync(cancellationToken);
    }

    public async ValueTask<BufferingProgress?> GetBufferingProgressAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for buffering progress.", nameof(GetBufferingProgressAsync));
            return null;
        }

        return await player.GetBufferingProgressAsync(cancellationToken);
    }

    public async ValueTask<PlaybackStatistics?> GetPlaybackStatisticsAsync(CancellationToken cancellationToken)
    {
        var player = ActivePlayer;
        if (player is null)
        {
            Logger.LogDebug("[{MethodName}] Active player not ready for statistics.", nameof(GetPlaybackStatisticsAsync));
            return null;
        }

        return await player.GetPlaybackStatisticsAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (_cts is null)
        {
            return;
        }

        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        _cts.Dispose();
        _cts = null;
    }
}
