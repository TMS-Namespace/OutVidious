using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Components;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Enums;
using TMS.Libs.Frontend.Web.CombinedVideoPlayer.Models;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Code-behind for the Invidious video player component.
/// Supports Native, DASH (Shaka Player), and Embedded player modes.
/// </summary>
public partial class VideoPlayerComponent : ComponentBase, IDisposable
{
    private const string AutoQualityLabel = "auto";

    private bool _disposed;
    private bool _playerLoading;
    private string? _playerError;
    private string? _selectedDashQuality = AutoQualityLabel;
    private Video? _lastViewModel;
    private CombinedVideoPlayerComponent? _combinedPlayer;

    [Inject]
    private ILogger<VideoPlayerComponent> Logger { get; set; } = default!;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = default!;

    [Parameter]
    public Video? ViewModel { get; set; }

    [Parameter]
    public string ProviderBaseUrl { get; set; } = string.Empty;

    private string? ChannelName => ViewModel?.Channel?.Name;

    private string? StreamUrl => ViewModel?.Streams?.CurrentStreamUrl;

    private string? DashManifestUrl => ViewModel?.Streams?.DashManifestUrl;

    private string? EmbedUrl => ViewModel?.Streams?.EmbedUrl;

    private string? PosterUrl => GetPosterUrl();

    private bool AutoPlay => false;

    private bool IsMuted => false;

    private double Volume => 1.0;

    private double PlaybackRate => 1.0;

    private TimeSpan? StartPosition => null;

    private bool ShowNativeControls => true;

    private IReadOnlyList<CaptionTrack> Captions => [];

    private string? SelectedCaptionId => null;

    private VideoPlayerVariant SelectedVariant => ViewModel?.Streams?.PlayerMode switch
    {
        PlayerMode.Dash => VideoPlayerVariant.Dash,
        PlayerMode.Embedded => VideoPlayerVariant.Embedded,
        _ => VideoPlayerVariant.Native
    };

    private IReadOnlyList<VideoQualityOption> AvailableQualities
    {
        get
        {
            if (ViewModel?.Streams is null)
            {
                return [];
            }

            return SelectedVariant == VideoPlayerVariant.Dash
                ? BuildQualityOptions(ViewModel.Streams.AvailableDashQualities, includeAuto: true)
                : BuildQualityOptions(ViewModel.Streams.AvailableQualities, includeAuto: false);
        }
    }

    private VideoQualityOption? SelectedQuality => SelectedVariant == VideoPlayerVariant.Dash
        ? BuildDashQualityOption(_selectedDashQuality)
        : null;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!ReferenceEquals(_lastViewModel, ViewModel))
        {
            if (_lastViewModel is not null)
            {
                _lastViewModel.StateChanged -= OnViewModelStateChanged;
            }

            _lastViewModel = ViewModel;
            _playerError = null;
            _playerLoading = false;
            _selectedDashQuality = AutoQualityLabel;
        }

        if (ViewModel != null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
            ViewModel.StateChanged += OnViewModelStateChanged;
        }
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task OnPlayerModeChangedAsync(PlayerMode mode)
    {
        Logger.LogDebug("[{MethodName}] Player mode changing from '{OldMode}' to '{NewMode}'.",
            nameof(OnPlayerModeChangedAsync),
            ViewModel?.Streams?.PlayerMode,
            mode);

        ViewModel?.Streams?.SetPlayerMode(mode);
        _playerError = null;

        if (mode == PlayerMode.Dash)
        {
            _selectedDashQuality = AutoQualityLabel;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void OnQualityChanged(string quality)
    {
        ViewModel?.Streams?.SetQuality(quality);
    }

    private async Task OnDashQualityChangedAsync(string quality)
    {
        _selectedDashQuality = quality;

        var option = BuildDashQualityOption(quality);
        if (option is null || _combinedPlayer is null)
        {
            return;
        }

        await _combinedPlayer.SetQualityAsync(option, CancellationToken.None);
    }

    private Task OnPlayerLoadingChangedAsync(bool isLoading)
    {
        _playerLoading = isLoading;
        if (isLoading)
        {
            _playerError = null;
        }
        return Task.CompletedTask;
    }

    private Task OnPlayerErrorAsync(string errorMessage)
    {
        _playerError = errorMessage;
        return Task.CompletedTask;
    }

    private async Task OnSurfaceClickAsync(PlayerMouseEventArgs args)
    {
        if (_combinedPlayer is null)
        {
            return;
        }

        Logger.LogDebug("[{MethodName}] Surface clicked, toggling play/pause.", nameof(OnSurfaceClickAsync));
        await _combinedPlayer.TogglePlayPauseAsync(CancellationToken.None);
    }

    private async Task OnSurfaceWheelAsync(PlayerWheelEventArgs args)
    {
        if (_combinedPlayer is null || ViewModel?.Streams is null)
        {
            return;
        }

        var volumeStep = 0.05;
        var currentVolume = Volume;
        var newVolume = args.DeltaY < 0
            ? Math.Min(1.0, currentVolume + volumeStep)
            : Math.Max(0.0, currentVolume - volumeStep);

        if (Math.Abs(newVolume - currentVolume) > 0.001)
        {
            Logger.LogDebug("[{MethodName}] Wheel scroll, adjusting volume from '{OldVolume}' to '{NewVolume}'.",
                nameof(OnSurfaceWheelAsync),
                currentVolume,
                newVolume);
            await _combinedPlayer.SetVolumeAsync(newVolume, CancellationToken.None);
        }
    }

    private async Task RetryPlaybackAsync()
    {
        if (_combinedPlayer is null)
        {
            return;
        }

        var success = await _combinedPlayer.ReloadAsync(CancellationToken.None);
        if (!success)
        {
            Logger.LogWarning("[{MethodName}] Reload request ignored or unsupported.", nameof(RetryPlaybackAsync));
        }
    }

    private static IReadOnlyList<VideoQualityOption> BuildQualityOptions(
        IReadOnlyList<string> qualityLabels,
        bool includeAuto)
    {
        var options = new List<VideoQualityOption>();

        if (includeAuto)
        {
            options.Add(new VideoQualityOption
            {
                Label = AutoQualityLabel,
                IsAuto = true
            });
        }

        options.AddRange(qualityLabels
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => new VideoQualityOption
            {
                Label = label,
                Height = ParseQualityHeight(label)
            }));

        return options;
    }

    private static VideoQualityOption? BuildDashQualityOption(string? quality)
    {
        if (string.IsNullOrWhiteSpace(quality))
        {
            return null;
        }

        if (string.Equals(quality, AutoQualityLabel, StringComparison.OrdinalIgnoreCase))
        {
            return new VideoQualityOption
            {
                Label = AutoQualityLabel,
                IsAuto = true
            };
        }

        return new VideoQualityOption
        {
            Label = quality,
            Height = ParseQualityHeight(quality)
        };
    }

    private static int? ParseQualityHeight(string qualityLabel)
    {
        var numericPart = new string(qualityLabel.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(numericPart, out var height) ? height : null;
    }

    private string? GetPosterUrl()
    {
        var thumbnailIdentity = ViewModel?.GetBestThumbnailIdentity();

        if (thumbnailIdentity is null)
        {
            return null;
        }

        var proxyUrl = thumbnailIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        return string.IsNullOrWhiteSpace(proxyUrl) ? null : proxyUrl;
    }

    /// <summary>
    /// Handles click on the channel link.
    /// Opens the channel in the Channel dock panel without navigation.
    /// </summary>
    private void OnChannelLinkClicked()
    {
        var channelId = ViewModel?.Channel?.RemoteIdentity?.RemoteId;
        if (string.IsNullOrWhiteSpace(channelId))
        {
            return;
        }

        Orchestrator.OpenChannel(channelId);
    }

    private static string FormatViewCount(long? count)
    {
        if (count == null)
        {
            return "N/A";
        }

        return count switch
        {
            >= 1_000_000_000 => $"{count / 1_000_000_000.0:F1}B",
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M",
            >= 1_000 => $"{count / 1_000.0:F1}K",
            _ => count.Value.ToString("N0")
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (ViewModel != null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
