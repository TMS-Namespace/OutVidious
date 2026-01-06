using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TMS.Apps.FrontTube.Frontend.WebUI.Services;
using TMS.Libs.Frontend.Web.DockViewWrapper.Components;
using TMS.Libs.Frontend.Web.DockViewWrapper.Enums;

namespace TMS.Apps.FrontTube.Frontend.WebUI.Layout;

/// <summary>
/// Main layout component for the FrontTube application.
/// Uses BootstrapBlazor DockViewV2Wrapper for dockable panels,
/// with MudBlazor components inside the panels.
/// </summary>
public partial class MainLayout : IAsyncDisposable
{
    /// <summary>
    /// The title of the channel panel used in visibility callbacks.
    /// </summary>
    private const string ChannelPanelTitle = "Channel";

    /// <summary>
    /// The default width in pixels for the channel drawer when it expands.
    /// </summary>
    private const int ChannelDrawerWidthPx = 800;

    private bool _isDarkMode = true;
    private bool _isDisposed;
    private bool _isDockViewReady;
    private DockViewV2Wrapper? _dockViewWrapper;

    /// <summary>
    /// Configuration for dock groups specifying which should be unpinned as drawers
    /// and individual panel settings.
    /// </summary>
    private readonly List<DockGroupConfiguration> _groupConfigurations =
    [
        // Group 1: Channel - starts as drawer with hidden tab
        new DockGroupConfiguration
        {
            GroupIndex = 1,
            GroupTitle= "Channel Info",
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig
                {
                    Title = "Channel",
                    DrawerTabVisibility = DrawerTabVisibility.Hidden,
                    DrawerWidthPx = ChannelDrawerWidthPx
                },
                new DockPanelInitConfig
                {
                    Title = "About",
                    DrawerTabVisibility = DrawerTabVisibility.Hidden,
                    DrawerWidthPx = ChannelDrawerWidthPx
                }
            ]
        },
        // Group 2: Search
        new DockGroupConfiguration
        {
            GroupIndex = 2,
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig { Title = "Search" }
            ]
        },
        // Group 3: Queue/Playlists/Favorites/History
        new DockGroupConfiguration
        {
            GroupIndex = 3,
            PinState = DockPanelPinState.Drawer,
            GroupTitle= "Collections",
            Panels =
            [
                new DockPanelInitConfig { Title = "Queue" }
            ]
        },
        // Group 4: Trending/Popular
        new DockGroupConfiguration
        {
            GroupIndex = 4,
            GroupTitle= "Discover",
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig { Title = "Trending" }
            ]
        },
        // Group 5: Description/Comments/Related
        new DockGroupConfiguration
        {
            GroupIndex = 5,
            GroupTitle= "Video Info",
            PinState = DockPanelPinState.Drawer,
            Panels =
            [
                new DockPanelInitConfig { Title = "Description" }
            ]
        }
    ];

    [Inject]
    private Orchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private BrowserConsoleCapture BrowserConsoleCapture { get; set; } = null!;

    /// <summary>
    /// Gets the BootstrapBlazor theme name based on the current dark mode state.
    /// </summary>
    private string BootstrapTheme => _isDarkMode ? "dark" : "light";

    /// <summary>
    /// Gets the DockViewTheme based on the current dark mode state.
    /// </summary>
    private DockViewTheme DockViewTheme => _isDarkMode ? DockViewTheme.VS : DockViewTheme.Light;

    /// <summary>
    /// Gets or sets the channel ID for the channel dock panel.
    /// </summary>
    private string? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets whether the channel dock panel is currently active.
    /// Used for lazy loading - content only renders when panel is visible.
    /// </summary>
    private bool IsChannelPanelActive { get; set; }

    /// <summary>
    /// Gets or sets whether the channel dock panel is visible.
    /// Controls the Visible attribute on the DockViewComponent.
    /// Starts as false (hidden) and is set to true when a channel is requested.
    /// </summary>
    private bool IsChannelPanelVisible { get; set; }

    /// <summary>
    /// Gets the CSS class for the DockViewV2 component.
    /// </summary>
    private string GetDockViewClass() => "fronttube-dockview";

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
    /// </summary>
    /// <param name="args">Tuple containing (title, isVisible).</param>
    private Task OnVisibleStateChangedAsync((string Title, bool IsVisible) args)
    {
        if (args.Title == ChannelPanelTitle)
        {
            IsChannelPanelActive = args.IsVisible;
            IsChannelPanelVisible = args.IsVisible;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles channel open requests from the Orchestrator.
    /// Shows the channel drawer tab (if hidden), and activates it to expand the drawer.
    /// Uses separate calls with delays between them for proper timing.
    /// </summary>
    private async void OnChannelOpenRequested(object? sender, string channelId)
    {
        if (_dockViewWrapper is null)
        {
            return;
        }

        ChannelId = channelId;
        IsChannelPanelActive = true;
        IsChannelPanelVisible = true;
        await InvokeAsync(StateHasChanged);

        // Small delay to ensure the panel content is rendered by Blazor
        await Task.Delay(100);

        try
        {
            // Step 1: Show the drawer tab (make sidebar button visible) and set width
            await _dockViewWrapper.ShowDrawerTabAsync(ChannelPanelTitle, ChannelDrawerWidthPx, CancellationToken.None);

            // Step 2: C# delay - this gives the browser event loop time to fully process the DOM changes
            // This is crucial - the delay must happen in C# (not JS) to allow proper interleaving
            await Task.Delay(100);

            // Step 3: Activate the panel to expand the drawer
            await _dockViewWrapper.ActivatePanelAsync(ChannelPanelTitle, CancellationToken.None);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[MainLayout] Error showing channel panel: {ex.Message}");
        }
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

