using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;
using TMS.Libs.Frontend.Web.DockPanels.Components;
using TMS.Libs.Frontend.Web.DockPanels.Enums;
using TMS.Libs.Frontend.Web.DockPanels.Models;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Layout;

/// <summary>
/// Main layout component for the FrontTube application.
/// Uses DocksHostComponent for dockable panels,
/// with MudBlazor components inside the panels.
/// </summary>
public partial class MainLayout : IAsyncDisposable
{
    /// <summary>
    /// The display title of the channel about panel.
    /// </summary>
    private const string ChannelAboutPanelTitle = "About";

    /// <summary>
    /// The icon class for the channel about panel.
    /// </summary>
    private const string ChannelAboutPanelIcon = "fa-solid fa-info-circle";

    /// <summary>
    /// The display title of the channel videos panel.
    /// </summary>
    private const string ChannelVideosPanelTitle = "Videos";

    /// <summary>
    /// The icon class for the channel videos panel.
    /// </summary>
    private const string ChannelVideosPanelIcon = "fa-solid fa-video";

    /// <summary>
    /// The static drawer title for the channel group.
    /// </summary>
    private const string ChannelGroupTitle = "Channel Info";

    /// <summary>
    /// The icon class for the channel group.
    /// </summary>
    private const string ChannelGroupIcon = "fa-solid fa-users";

    /// <summary>
    /// The default width in pixels for the channel drawer when it expands.
    /// </summary>
    private const int ChannelDrawerWidthPx = 800;

    /// <summary>
    /// The default width in pixels for sidebar drawers.
    /// </summary>
    private const int SidebarDrawerWidthPx = 500;

    /// <summary>
    /// The static drawer title for the collections group.
    /// </summary>
    private const string CollectionsGroupTitle = "Collections";

    /// <summary>
    /// The icon class for the collections group.
    /// </summary>
    private const string CollectionsGroupIcon = "fa-solid fa-folder-open";

    /// <summary>
    /// The static drawer title for the discovery group.
    /// </summary>
    private const string DiscoverGroupTitle = "Discover";

    /// <summary>
    /// The icon class for the discovery group.
    /// </summary>
    private const string DiscoverGroupIcon = "fa-solid fa-compass";

    /// <summary>
    /// The static drawer title for the video info group.
    /// </summary>
    private const string VideoInfoGroupTitle = "Video Info";

    /// <summary>
    /// The icon class for the video info group.
    /// </summary>
    private const string VideoInfoGroupIcon = "fa-solid fa-info-circle";

    /// <summary>
    /// The icon class for the main video player panel.
    /// </summary>
    private const string VideoPlayerPanelIcon = "fa-solid fa-play";

    /// <summary>
    /// The icon class for the search panel.
    /// </summary>
    private const string SearchPanelIcon = "fa-solid fa-magnifying-glass";

    /// <summary>
    /// The icon class for the search group.
    /// </summary>
    private const string SearchGroupIcon = "fa-solid fa-magnifying-glass";

    /// <summary>
    /// The icon class for the queue panel.
    /// </summary>
    private const string QueuePanelIcon = "fa-solid fa-list-ol";

    /// <summary>
    /// The icon class for the local playlists panel.
    /// </summary>
    private const string LocalPlaylistsPanelIcon = "fa-solid fa-folder";

    /// <summary>
    /// The icon class for the favorites panel.
    /// </summary>
    private const string FavoritesPanelIcon = "fa-solid fa-heart";

    /// <summary>
    /// The icon class for the history panel.
    /// </summary>
    private const string HistoryPanelIcon = "fa-solid fa-clock-rotate-left";

    /// <summary>
    /// The icon class for the trending panel.
    /// </summary>
    private const string TrendingPanelIcon = "fa-solid fa-fire";

    /// <summary>
    /// The icon class for the popular panel.
    /// </summary>
    private const string PopularPanelIcon = "fa-solid fa-chart-line";

    /// <summary>
    /// The icon class for the video description panel.
    /// </summary>
    private const string VideoDescriptionPanelIcon = "fa-solid fa-file-lines";

    /// <summary>
    /// The icon class for the video comments panel.
    /// </summary>
    private const string VideoCommentsPanelIcon = "fa-solid fa-comments";

    /// <summary>
    /// The icon class for the related videos panel.
    /// </summary>
    private const string RelatedVideosPanelIcon = "fa-solid fa-video";

    private static readonly DockPanelDrawerOptions ChannelDrawerOptions = new()
    {
        Width = ChannelDrawerWidthPx,
        Visible = false
    };

    private static readonly DockPanelDrawerOptions SidebarDrawerOptions = new()
    {
        Width = SidebarDrawerWidthPx,
        Visible = false
    };

    private static readonly TimeSpan ChannelDockOperationTimeout = TimeSpan.FromSeconds(2);

    private bool _isDarkMode = true;
    private bool _isDisposed;
    private bool _isDockPanelsReady;
    private DocksHostComponent? _dockPanelsComponent;
    private ILogger<MainLayout>? _logger;
    private bool _deferChannelAboutActivation;
    private TaskCompletionSource<bool>? _channelPanelsReadyTcs;

    private DockPanelComponent? _videoPlayerPanel;
    private DockPanelComponent? _channelAboutPanel;
    private DockPanelComponent? _channelVideosPanel;
    private DockPanelComponent? _searchPanel;
    private DockPanelComponent? _queuePanel;
    private DockPanelComponent? _localPlaylistsPanel;
    private DockPanelComponent? _favoritesPanel;
    private DockPanelComponent? _historyPanel;
    private DockPanelComponent? _trendingPanel;
    private DockPanelComponent? _popularPanel;
    private DockPanelComponent? _videoDescriptionPanel;
    private DockPanelComponent? _videoCommentsPanel;
    private DockPanelComponent? _relatedVideosPanel;

    /// <summary>
    /// Dictionary to track the visibility state of all panels by ID.
    /// This prevents Blazor from recreating closed panels when re-rendering.
    /// </summary>
    private readonly Dictionary<Guid, bool> _panelVisibility = [];

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private BrowserConsoleCapture BrowserConsoleCapture { get; set; } = null!;

    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = null!;

    /// <summary>
    /// Gets the BootstrapBlazor theme name based on the current dark mode state.
    /// </summary>
    private string BootstrapTheme => _isDarkMode ? "dark" : "light";

    /// <summary>
    /// Gets the dock panel theme based on the current dark mode state.
    /// </summary>
    private DockPanelTheme CurrentDockPanelTheme => _isDarkMode ? DockPanelTheme.VS : DockPanelTheme.Light;

    /// <summary>
    /// Gets or sets the channel ID for the channel dock panels.
    /// </summary>
    private string? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets whether the channel About panel is currently active.
    /// Used for lazy loading - content only renders when panel is visible.
    /// </summary>
    private bool IsChannelAboutPanelActive { get; set; }

    /// <summary>
    /// Gets or sets whether the channel Videos panel is currently active.
    /// Used for lazy loading - content only renders when panel is visible.
    /// </summary>
    private bool IsChannelVideosPanelActive { get; set; }

    /// <summary>
    /// Gets or sets whether the channel group should be rendered in the DOM.
    /// Once a channel is opened, this remains true to keep the group in the DOM
    /// (but hidden via DockView's visibility system) to prevent recreation issues.
    /// </summary>
    private bool ShouldRenderChannelGroup { get; set; }

    /// <summary>
    /// Helper to get panel visibility, defaulting to true for panels without state.
    /// </summary>
    private bool IsPanelVisible(DockPanelComponent? panel, bool defaultState = true)
    {
        if (panel is null)
        {
            return defaultState;
        }

        return _panelVisibility.TryGetValue(panel.PanelId, out var visible)
            ? visible
            : defaultState;
    }

    private void SetPanelVisibility(DockPanelComponent? panel, bool isVisible)
    {
        if (panel is null)
        {
            return;
        }

        _panelVisibility[panel.PanelId] = isVisible;
    }

    /// <summary>
    /// Gets the CSS class for the dock panels component.
    /// </summary>
    private string GetDockPanelsClass() => "fronttube-dockview";

    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1976d2",
            Secondary = "#dc004e",
            AppbarBackground = "#1976d2"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#90caf9",
            Secondary = "#f48fb1",
            AppbarBackground = "#1e1e1e",
            Surface = "#121212",
            Background = "#0a0a0a"
        }
    };

    protected override void OnInitialized()
    {
        _logger = LoggerFactory.CreateLogger<MainLayout>();

        Orchestrator.ChannelOpenRequested += OnChannelOpenRequested;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize browser console capture to forward JS console to Serilog
            await BrowserConsoleCapture.InitializeAsync(CancellationToken.None);
        }

        TrySetChannelPanelsReady();
    }

    private void TrySetChannelPanelsReady()
    {
        if (_channelPanelsReadyTcs is null)
        {
            return;
        }

        if (_channelAboutPanel is null || _channelVideosPanel is null)
        {
            return;
        }

        _channelPanelsReadyTcs.TrySetResult(true);
        _channelPanelsReadyTcs = null;
    }

    private async Task<bool> WaitForChannelPanelsReadyAsync(CancellationToken cancellationToken)
    {
        if (_channelAboutPanel is not null && _channelVideosPanel is not null)
        {
            return true;
        }

        _channelPanelsReadyTcs ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        try
        {
            await _channelPanelsReadyTcs.Task.WaitAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            _channelPanelsReadyTcs = null;
            return false;
        }
    }

    private void ToggleDarkMode()
    {
        _isDarkMode = !_isDarkMode;
    }

    /// <summary>
    /// Handles visibility state changes for dock panels.
    /// Called when any dock panel changes visibility.
    /// We update our internal state to track visibility, but we must NOT call StateHasChanged()
    /// when panels close via UI, as that triggers re-renders that recreate panels.
    /// </summary>
    /// <param name="args">Tuple containing (panel, isVisible).</param>
    private Task OnVisibleStateChangedAsync((DockPanelComponent Panel, bool IsVisible) args)
    {
        _logger?.LogDebug(
            "[{MethodName}] Visibility change: PanelId='{PanelId}', Title='{Title}', IsVisible={IsVisible}.",
            nameof(OnVisibleStateChangedAsync),
            args.Panel.PanelId,
            args.Panel.Title,
            args.IsVisible);

        // Update dictionary for all panels (Search, Queue, History, etc.)
        SetPanelVisibility(args.Panel, args.IsVisible);

        // Track visibility state for both open and close events, but only trigger re-render on open
        var shouldUpdate = false;

        if (_channelAboutPanel?.PanelId == args.Panel.PanelId
            || _channelVideosPanel?.PanelId == args.Panel.PanelId)
        {
            // Once opened, always render the group (even when closed) to prevent recreation
            if (args.IsVisible)
            {
                ShouldRenderChannelGroup = true;
            }
        }

        // Sync Blazor state on Open events
        if (args.IsVisible)
        {
            shouldUpdate = true;
        }

        // CRITICAL: Only call StateHasChanged when opening panels.
        // When closing, we update our state via dictionary but do NOT trigger a re-render,
        // allowing DockView to handle the panel removal without Blazor interfering.
        // However, the updated state is ready for the NEXT re-render.
        return shouldUpdate ? InvokeAsync(StateHasChanged) : Task.CompletedTask;
    }

    /// <summary>
    /// Handles active state changes for dock panels.
    /// This drives lazy loading of panel content.
    /// </summary>
    /// <param name="args">Tuple containing (panel, isActive).</param>
    private Task OnActiveStateChangedAsync((DockPanelComponent Panel, bool IsActive) args)
    {
        _logger?.LogDebug(
            "[{MethodName}] Active change: PanelId='{PanelId}', Title='{Title}', IsActive={IsActive}.",
            nameof(OnActiveStateChangedAsync),
            args.Panel.PanelId,
            args.Panel.Title,
            args.IsActive);

        var shouldUpdate = false;

        if (_channelAboutPanel?.PanelId == args.Panel.PanelId)
        {
            if (_deferChannelAboutActivation && args.IsActive)
            {
                return Task.CompletedTask;
            }

            IsChannelAboutPanelActive = args.IsActive;
            shouldUpdate = true;
        }
        else if (_channelVideosPanel?.PanelId == args.Panel.PanelId)
        {
            IsChannelVideosPanelActive = args.IsActive;
            shouldUpdate = true;
        }

        return shouldUpdate ? InvokeAsync(StateHasChanged) : Task.CompletedTask;
    }

    /// <summary>
    /// Handles channel open requests from the Orchestrator.
    /// Shows the About panel in the channel drawer and activates it.
    /// If this is the first time opening a channel, also unpins the group to make it a drawer.
    /// </summary>
    private async void OnChannelOpenRequested(object? sender, string channelId)
    {
        if (_dockPanelsComponent is null)
        {
            return;
        }

        ChannelId = channelId;
        _deferChannelAboutActivation = true;
        IsChannelAboutPanelActive = false;
        IsChannelVideosPanelActive = false;
        ShouldRenderChannelGroup = true;
        await InvokeAsync(StateHasChanged);

        using var cancellationTokenSource = new CancellationTokenSource(ChannelDockOperationTimeout);

        try
        {
            _logger?.LogDebug(
                "[{MethodName}] Configuring channel panel for channel '{ChannelId}'.",
                nameof(OnChannelOpenRequested),
                channelId);

            if (!await WaitForChannelPanelsReadyAsync(cancellationTokenSource.Token))
            {
                _logger?.LogError(
                     "[{MethodName}] Timeout waiting for channel panels to render for channel '{ChannelId}'.",
                     nameof(OnChannelOpenRequested),
                     channelId);
                return;
            }

            SetPanelVisibility(_channelAboutPanel, true);
            SetPanelVisibility(_channelVideosPanel, true);
            await InvokeAsync(StateHasChanged);

            if (_channelAboutPanel is null)
            {
                _logger?.LogError(
                    "[{MethodName}] Channel about panel reference missing for channel '{ChannelId}'.",
                    nameof(OnChannelOpenRequested),
                    channelId);
                return;
            }

            var opened = await _dockPanelsComponent.OpenDrawerPanelAsync(
                _channelAboutPanel,
                ChannelDrawerWidthPx,
                ChannelGroupTitle,
                cancellationTokenSource.Token);

            if (!opened)
            {
                _logger?.LogError(
                    "[{MethodName}] Failed to open channel drawer for channel '{ChannelId}'.",
                    nameof(OnChannelOpenRequested),
                    channelId);
                return;
            }

            IsChannelAboutPanelActive = true;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning(
                "[{MethodName}] Timeout while opening channel panel for channel '{ChannelId}'.",
                nameof(OnChannelOpenRequested),
                channelId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to show channel about panel for channel '{ChannelId}'.",
                nameof(OnChannelOpenRequested),
                channelId);
            // Intentional: avoid breaking the UI flow while surfacing the error via logs.
        }
        finally
        {
            _deferChannelAboutActivation = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        Orchestrator.ChannelOpenRequested -= OnChannelOpenRequested;

        _channelPanelsReadyTcs?.TrySetCanceled();
        _channelPanelsReadyTcs = null;

        await BrowserConsoleCapture.DisposeAsync();

        _isDisposed = true;
    }
}
