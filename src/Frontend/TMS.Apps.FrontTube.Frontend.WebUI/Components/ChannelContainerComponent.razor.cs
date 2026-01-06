using System.Net;
using Microsoft.AspNetCore.Components;
using TMS.Apps.FrontTube.Backend.Core.ViewModels;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Components;

/// <summary>
/// Component for displaying a YouTube channel's content in a dock panel.
/// Implements lazy loading - only loads when the panel is active/visible.
/// </summary>
public partial class ChannelContainerComponent : ComponentBase, IDisposable
{
    private const string AboutTabId = "about";
    private const string AboutTabLabel = "About";
    private int _selectedTabIndex;
    private bool _isDisposed;
    private bool _initialLoadComplete;
    private string? _loadedChannelId;
    private CancellationTokenSource? _cts;
    private bool _wasActive;

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private ILogger<ChannelContainerComponent> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the dock panel is currently active/visible.
    /// When true, the component will load and render content.
    /// When false, the component renders nothing to save resources.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// The channel ID to display.
    /// </summary>
    [Parameter]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Event callback when channel ID changes (e.g., user navigates to different channel).
    /// </summary>
    [Parameter]
    public EventCallback<string?> ChannelIdChanged { get; set; }

    protected Channel? ViewModel { get; private set; }

    protected bool IsInitialLoading { get; private set; }

    protected string? ErrorMessage { get; private set; }

    protected int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;

                if (_initialLoadComplete)
                {
                    _ = OnTabChangedAsync(value);
                }
            }
        }
    }

    protected string? AvatarUrl => GetBestAvatar();

    protected string? BannerUrl => GetBestBanner();

    protected IReadOnlyList<string> Tabs
    {
        get
        {
            if (ViewModel is null)
            {
                return [];
            }

            var tabs = ViewModel.AvailableTabs.ToList();

            if (!tabs.Any(t => string.Equals(t, AboutTabId, StringComparison.OrdinalIgnoreCase)))
            {
                tabs.Add(AboutTabId);
            }

            return tabs;
        }
    }

    protected bool IsAboutTabSelected
    {
        get
        {
            var tabId = GetSelectedTabId();
            return tabId != null && string.Equals(tabId, AboutTabId, StringComparison.OrdinalIgnoreCase);
        }
    }

    protected bool HasChannelDescription => !string.IsNullOrWhiteSpace(ViewModel?.DescriptionHtml)
        || !string.IsNullOrWhiteSpace(ViewModel?.Description);

    protected MarkupString ChannelDescriptionMarkup
    {
        get
        {
            if (ViewModel is null)
            {
                return new MarkupString(string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(ViewModel.DescriptionHtml))
            {
                return new MarkupString(ViewModel.DescriptionHtml);
            }

            if (string.IsNullOrWhiteSpace(ViewModel.Description))
            {
                return new MarkupString(string.Empty);
            }

            var encoded = WebUtility.HtmlEncode(ViewModel.Description);
            var withLineBreaks = encoded
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");

            return new MarkupString(withLineBreaks);
        }
    }

    protected override void OnInitialized()
    {
        _cts = new CancellationTokenSource();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Handle activation state change
        if (IsActive && !_wasActive)
        {
            // Panel just became active - load content if we have a channel ID
            _wasActive = true;
            if (!string.IsNullOrWhiteSpace(ChannelId) && _loadedChannelId != ChannelId)
            {
                await LoadChannelAsync(ChannelId);
            }
        }
        else if (!IsActive && _wasActive)
        {
            // Panel just became inactive - we could optionally clean up here
            _wasActive = false;
        }
        else if (IsActive && !string.IsNullOrWhiteSpace(ChannelId) && _loadedChannelId != ChannelId)
        {
            // Channel ID changed while panel is active
            await LoadChannelAsync(ChannelId);
        }
    }

    /// <summary>
    /// Loads a channel by ID. Can be called externally to trigger channel loading.
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
            await LoadChannelAsync(channelId);
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
            Logger.LogDebug("[{MethodName}] Loading channel '{ChannelId}'.", nameof(LoadChannelAsync), channelId);

            // Dispose previous ViewModel if any
            if (ViewModel is not null)
            {
                ViewModel.StateChanged -= OnViewModelStateChanged;
                ViewModel.Dispose();
            }

            ViewModel = await Orchestrator.Super.GetChannelByIdAsync(channelId, _cts.Token);

            if (ViewModel is null)
            {
                ErrorMessage = $"Channel '{channelId}' not found.";
                Logger.LogWarning("[{MethodName}] Channel not found: '{ChannelId}'.", nameof(LoadChannelAsync), channelId);
                IsInitialLoading = false;
                StateHasChanged();
                return;
            }

            ViewModel.StateChanged += OnViewModelStateChanged;

            Logger.LogDebug("[{MethodName}] Channel loaded: '{ChannelName}'.", nameof(LoadChannelAsync), ViewModel.Name);

            _initialLoadComplete = true;
            _loadedChannelId = channelId;
            IsInitialLoading = false;
            StateHasChanged();

            // Load initial videos in background
            _ = ViewModel.LoadInitialVideosAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("[{MethodName}] Channel loading cancelled for: '{ChannelId}'.", nameof(LoadChannelAsync), channelId);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to load channel. Please try again.";
            Logger.LogError(ex, "[{MethodName}] Unexpected error loading channel: '{ChannelId}'.", nameof(LoadChannelAsync), channelId);
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

    private async Task OnTabChangedAsync(int tabIndex)
    {
        var tabs = Tabs;

        if (ViewModel is null ||
            tabIndex < 0 ||
            tabIndex >= tabs.Count ||
            _cts is null)
        {
            return;
        }

        var tabId = tabs[tabIndex];

        if (string.Equals(tabId, AboutTabId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await ViewModel.SelectTabAsync(tabId, _cts.Token);
    }

    private void OnViewModelStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private string? GetBestAvatar()
    {
        var avatarIdentity = ViewModel?.GetBestAvatarIdentity();
        if (avatarIdentity is null)
        {
            return null;
        }

        var proxyUrl = avatarIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        return string.IsNullOrWhiteSpace(proxyUrl) ? null : proxyUrl;
    }

    private string? GetBestBanner()
    {
        var bannerIdentity = ViewModel?.GetBestBannerIdentity();
        if (bannerIdentity is null)
        {
            return null;
        }

        var proxyUrl = bannerIdentity.GetProxyUrl(Orchestrator.Super.Proxy);
        return string.IsNullOrWhiteSpace(proxyUrl) ? null : proxyUrl;
    }

    private string? GetSelectedTabId()
    {
        var tabs = Tabs;

        if (_selectedTabIndex < 0 || _selectedTabIndex >= tabs.Count)
        {
            return null;
        }

        return tabs[_selectedTabIndex];
    }

    protected static string GetTabLabel(string tabId)
    {
        if (string.Equals(tabId, AboutTabId, StringComparison.OrdinalIgnoreCase))
        {
            return AboutTabLabel;
        }

        return tabId;
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
