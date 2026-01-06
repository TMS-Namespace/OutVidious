using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component for displaying channel videos.
/// Loads videos on demand when the panel becomes active.
/// </summary>
public partial class ChannelVideosComponent : ComponentBase, IDisposable
{
    private bool _isDisposed;
    private string? _loadedChannelId;
    private CancellationTokenSource? _cts;
    private bool _wasActive;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private ILogger<ChannelVideosComponent> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the dock panel is currently active/visible.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// The channel ID to display videos for.
    /// </summary>
    [Parameter]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Event callback when channel ID changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ChannelIdChanged { get; set; }

    protected Channel? ViewModel { get; private set; }

    protected bool IsInitialLoading { get; private set; }

    protected string? ErrorMessage { get; private set; }

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (IsActive && !_wasActive)
        {
            _wasActive = true;
            if (!string.IsNullOrWhiteSpace(ChannelId) && _loadedChannelId != ChannelId)
            {
                await LoadChannelVideosAsync(ChannelId);
            }
        }
        else if (!IsActive && _wasActive)
        {
            _wasActive = false;
        }
        else if (IsActive && !string.IsNullOrWhiteSpace(ChannelId) && _loadedChannelId != ChannelId)
        {
            await LoadChannelVideosAsync(ChannelId);
        }
    }

    /// <summary>
    /// Loads channel videos by channel ID.
    /// </summary>
    public async Task LoadChannelByIdAsync(string channelId)
    {
        if (ChannelId != channelId)
        {
            ChannelId = channelId;
            await ChannelIdChanged.InvokeAsync(channelId);
        }

        if (IsActive)
        {
            await LoadChannelVideosAsync(channelId);
        }
    }

    private async Task LoadChannelVideosAsync(string channelId)
    {
        if (_cts is null)
        {
            return;
        }

        IsInitialLoading = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            Logger.LogDebug("[{MethodName}] Loading videos for channel '{ChannelId}'.", nameof(LoadChannelVideosAsync), channelId);

            if (ViewModel is not null)
            {
                ViewModel.StateChanged -= OnViewModelStateChanged;
                ViewModel.Dispose();
            }

            ViewModel = await Orchestrator.Super.GetChannelByIdAsync(channelId, _cts.Token);

            if (ViewModel is null)
            {
                ErrorMessage = $"Channel '{channelId}' not found.";
                Logger.LogWarning("[{MethodName}] Channel not found: '{ChannelId}'.", nameof(LoadChannelVideosAsync), channelId);
                IsInitialLoading = false;
                StateHasChanged();
                return;
            }

            ViewModel.StateChanged += OnViewModelStateChanged;

            Logger.LogDebug("[{MethodName}] Channel loaded: '{ChannelName}'. Loading videos...", nameof(LoadChannelVideosAsync), ViewModel.Name);

            _loadedChannelId = channelId;
            IsInitialLoading = false;
            StateHasChanged();

            await ViewModel.LoadInitialVideosAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("[{MethodName}] Channel videos loading cancelled for: '{ChannelId}'.", nameof(LoadChannelVideosAsync), channelId);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load channel videos. Please try again.";
            Logger.LogError(ex, "[{MethodName}] Unexpected error loading channel videos: '{ChannelId}'.", nameof(LoadChannelVideosAsync), channelId);
            IsInitialLoading = false;
            StateHasChanged();
        }
    }

    protected async Task HandleVideoClick(Video video)
    {
        var watchUrl = video.RemoteIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        NavigationManager.NavigateTo(watchUrl);
        await Task.CompletedTask;
    }

    protected async Task HandleLoadMore()
    {
        if (_cts is not null && ViewModel is not null)
        {
            await ViewModel.LoadMoreVideosAsync(_cts.Token);
        }
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (ViewModel is not null)
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
            ViewModel.Dispose();
        }

        _cts?.Cancel();
        _cts?.Dispose();

        _isDisposed = true;
    }
}
