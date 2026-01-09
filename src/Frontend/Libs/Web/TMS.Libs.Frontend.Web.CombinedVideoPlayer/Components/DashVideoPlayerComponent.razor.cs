using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Enums;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interfaces;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Interop;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

namespace TMS.Libs.Frontend.Web.CombinedVideoPlayer.Components;

/// <summary>
/// DASH video player implementation backed by Shaka Player.
/// </summary>
public partial class DashVideoPlayerComponent : ComponentBase, IPlayer, IAsyncDisposable
{
    private const double MinVolume = 0.0;
    private const double MaxVolume = 1.0;
    private const double MinPlaybackRate = 0.1;

    private readonly string _instanceId = Guid.NewGuid().ToString("N")[..8];
    private DotNetObjectReference<DashVideoPlayerComponent>? _dotNetRef;
    private IJSObjectReference? _jsModule;
    private bool _isDisposed;
    private bool _isRegistered;
    private bool _pendingInitialization;
    private bool _pendingSettings;
    private bool _startPositionApplied;
    private bool _isReady;
    private string? _lastManifestUrl;
    private TimeSpan? _lastStartPosition;
    private VideoQualityOption? _lastSelectedQuality;
    private double _lastVolume = -1;
    private double _lastPlaybackRate = -1;
    private bool _lastMuted;
    private string? _lastCaptionId;
    private DateTimeOffset _lastStatisticsUpdateAt = DateTimeOffset.MinValue;
    private CancellationTokenSource? _cts;
    private ILoggerFactory? _lastLoggerFactory;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private ILogger<DashVideoPlayerComponent> Logger { get; set; } = NullLogger<DashVideoPlayerComponent>.Instance;

    [Parameter]
    public ILoggerFactory? LoggerFactory { get; set; }

    [Parameter]
    public string? ManifestUrl { get; set; }

    [Parameter]
    public string? PosterUrl { get; set; }

    [Parameter]
    public bool AutoPlay { get; set; }

    [Parameter]
    public bool IsMuted { get; set; }

    [Parameter]
    public double Volume { get; set; } = 1.0;

    [Parameter]
    public double PlaybackRate { get; set; } = 1.0;

    [Parameter]
    public bool ShowNativeControls { get; set; } = true;

    [Parameter]
    public TimeSpan? StartPosition { get; set; }

    [Parameter]
    public IReadOnlyList<CaptionTrack> Captions { get; set; } = [];

    [Parameter]
    public string? SelectedCaptionId { get; set; }

    [Parameter]
    public IReadOnlyList<VideoQualityOption> AvailableQualities { get; set; } = [];

    [Parameter]
    public VideoQualityOption? SelectedQuality { get; set; }

    [Parameter]
    public string? PlayerClass { get; set; }

    [Parameter]
    public string? PlayerStyle { get; set; }

    [Parameter]
    public EventCallback PlayerReady { get; set; }

    [Parameter]
    public EventCallback<PlaybackProgress> PlaybackProgressChanged { get; set; }

    [Parameter]
    public EventCallback<BufferingProgress> BufferingProgressChanged { get; set; }

    [Parameter]
    public EventCallback<bool> BufferingStateChanged { get; set; }

    [Parameter]
    public EventCallback<PlaybackStatistics> PlaybackStatisticsChanged { get; set; }

    [Parameter]
    public TimeSpan StatisticsUpdateInterval { get; set; } = TimeSpan.FromSeconds(CombinedVideoPlayerConstants.DefaultStatisticsUpdateIntervalSeconds);

    [Parameter]
    public EventCallback<string> PlayerError { get; set; }

    public VideoPlayerVariant Variant => VideoPlayerVariant.Dash;

    public PlayerCapabilities Capabilities => PlayerCapabilities.Dash;

    private string VideoElementId => $"dash-video-{_instanceId}";

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_lastLoggerFactory, LoggerFactory))
        {
            _lastLoggerFactory = LoggerFactory;
            Logger = (LoggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DashVideoPlayerComponent>();
        }

        if (!string.Equals(_lastManifestUrl, ManifestUrl, StringComparison.Ordinal))
        {
            _lastManifestUrl = ManifestUrl;
            _pendingInitialization = true;
            _startPositionApplied = false;
            _isReady = false;
            _pendingSettings = true;
        }

        if (_lastStartPosition != StartPosition)
        {
            _lastStartPosition = StartPosition;
            _startPositionApplied = false;
        }

        if (!Equals(_lastSelectedQuality, SelectedQuality) ||
            !string.Equals(_lastCaptionId, SelectedCaptionId, StringComparison.Ordinal) ||
            Math.Abs(_lastVolume - Volume) > double.Epsilon ||
            Math.Abs(_lastPlaybackRate - PlaybackRate) > double.Epsilon ||
            _lastMuted != IsMuted)
        {
            _pendingSettings = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_isDisposed)
        {
            return;
        }

        var token = _cts?.Token ?? CancellationToken.None;

        if (!_isRegistered)
        {
            await RegisterAsync(token);
        }

        if (_pendingInitialization)
        {
            _pendingInitialization = false;
            await InitializeDashAsync(token);
        }

        if (_pendingSettings)
        {
            _pendingSettings = false;
            await ApplyPlaybackSettingsAsync(token);
        }
    }

    private async Task RegisterAsync(CancellationToken cancellationToken)
    {
        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return;
        }

        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            var registered = await module.InvokeAsync<bool>(
                CombinedVideoPlayerJsInteropConstants.RegisterVideoElement,
                cancellationToken,
                VideoElementId,
                _dotNetRef);
            _isRegistered = registered;
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to register DASH player element '{ElementId}'.",
                nameof(RegisterAsync),
                VideoElementId);
        }
    }

    private async Task InitializeDashAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ManifestUrl))
        {
            return;
        }

        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return;
        }

        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync(
                CombinedVideoPlayerJsInteropConstants.DestroyDashPlayer,
                cancellationToken,
                VideoElementId);

            var success = await module.InvokeAsync<bool>(
                CombinedVideoPlayerJsInteropConstants.InitializeDashPlayer,
                cancellationToken,
                VideoElementId,
                ManifestUrl,
                _dotNetRef);

            if (!success)
            {
                await PlayerError.InvokeAsync(CombinedVideoPlayerText.DashInitializationFailedMessage);
            }
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to initialize DASH player for '{ElementId}'.",
                nameof(InitializeDashAsync),
                VideoElementId);
        }
    }

    private async Task ApplyPlaybackSettingsAsync(CancellationToken cancellationToken)
    {
        _lastSelectedQuality = SelectedQuality;
        _lastVolume = Volume;
        _lastPlaybackRate = PlaybackRate;
        _lastMuted = IsMuted;
        _lastCaptionId = SelectedCaptionId;

        if (SelectedQuality is not null)
        {
            await SetQualityAsync(SelectedQuality, cancellationToken);
        }

        await SetVolumeAsync(Volume, cancellationToken);
        await SetPlaybackRateAsync(PlaybackRate, cancellationToken);
        await SetMutedAsync(IsMuted, cancellationToken);
        await SetCaptionAsync(SelectedCaptionId, cancellationToken);
    }

    [JSInvokable]
    public async Task OnPlayerReady()
    {
        if (_isReady)
        {
            return;
        }

        _isReady = true;

        if (!_startPositionApplied && StartPosition.HasValue)
        {
            _startPositionApplied = true;
            await SeekAsync(StartPosition.Value, _cts?.Token ?? CancellationToken.None);
            if (AutoPlay)
            {
                await PlayAsync(_cts?.Token ?? CancellationToken.None);
            }
        }

        await PlayerReady.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnPlaybackProgressChanged(PlaybackProgressPayload payload)
    {
        var progress = MapPlaybackProgress(payload);
        await PlaybackProgressChanged.InvokeAsync(progress);
        await TryReportStatisticsAsync(_cts?.Token ?? CancellationToken.None);
    }

    [JSInvokable]
    public async Task OnBufferingProgressChanged(BufferingProgressPayload payload)
    {
        var progress = MapBufferingProgress(payload);
        await BufferingProgressChanged.InvokeAsync(progress);
    }

    [JSInvokable]
    public async Task OnBufferingStateChanged(bool isBuffering)
    {
        await BufferingStateChanged.InvokeAsync(isBuffering);
    }

    [JSInvokable]
    public async Task OnPlayerError(string errorMessage)
    {
        await PlayerError.InvokeAsync(errorMessage);
    }

    public async ValueTask<bool> PlayAsync(CancellationToken cancellationToken)
    {
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.Play,
            cancellationToken,
            VideoElementId);
    }

    public async ValueTask<bool> PauseAsync(CancellationToken cancellationToken)
    {
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.Pause,
            cancellationToken,
            VideoElementId);
    }

    public async ValueTask<bool> TogglePlayPauseAsync(CancellationToken cancellationToken)
    {
        var progress = await GetPlaybackProgressAsync(cancellationToken);
        if (progress is null)
        {
            return false;
        }

        return progress.IsPaused
            ? await PlayAsync(cancellationToken)
            : await PauseAsync(cancellationToken);
    }

    public async ValueTask<bool> SeekAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var seconds = Math.Max(position.TotalSeconds, 0);
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.Seek,
            cancellationToken,
            VideoElementId,
            seconds);
    }

    public async ValueTask<bool> PlayFromAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        var seekResult = await SeekAsync(position, cancellationToken);
        if (!seekResult)
        {
            return false;
        }

        return await PlayAsync(cancellationToken);
    }

    public async ValueTask<bool> SetVolumeAsync(double volume, CancellationToken cancellationToken)
    {
        var clamped = Math.Clamp(volume, MinVolume, MaxVolume);
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.SetVolume,
            cancellationToken,
            VideoElementId,
            clamped);
    }

    public async ValueTask<bool> SetPlaybackRateAsync(double playbackRate, CancellationToken cancellationToken)
    {
        var clamped = Math.Max(playbackRate, MinPlaybackRate);
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.SetPlaybackRate,
            cancellationToken,
            VideoElementId,
            clamped);
    }

    public async ValueTask<bool> SetMutedAsync(bool isMuted, CancellationToken cancellationToken)
    {
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.SetMuted,
            cancellationToken,
            VideoElementId,
            isMuted);
    }

    public async ValueTask<bool> SetQualityAsync(VideoQualityOption quality, CancellationToken cancellationToken)
    {
        if (quality.IsAuto)
        {
            return await InvokeJsBooleanAsync(
                CombinedVideoPlayerJsInteropConstants.SetDashAutoQuality,
                cancellationToken,
                VideoElementId);
        }

        if (!quality.Height.HasValue)
        {
            Logger.LogDebug(
                "[{MethodName}] DASH quality missing height; skipping selection.",
                nameof(SetQualityAsync));
            return false;
        }

        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.SetDashMaxResolution,
            cancellationToken,
            VideoElementId,
            quality.Height.Value);
    }

    public async ValueTask<bool> SetCaptionAsync(string? captionId, CancellationToken cancellationToken)
    {
        return await InvokeJsBooleanAsync(
            CombinedVideoPlayerJsInteropConstants.SetCaption,
            cancellationToken,
            VideoElementId,
            captionId);
    }

    public async ValueTask<bool> ReloadAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ManifestUrl))
        {
            return false;
        }

        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return false;
        }

        try
        {
            await module.InvokeVoidAsync(
                CombinedVideoPlayerJsInteropConstants.DestroyDashPlayer,
                cancellationToken,
                VideoElementId);
            _dotNetRef ??= DotNetObjectReference.Create(this);
            var success = await module.InvokeAsync<bool>(
                CombinedVideoPlayerJsInteropConstants.InitializeDashPlayer,
                cancellationToken,
                VideoElementId,
                ManifestUrl,
                _dotNetRef);
            return success;
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to reload DASH player '{ElementId}'.",
                nameof(ReloadAsync),
                VideoElementId);
            return false;
        }
    }

    public async ValueTask<PlaybackProgress?> GetPlaybackProgressAsync(CancellationToken cancellationToken)
    {
        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return null;
        }

        try
        {
            var payload = await module.InvokeAsync<PlaybackProgressPayload?>(
                CombinedVideoPlayerJsInteropConstants.GetPlaybackProgress,
                cancellationToken,
                VideoElementId);
            return payload is null ? null : MapPlaybackProgress(payload);
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to get playback progress for '{ElementId}'.",
                nameof(GetPlaybackProgressAsync),
                VideoElementId);
            return null;
        }
    }

    public async ValueTask<BufferingProgress?> GetBufferingProgressAsync(CancellationToken cancellationToken)
    {
        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return null;
        }

        try
        {
            var payload = await module.InvokeAsync<BufferingProgressPayload?>(
                CombinedVideoPlayerJsInteropConstants.GetBufferingProgress,
                cancellationToken,
                VideoElementId);
            return payload is null ? null : MapBufferingProgress(payload);
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to get buffering progress for '{ElementId}'.",
                nameof(GetBufferingProgressAsync),
                VideoElementId);
            return null;
        }
    }

    public async ValueTask<PlaybackStatistics?> GetPlaybackStatisticsAsync(CancellationToken cancellationToken)
    {
        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return null;
        }

        try
        {
            var payload = await module.InvokeAsync<PlaybackStatisticsPayload?>(
                CombinedVideoPlayerJsInteropConstants.GetPlaybackStatistics,
                cancellationToken,
                VideoElementId);
            return payload is null ? null : MapPlaybackStatistics(payload);
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to get playback statistics for '{ElementId}'.",
                nameof(GetPlaybackStatisticsAsync),
                VideoElementId);
            return null;
        }
    }

    private async ValueTask<bool> InvokeJsBooleanAsync(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] args)
    {
        var module = await GetModuleAsync(cancellationToken);
        if (module is null)
        {
            return false;
        }

        try
        {
            return await module.InvokeAsync<bool>(identifier, cancellationToken, args);
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: JS call '{Identifier}' failed for '{ElementId}'.",
                nameof(InvokeJsBooleanAsync),
                identifier,
                VideoElementId);
            return false;
        }
    }

    private static PlaybackProgress MapPlaybackProgress(PlaybackProgressPayload payload)
    {
        var duration = TimeSpan.FromSeconds(payload.DurationSeconds);
        var position = TimeSpan.FromSeconds(payload.PositionSeconds);
        return new PlaybackProgress
        {
            Duration = duration,
            Position = position,
            ProgressRatio = payload.DurationSeconds > 0 ? payload.PositionSeconds / payload.DurationSeconds : null,
            IsPaused = payload.IsPaused,
            IsBuffering = payload.IsBuffering,
            PlaybackRate = payload.PlaybackRate,
            Volume = payload.Volume,
            IsMuted = payload.IsMuted
        };
    }

    private static BufferingProgress MapBufferingProgress(BufferingProgressPayload payload)
    {
        var ranges = payload.Ranges
            .Select(range => new BufferedRange
            {
                Start = TimeSpan.FromSeconds(range.StartSeconds),
                End = TimeSpan.FromSeconds(range.EndSeconds)
            })
            .ToList();

        return new BufferingProgress
        {
            BufferedUntil = payload.BufferedUntilSeconds.HasValue
                ? TimeSpan.FromSeconds(payload.BufferedUntilSeconds.Value)
                : null,
            BufferedRatio = payload.BufferedRatio,
            Ranges = ranges
        };
    }

    private static PlaybackStatistics MapPlaybackStatistics(PlaybackStatisticsPayload payload)
    {
        return new PlaybackStatistics
        {
            DroppedFrames = payload.DroppedFrames,
            TotalFrames = payload.TotalFrames,
            EstimatedBandwidthKbps = payload.EstimatedBandwidthKbps,
            StreamBandwidthKbps = payload.StreamBandwidthKbps,
            BufferingSeconds = payload.BufferingSeconds,
            Width = payload.Width,
            Height = payload.Height
        };
    }

    private async Task TryReportStatisticsAsync(CancellationToken cancellationToken)
    {
        if (!PlaybackStatisticsChanged.HasDelegate)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (now - _lastStatisticsUpdateAt < StatisticsUpdateInterval)
        {
            return;
        }

        _lastStatisticsUpdateAt = now;

        var statistics = await GetPlaybackStatisticsAsync(cancellationToken);
        if (statistics is null)
        {
            return;
        }

        await PlaybackStatisticsChanged.InvokeAsync(statistics);
    }

    private async ValueTask<IJSObjectReference?> GetModuleAsync(CancellationToken cancellationToken)
    {
        if (_jsModule is not null)
        {
            return _jsModule;
        }

        try
        {
            _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>(
                CombinedVideoPlayerJsInteropConstants.Import,
                cancellationToken,
                CombinedVideoPlayerJsInteropConstants.ModulePath);
            return _jsModule;
        }
        catch (JSException ex)
        {
            Logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to load JS module for '{ElementId}'.",
                nameof(GetModuleAsync),
                VideoElementId);
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_cts is not null)
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            _cts.Dispose();
            _cts = null;
        }

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync(
                    CombinedVideoPlayerJsInteropConstants.DestroyDashPlayer,
                    CancellationToken.None,
                    VideoElementId);
                await _jsModule.InvokeVoidAsync(
                    CombinedVideoPlayerJsInteropConstants.UnregisterVideoElement,
                    CancellationToken.None,
                    VideoElementId);
            }
            catch (JSException ex)
            {
                Logger.LogError(
                    ex,
                    "[{MethodName}] Unexpected error: Failed to unregister DASH player '{ElementId}'.",
                    nameof(DisposeAsync),
                    VideoElementId);
            }

            await _jsModule.DisposeAsync();
            _jsModule = null;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}
