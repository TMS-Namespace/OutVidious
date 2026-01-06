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
/// Uses DockPanelsComponent for dockable panels,
/// with MudBlazor components inside the panels.
/// </summary>
public partial class MainLayout : IAsyncDisposable
{
    /// <summary>
    /// The unique key of the channel about panel (used for programmatic access).
    /// </summary>
    private const string ChannelAboutPanelKey = "channel-about";

    /// <summary>
    /// The unique key of the channel videos panel (used for programmatic access).
    /// </summary>
    private const string ChannelVideosPanelKey = "channel-videos";

    /// <summary>
    /// The display title of the channel about panel.
    /// </summary>
    private const string ChannelAboutPanelTitle = "About";

    /// <summary>
    /// The display title of the channel videos panel.
    /// </summary>
    private const string ChannelVideosPanelTitle = "Videos";

    /// <summary>
    /// The static drawer title for the channel group.
    /// </summary>
    private const string ChannelGroupTitle = "Channel Info";

    /// <summary>
    /// The default width in pixels for the channel drawer when it expands.
    /// </summary>
    private const int ChannelDrawerWidthPx = 800;

    /// <summary>
    /// The shared group ID for channel drawer panels.
    /// </summary>
    private const string ChannelDrawerGroupId = "channel-info";

    private static readonly DockPanelDrawerOptions ChannelDrawerOptions = new()
    {
        Width = ChannelDrawerWidthPx,
        Visible = false
    };

    private bool _isDarkMode = true;
    private bool _isDisposed;
    private bool _isDockPanelsReady;
    private DockPanelsComponent? _dockPanelsComponent;
    private ILogger<MainLayout>? _logger;
    private int _channelAboutRenderVersion;
    private bool _deferChannelAboutActivation;

    /// <summary>
    /// Configuration for dock groups specifying which should be unpinned as drawers
    /// and individual panel settings.
    /// Note: Channel group (Group 1) is dynamically rendered and not included here.
    /// The indices here are 1-based but shifted because Group 1 is missing at startup.
    /// We start from 1 (Search) to match the DOM order at startup (VideoPlayer is 0).
    /// </summary>
    private readonly List<DockGroupConfiguration> _groupConfigurations =
    [
        // Group 2: Search -> Becomes Index 1 at startup
        new DockGroupConfiguration
        {
            GroupIndex = 1,
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig { Key = "search" }
            ]
        },
        // Group 3: Queue/Playlists/Favorites/History -> Becomes Index 2 at startup
        new DockGroupConfiguration
        {
            GroupIndex = 2,
            PinState = DockPanelPinState.Drawer,
            GroupTitle = "Collections",
            Panels =
            [
                new DockPanelInitConfig { Key = "queue" }
            ]
        },
        // Group 4: Trending/Popular -> Becomes Index 3 at startup
        new DockGroupConfiguration
        {
            GroupIndex = 3,
            GroupTitle = "Discover",
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig { Key = "trending" }
            ]
        },
        // Group 5: Description/Comments/Related -> Becomes Index 4 at startup
        new DockGroupConfiguration
        {
            GroupIndex = 4,
            GroupTitle = "Video Info",
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig { Key = "videoDescription" }
            ]
        }
    ];

    /// <summary>
    /// Dictionary to track the visibility state of all panels by Title.
    /// This prevents Blazor from recreating closed panels when re-rendering.
    /// </summary>
    private readonly Dictionary<string, bool> _panelVisibility = [];

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
    /// Gets or sets whether the channel About panel is visible.
    /// </summary>
    private bool IsChannelAboutPanelVisible
    {
        get => ShouldRenderChannelGroup;
        set => _panelVisibility[ChannelAboutPanelTitle] = value;
    }

    /// <summary>
    /// Gets or sets whether the channel Videos panel is visible.
    /// </summary>
    private bool IsChannelVideosPanelVisible
    {
        get => ShouldRenderChannelGroup;
        set => _panelVisibility[ChannelVideosPanelTitle] = value;
    }

    /// <summary>
    /// Gets or sets whether the channel group should be rendered in the DOM.
    /// Once a channel is opened, this remains true to keep the group in the DOM
    /// (but hidden via DockView's visibility system) to prevent recreation issues.
    /// </summary>
    private bool ShouldRenderChannelGroup { get; set; }

    /// <summary>
    /// Helper to get panel visibility, defaulting to true for non-channel panels.
    /// </summary>
    private bool IsPanelVisible(string title, bool defaultState = true) => 
        _panelVisibility.TryGetValue(title, out var v) ? v : defaultState;

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
    /// <param name="args">Tuple containing (title, isVisible).</param>
    private Task OnVisibleStateChangedAsync((string Title, bool IsVisible) args)
    {
        _logger?.LogDebug(
            "[{MethodName}] Visibility change: Title='{Title}', IsVisible={IsVisible}.",
            nameof(OnVisibleStateChangedAsync),
            args.Title,
            args.IsVisible);

        // Update dictionary for all panels (Search, Queue, History, etc.)
        _panelVisibility[args.Title] = args.IsVisible;

        // Track visibility state for both open and close events, but only trigger re-render on open
        var shouldUpdate = false;

        if (args.Title == ChannelAboutPanelTitle || args.Title == ChannelVideosPanelTitle)
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
    /// <param name="args">Tuple containing (title, key, isActive).</param>
    private Task OnActiveStateChangedAsync((string Title, string? Key, bool IsActive) args)
    {
        _logger?.LogDebug(
            "[{MethodName}] Active change: Title='{Title}', IsActive={IsActive}.",
            nameof(OnActiveStateChangedAsync),
            args.Title,
            args.IsActive);

        var shouldUpdate = false;

        if (args.Title == ChannelAboutPanelTitle)
        {
            if (_deferChannelAboutActivation && args.IsActive)
            {
                return Task.CompletedTask;
            }

            IsChannelAboutPanelActive = args.IsActive;
            shouldUpdate = true;
        }
        else if (args.Title == ChannelVideosPanelTitle)
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
    /// Uses separate calls with delays between them for proper timing.
    /// If the panel was previously closed via X button, it will be recreated.
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
        IsChannelAboutPanelVisible = true;
        IsChannelVideosPanelVisible = true;
        ShouldRenderChannelGroup = true;
        await InvokeAsync(StateHasChanged);

        // Small delay to ensure the panel content is rendered by Blazor
        await Task.Delay(100, CancellationToken.None);

        try
        {
            _logger?.LogDebug(
                "[{MethodName}] Configuring channel panel for channel '{ChannelId}'.",
                nameof(OnChannelOpenRequested),
                channelId);

            // Wait for the panel to appear in DockView before attempting to modify it.
            // This prevents race conditions where Blazor logic runs before the JS update cycle completes.
            if (!await WaitForPanelByTitleAsync(ChannelAboutPanelTitle, CancellationToken.None))
            {
                _logger?.LogError(
                     "[{MethodName}] Timeout waiting for panel '{PanelTitle}' to appear in DockView.",
                     nameof(OnChannelOpenRequested),
                     ChannelAboutPanelTitle);
                // Even if it times out, we might try to proceed, but likely Unpin will fail.
                // We'll proceed to attempt repair.
            }

            var drawerReady = await EnsureDrawerForPanelAsync(ChannelAboutPanelTitle, ChannelAboutPanelKey, CancellationToken.None);

            if (!drawerReady)
            {
                _logger?.LogError(
                    "[{MethodName}] Failed to unpin panel '{PanelTitle}' as drawer.",
                    nameof(OnChannelOpenRequested),
                    ChannelAboutPanelTitle);
            }

            if (drawerReady)
            {
                await _dockPanelsComponent.SetActivePanelByKeyAsync(ChannelAboutPanelKey, CancellationToken.None);
            }

            // Step 1: Show the drawer tab (make sidebar button visible), set width
            var showResult = drawerReady
                && await _dockPanelsComponent.ShowDrawerTabAsync(ChannelAboutPanelTitle, ChannelDrawerWidthPx, CancellationToken.None);

            if (drawerReady)
            {
                await _dockPanelsComponent.SetGroupStaticTitleByKeyAsync(ChannelAboutPanelKey, ChannelGroupTitle, CancellationToken.None);
            }

            // If showResult is false, the panel was orphaned and removed by JS
            // Force a Blazor re-render to recreate it
            if (drawerReady && !showResult)
            {
                _logger?.LogDebug(
                    "[{MethodName}] Panel '{PanelTitle}' was orphaned; triggering re-render.",
                    nameof(OnChannelOpenRequested),
                    ChannelAboutPanelTitle);

                _channelAboutRenderVersion++;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(150, CancellationToken.None);

                // Retry showing the drawer tab
                await WaitForPanelByTitleAsync(ChannelAboutPanelTitle, CancellationToken.None);
                await _dockPanelsComponent.ShowDrawerTabAsync(ChannelAboutPanelTitle, ChannelDrawerWidthPx, CancellationToken.None);
                await _dockPanelsComponent.SetActivePanelByKeyAsync(ChannelAboutPanelKey, CancellationToken.None);
                await _dockPanelsComponent.SetGroupStaticTitleByKeyAsync(ChannelAboutPanelKey, ChannelGroupTitle, CancellationToken.None);
            }

            // Step 2: C# delay - this gives the browser event loop time to fully process the DOM changes
            // This is crucial - the delay must happen in C# (not JS) to allow proper interleaving
            await Task.Delay(100, CancellationToken.None);

            // Step 3: Expand the drawer without toggling via click
            if (drawerReady)
            {
                await _dockPanelsComponent.ShowDrawerAsync(ChannelAboutPanelTitle, CancellationToken.None);
            }

            _deferChannelAboutActivation = false;
            IsChannelAboutPanelActive = true;
            await InvokeAsync(StateHasChanged);
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
    }

    private async Task<bool> WaitForPanelByTitleAsync(string panelTitle, CancellationToken cancellationToken)
    {
        if (_dockPanelsComponent == null) return false;

        // Poll for the panel existence
        for (int i = 0; i < 20; i++) // 20 attempts * 50ms = 1 second max wait
        {
            if (await _dockPanelsComponent.PanelExistsAsync(panelTitle, cancellationToken))
            {
                return true;
            }
            await Task.Delay(50, cancellationToken);
        }

        return false;
    }

    private async Task<bool> EnsureDrawerForPanelAsync(string panelTitle, string panelKey, CancellationToken cancellationToken)
    {
        if (_dockPanelsComponent == null)
        {
            return false;
        }

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var state = await _dockPanelsComponent.GetPanelStateAsync(panelTitle, cancellationToken);
            if (state is { IsDrawer: true })
            {
                if (await _dockPanelsComponent.IsDrawerReadyAsync(panelTitle, cancellationToken))
                {
                    return true;
                }
            }

            if (state == null || state.LocationType == DockPanelLocationType.Grid)
            {
                await _dockPanelsComponent.UnpinGroupByPanelTitleAsync(panelTitle, cancellationToken);
            }
            else if (attempt == 2)
            {
                await _dockPanelsComponent.UnpinGroupByPanelKeyAsync(panelKey, cancellationToken);
            }

            await Task.Delay(100, cancellationToken);
        }

        return false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        Orchestrator.ChannelOpenRequested -= OnChannelOpenRequested;

        await BrowserConsoleCapture.DisposeAsync();

        _isDisposed = true;
    }
}
