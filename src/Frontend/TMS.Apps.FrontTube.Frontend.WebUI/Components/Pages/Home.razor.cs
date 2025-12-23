using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components.Pages;

/// <summary>
/// Code-behind for the Home page.
/// </summary>
public partial class Home : ComponentBase, IDisposable
{
    private const string DefaultVideoId = "JGJON9_uMHI";

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private ILogger<Home> Logger { get; set; } = null!;

    private VideoPlayerViewModel? _videoPlayerVm;
    private string _videoIdInput = DefaultVideoId;
    private bool _isLoading;
    private bool _disposed;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Gets the provider base URL.
    /// </summary>
    protected string ProviderBaseUrl => Orchestrator.ProviderBaseUrl;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _cts = new CancellationTokenSource();

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
            Logger.LogInformation("Calling Super.GetVideoByRemoteIdAsync...");
            
            // Dispose previous ViewModel if any
            _videoPlayerVm?.Dispose();
            
            _videoPlayerVm = await Orchestrator.Super.GetVideoByRemoteIdAsync(videoId, _cts.Token);
            
            if (_videoPlayerVm is not null)
            {
                Logger.LogInformation("VideoPlayerViewModel created. Title: {Title}", _videoPlayerVm.VideoInfo.Title);
                Logger.LogInformation("Current Stream URL: {Url}", _videoPlayerVm.CurrentStreamUrl ?? "(null)");
            }
            else
            {
                Logger.LogWarning("Video not found: {VideoId}", videoId);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Video load cancelled for: {VideoId}", videoId);
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
