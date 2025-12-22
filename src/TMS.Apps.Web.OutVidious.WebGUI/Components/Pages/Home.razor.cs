using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;
using TMS.Apps.Web.OutVidious.Core.ViewModels;

namespace TMS.Apps.Web.OutVidious.WebGUI.Components.Pages;

/// <summary>
/// Code-behind for the Home page.
/// </summary>
public partial class Home : ComponentBase, IDisposable
{
    private const string DefaultVideoId = "JGJON9_uMHI";

    [Inject]
    private IVideoProvider VideoProvider { get; set; } = null!;

    [Inject]
    private ILogger<Home> Logger { get; set; } = null!;

    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = null!;

    private VideoPlayerViewModel? _videoPlayerVm;
    private string _videoIdInput = DefaultVideoId;
    private bool _isLoading;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Gets the provider base URL.
    /// </summary>
    protected string ProviderBaseUrl => VideoProvider.BaseUrl.ToString().TrimEnd('/');

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _cts = new CancellationTokenSource();
        
        // Create the ViewModel
        var vmLogger = LoggerFactory.CreateLogger<VideoPlayerViewModel>();
        _videoPlayerVm = new VideoPlayerViewModel(VideoProvider, vmLogger);

        // Auto-load the demo video
        Logger.LogInformation("Auto-loading demo video: {VideoId}", DefaultVideoId);
        await LoadVideoAsync(DefaultVideoId);
    }

    private async Task OnLoadVideoClickedAsync()
    {
        Logger.LogInformation("=== LOAD VIDEO BUTTON CLICKED ===");
        Logger.LogInformation("Current video ID input: '{VideoId}'", _videoIdInput);
        
        if (string.IsNullOrWhiteSpace(_videoIdInput))
        {
            Logger.LogWarning("Load video clicked with empty video ID");
            return;
        }

        Logger.LogInformation("Calling LoadVideoAsync with video ID: {VideoId}", _videoIdInput.Trim());
        await LoadVideoAsync(_videoIdInput.Trim());
        Logger.LogInformation("LoadVideoAsync completed");
    }

    private async Task LoadVideoAsync(string videoId)
    {
        Logger.LogInformation("LoadVideoAsync started for video: {VideoId}", videoId);
        
        if (_videoPlayerVm == null)
        {
            Logger.LogWarning("_videoPlayerVm is null, cannot load video");
            return;
        }
        
        if (_cts == null)
        {
            Logger.LogWarning("_cts is null, cannot load video");
            return;
        }

        _isLoading = true;
        Logger.LogDebug("Setting _isLoading = true, calling StateHasChanged");
        StateHasChanged();

        try
        {
            Logger.LogInformation("Calling _videoPlayerVm.LoadVideoAsync...");
            await _videoPlayerVm.LoadVideoAsync(videoId, _cts.Token);
            Logger.LogInformation("_videoPlayerVm.LoadVideoAsync completed. LoadState: {State}", _videoPlayerVm.LoadState);
            Logger.LogInformation("Video Title: {Title}", _videoPlayerVm.VideoInfo?.Title ?? "(null)");
            Logger.LogInformation("Current Stream URL: {Url}", _videoPlayerVm.CurrentStreamUrl ?? "(null)");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load video: {VideoId}", videoId);
        }
        finally
        {
            _isLoading = false;
            Logger.LogDebug("Setting _isLoading = false, calling StateHasChanged");
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _videoPlayerVm?.Dispose();
        _disposed = true;
    }
}
