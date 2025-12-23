using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Core.ViewModels;
using TMS.Apps.Web.OutVidious.WebGUI.Services;

namespace TMS.Apps.Web.OutVidious.WebGUI.Components.Pages;

/// <summary>
/// Page for displaying a YouTube channel's content.
/// </summary>
public partial class ChannelPageBase : ComponentBase, IDisposable
{
    private int _selectedTabIndex;
    private bool _isDisposed;
    private bool _initialLoadComplete;
    private string? _loadedChannelId;
    private CancellationTokenSource? _cts;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private ILogger<ChannelPageBase> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// The channel ID from the route.
    /// </summary>
    [Parameter]
    public string? ChannelId { get; set; }

    protected ChannelViewModel? ViewModel { get; private set; }

    protected bool IsInitialLoading { get; private set; } = true;

    protected string? ErrorMessage { get; private set; }

    protected string PageTitle => ViewModel?.Channel.Name ?? "Channel";

    protected int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                
                // Only trigger tab change after initial load is complete
                // to avoid clearing videos when MudTabs first binds
                if (_initialLoadComplete)
                {
                    _ = OnTabChangedAsync(value);
                }
            }
        }
    }

    protected string? AvatarUrl => GetBestAvatar();

    protected string? BannerUrl => GetBestBanner();

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Only load if channel ID changed or hasn't been loaded yet
        if (!string.IsNullOrWhiteSpace(ChannelId) && 
            _cts is not null && 
            _loadedChannelId != ChannelId)
        {
            await LoadChannelAsync(ChannelId);
        }
    }

    private async Task LoadChannelAsync(string channelId)
    {
        if (_cts is null)
        {
            return;
        }

        IsInitialLoading = true;
        _initialLoadComplete = false;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            Logger.LogDebug("Loading channel: {ChannelId}", channelId);

            // Dispose previous ViewModel if any
            if (ViewModel is not null)
            {
                ViewModel.StateChanged -= OnViewModelStateChanged;
                ViewModel.Dispose();
            }

            ViewModel = await Orchestrator.Super.GetChannelDetailsByRemoteIdAsync(channelId, _cts.Token);

            if (ViewModel is null)
            {
                ErrorMessage = $"Channel '{channelId}' not found.";
                Logger.LogWarning("Channel not found: {ChannelId}", channelId);
                return;
            }

            ViewModel.StateChanged += OnViewModelStateChanged;

            Logger.LogDebug("Channel loaded: {ChannelName}", ViewModel.Channel.Name);

            // Mark initial load complete and show content - videos will load in background
            _initialLoadComplete = true;
            _loadedChannelId = channelId;
            IsInitialLoading = false;
            StateHasChanged();

            // Load initial videos in background (don't await - let UI render first)
            _ = ViewModel.LoadInitialVideosAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Channel loading cancelled for: {ChannelId}", channelId);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load channel. Please try again.";
            Logger.LogError(ex, "Error loading channel: {ChannelId}", channelId);
            IsInitialLoading = false;
            StateHasChanged();
        }
    }

    protected async Task HandleVideoClick(VideoSummary video)
    {
        var watchUrl = $"/watch/{video.VideoId}";
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

    private async Task OnTabChangedAsync(int tabIndex)
    {
        if (ViewModel?.Channel is null || 
            tabIndex < 0 || 
            tabIndex >= ViewModel.Channel.AvailableTabs.Count ||
            _cts is null)
        {
            return;
        }

        var tab = ViewModel.Channel.AvailableTabs[tabIndex];
        await ViewModel.SelectTabAsync(tab.TabId, _cts.Token);
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private string? GetBestAvatar()
    {
        var avatars = ViewModel?.Channel.Avatars;
        
        if (avatars is null || avatars.Count == 0)
        {
            return null;
        }

        // Prefer larger avatars for the channel header
        var preferred = avatars
            .Where(a => a.Width >= 88)
            .OrderByDescending(a => a.Width)
            .FirstOrDefault();

        var avatar = preferred ?? avatars.First();
        return Orchestrator.Super.BuildImageProxyUrl(avatar.Url);
    }

    private string? GetBestBanner()
    {
        var banners = ViewModel?.Channel.Banners;
        
        if (banners is null || banners.Count == 0)
        {
            return null;
        }

        // Prefer a banner around 1280-1920 width
        var preferred = banners
            .Where(b => b.Width >= 1280 && b.Width <= 2560)
            .OrderByDescending(b => b.Width)
            .FirstOrDefault();

        var banner = preferred ?? banners.First();
        return Orchestrator.Super.BuildImageProxyUrl(banner.Url);
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
