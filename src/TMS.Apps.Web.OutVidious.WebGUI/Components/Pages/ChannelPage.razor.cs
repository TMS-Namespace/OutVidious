using Microsoft.AspNetCore.Components;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Contracts;
using TMS.Apps.Web.OutVidious.Common.ProvidersCore.Interfaces;
using TMS.Apps.Web.OutVidious.Core.ViewModels;

namespace TMS.Apps.Web.OutVidious.WebGUI.Components.Pages;

/// <summary>
/// Page for displaying a YouTube channel's content.
/// </summary>
public partial class ChannelPageBase : ComponentBase, IDisposable
{
    private int _selectedTabIndex;
    private bool _isDisposed;
    private CancellationTokenSource? _cts;

    [Inject]
    private IVideoProvider VideoProvider { get; set; } = null!;

    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// The channel ID from the route.
    /// </summary>
    [Parameter]
    public string? ChannelId { get; set; }

    protected ChannelViewModel ViewModel { get; private set; } = null!;

    protected string PageTitle => ViewModel?.Channel?.Name ?? "Channel";

    protected int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                _ = OnTabChangedAsync(value);
            }
        }
    }

    protected string? AvatarUrl => GetBestAvatar();

    protected string? BannerUrl => GetBestBanner();

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
        
        var vmLogger = LoggerFactory.CreateLogger<ChannelViewModel>();
        ViewModel = new ChannelViewModel(VideoProvider, vmLogger);
        ViewModel.StateChanged += OnViewModelStateChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(ChannelId) && _cts is not null)
        {
            await ViewModel.LoadChannelAsync(ChannelId, _cts.Token);
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
        if (_cts is not null)
        {
            await ViewModel.LoadMoreVideosAsync(_cts.Token);
        }
    }

    private async Task OnTabChangedAsync(int tabIndex)
    {
        if (ViewModel.Channel is null || 
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
        var avatars = ViewModel?.Channel?.Avatars;
        
        if (avatars is null || avatars.Count == 0)
        {
            return null;
        }

        // Prefer larger avatars for the channel header
        var preferred = avatars
            .Where(a => a.Width >= 88)
            .OrderByDescending(a => a.Width)
            .FirstOrDefault();

        return preferred?.Url.ToString() ?? avatars.First().Url.ToString();
    }

    private string? GetBestBanner()
    {
        var banners = ViewModel?.Channel?.Banners;
        
        if (banners is null || banners.Count == 0)
        {
            return null;
        }

        // Prefer a banner around 1280-1920 width
        var preferred = banners
            .Where(b => b.Width >= 1280 && b.Width <= 2560)
            .OrderByDescending(b => b.Width)
            .FirstOrDefault();

        return preferred?.Url.ToString() ?? banners.First().Url.ToString();
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
