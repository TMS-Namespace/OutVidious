using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using TMS.Apps.FrontTube.Backend.Core.Enums;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Code-behind for the Invidious video player component.
/// Supports Native, DASH (Shaka Player), and Embedded player modes.
/// </summary>
public partial class VideoPlayerComponent : ComponentBase, IAsyncDisposable
{
    private bool _disposed;
    private DotNetObjectReference<VideoPlayerComponent>? _dotNetRef;
    private bool _shakaPlayerLoading;
    private bool _shakaPlayerInitialized;
    private string? _shakaPlayerError;
    private string? _selectedDashQuality = "auto";
    private string? _currentDashQuality;
    private string _instanceId = Guid.NewGuid().ToString("N")[..8];
    private string? _lastInitializedManifestUrl;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ILogger<VideoPlayerComponent> Logger { get; set; } = default!;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = default!;

    [Parameter]
    public Backend.Core.ViewModels.Video? ViewModel { get; set; }

    [Parameter]
    public string ProviderBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Unique ID for the Shaka Player video element.
    /// </summary>
    private string ShakaVideoElementId => $"shaka-video-{_instanceId}";

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ViewModel != null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
            ViewModel.StateChanged += OnViewModelStateChanged;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // Initialize Shaka Player when in DASH mode (only once per manifest URL)
        if (ViewModel?.Streams?.PlayerMode == PlayerMode.Dash && 
            ViewModel.LoadState == VideoLoadState.Loaded &&
            !string.IsNullOrEmpty(ViewModel.Streams.DashManifestUrl) &&
            !_shakaPlayerLoading &&
            !_shakaPlayerInitialized &&
            string.IsNullOrEmpty(_shakaPlayerError) &&
            _lastInitializedManifestUrl != ViewModel.Streams.DashManifestUrl)
        {
            await InitializeShakaPlayerAsync();
        }
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task OnPlayerModeChangedAsync(PlayerMode mode)
    {
        Logger.LogDebug("Player mode changing from {OldMode} to {NewMode}", ViewModel?.Streams?.PlayerMode, mode);
        
        // Clean up Shaka player when switching away from DASH mode
        if (ViewModel?.Streams?.PlayerMode == PlayerMode.Dash && mode != PlayerMode.Dash)
        {
            await DestroyShakaPlayerAsync();
            _shakaPlayerInitialized = false;
            _lastInitializedManifestUrl = null;
        }

        ViewModel?.Streams?.SetPlayerMode(mode);
        _shakaPlayerError = null;
        _currentDashQuality = null;
        
        await InvokeAsync(StateHasChanged);
    }

    private void OnQualityChanged(string quality)
    {
        ViewModel?.Streams?.SetQuality(quality);
    }

    private async Task OnDashQualityChangedAsync(string quality)
    {
        _selectedDashQuality = quality;

        if (quality == "auto")
        {
            await JsRuntime.InvokeVoidAsync("shakaPlayerInterop.setQuality", ShakaVideoElementId, "auto");
        }
        else
        {
            // Parse quality label (e.g., "1080p" -> 1080)
            var heightStr = new string(quality.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(heightStr, out var height))
            {
                await JsRuntime.InvokeVoidAsync("shakaPlayerInterop.setMaxResolution", ShakaVideoElementId, height);
            }
        }
    }

    private async Task InitializeShakaPlayerAsync()
    {
        if (ViewModel?.Streams?.DashManifestUrl == null)
        {
            return;
        }

        Logger.LogDebug("Initializing Shaka Player for manifest: {ManifestUrl}", ViewModel.Streams.DashManifestUrl);
        
        _shakaPlayerLoading = true;
        _shakaPlayerError = null;
        // Don't call StateHasChanged here to avoid triggering another render cycle

        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);

            // Small delay to ensure the video element is rendered
            await Task.Delay(100);

            var success = await JsRuntime.InvokeAsync<bool>(
                "shakaPlayerInterop.initialize",
                ShakaVideoElementId,
                ViewModel.Streams.DashManifestUrl,
                _dotNetRef);

            if (success)
            {
                _shakaPlayerInitialized = true;
                _lastInitializedManifestUrl = ViewModel.Streams.DashManifestUrl;
                Logger.LogDebug("Shaka Player initialized successfully for: {ManifestUrl}", ViewModel.Streams.DashManifestUrl);
            }
            else
            {
                _shakaPlayerError = "Failed to initialize DASH player. Your browser may not support this feature.";
                Logger.LogWarning("Shaka Player initialization failed for: {ManifestUrl}", ViewModel.Streams.DashManifestUrl);
            }
        }
        catch (JSException ex)
        {
            _shakaPlayerError = $"JavaScript error: {ex.Message}";
            Logger.LogError(ex, "JavaScript error during Shaka Player initialization");
        }
        catch (Exception ex)
        {
            _shakaPlayerError = $"Unexpected error: {ex.Message}";
            Logger.LogError(ex, "Unexpected error during Shaka Player initialization");
        }
        finally
        {
            _shakaPlayerLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task DestroyShakaPlayerAsync()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("shakaPlayerInterop.destroy", ShakaVideoElementId);
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    private async Task RetryDashPlayerAsync()
    {
        Logger.LogDebug("Retrying Shaka Player initialization");
        _shakaPlayerError = null;
        _shakaPlayerInitialized = false;
        _lastInitializedManifestUrl = null;
        await DestroyShakaPlayerAsync();
        await InitializeShakaPlayerAsync();
    }

    /// <summary>
    /// Called from JavaScript when the player encounters an error.
    /// </summary>
    [JSInvokable]
    public void OnPlayerError(string errorMessage)
    {
        _shakaPlayerError = errorMessage;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called from JavaScript when the video quality changes (ABR adaptation).
    /// </summary>
    [JSInvokable]
    public void OnDashQualityAdapted(string quality)
    {
        _currentDashQuality = quality;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called from JavaScript when the video is loaded.
    /// </summary>
    [JSInvokable]
    public void OnVideoLoaded()
    {
        _shakaPlayerLoading = false;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called from JavaScript when playback starts.
    /// </summary>
    [JSInvokable]
    public void OnPlaybackStarted()
    {
        // Can be used for analytics or UI updates
    }

    private string? GetPosterUrl()
    {
        var thumbnailIdentity = ViewModel?.GetBestThumbnailIdentity();
        
        if (thumbnailIdentity is null)
        {
            return null;
        }

        // Now proxy it through our local image proxy endpoint for caching
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
            _ => count!.Value.ToString("N0")
        };
    }

    private static string FormatNumber(int count)
    {
        return count switch
        {
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M",
            >= 1_000 => $"{count / 1_000.0:F1}K",
            _ => count.ToString("N0")
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (ViewModel != null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
        }

        await DestroyShakaPlayerAsync();
        _dotNetRef?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
