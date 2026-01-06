using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using TMS.Libs.Frontend.Web.DockViewWrapper.Enums;
using TMS.Libs.Frontend.Web.DockViewWrapper.Services;

namespace TMS.Libs.Frontend.Web.DockViewWrapper.Components;

/// <summary>
/// Wrapper around <see cref="DockViewV2"/> that provides extended functionality
/// for declarative configuration of panel pin states, drawer widths, and visibility.
/// </summary>
public partial class DockViewV2Wrapper : IAsyncDisposable
{
    private const int DefaultDelayBetweenOperationsMs = 30;
    private const int InitialDelayMs = 100;

    private DockViewV2? _innerDockView;
    private bool _isInitialized;
    private bool _isDisposed;

    /// <summary>
    /// Gets or sets the DockView interop service.
    /// </summary>
    [Inject]
    private IDockViewInterop DockViewInterop { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the DockView.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the name for the DockView.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the DockView.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether to enable local storage for layout persistence.
    /// </summary>
    [Parameter]
    public bool EnableLocalStorage { get; set; }

    /// <summary>
    /// Gets or sets the DockView theme.
    /// </summary>
    [Parameter]
    public DockViewTheme Theme { get; set; } = DockViewTheme.VS;

    /// <summary>
    /// Gets or sets whether to show the lock button on panels.
    /// </summary>
    [Parameter]
    public bool ShowLock { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the maximize button on panels.
    /// </summary>
    [Parameter]
    public bool ShowMaximize { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the float button on panels.
    /// </summary>
    [Parameter]
    public bool ShowFloat { get; set; } = true;

    /// <summary>
    /// Gets or sets the version for layout versioning.
    /// </summary>
    [Parameter]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the child content (DockViewContent/DockViewComponent).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the configurations for groups that should be unpinned as drawers.
    /// </summary>
    [Parameter]
    public IReadOnlyList<DockGroupConfiguration> GroupConfigurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the default width in pixels for all drawer panels.
    /// Individual panel configurations can override this value.
    /// </summary>
    [Parameter]
    public int DefaultDrawerWidthPx { get; set; } = 500;

    /// <summary>
    /// Gets or sets an additional CSS class to apply when the DockView is hidden during initialization.
    /// </summary>
    [Parameter]
    public string? HiddenClass { get; set; }

    /// <summary>
    /// Gets or sets whether the DockView is ready (visible after initialization).
    /// </summary>
    [Parameter]
    public bool IsReady { get; set; }

    /// <summary>
    /// Gets or sets the callback when the ready state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsReadyChanged { get; set; }

    /// <summary>
    /// Event raised when the DockView initialization is complete and all configurations have been applied.
    /// </summary>
    [Parameter]
    public EventCallback OnInitializationCompleteAsync { get; set; }

    /// <summary>
    /// Event raised when a panel's visibility state changes.
    /// The tuple contains (panelTitle, isVisible).
    /// </summary>
    [Parameter]
    public EventCallback<(string Title, bool IsVisible)> OnVisibleStateChangedAsync { get; set; }

    /// <summary>
    /// Gets the DockView ID for interop operations.
    /// </summary>
    public string DockViewId => Id ?? Name ?? string.Empty;

    /// <summary>
    /// Shows a drawer tab that was previously hidden.
    /// </summary>
    /// <param name="panelTitle">The title of the panel whose drawer tab to show.</param>
    /// <param name="widthPx">Optional width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> ShowDrawerTabAsync(string panelTitle, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return await DockViewInterop.ShowDrawerTabAsync(DockViewId, panelTitle, widthPx.Value, cancellationToken);
        }

        return await DockViewInterop.ShowDrawerTabAsync(DockViewId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer's tab button in the sidebar.
    /// </summary>
    /// <param name="panelTitle">The title of the panel whose tab to hide.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> HideDrawerTabAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return await DockViewInterop.HideDrawerTabAsync(DockViewId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Activates (focuses) a panel and expands its drawer if applicable.
    /// </summary>
    /// <param name="panelTitle">The title of the panel to activate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> ActivatePanelAsync(string panelTitle, CancellationToken cancellationToken)
    {
        return await DockViewInterop.ActivatePanelAsync(DockViewId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Shows a drawer's tab button (if hidden), sets its width, and expands it.
    /// This is a combined operation that ensures the drawer is visible and expanded.
    /// </summary>
    /// <param name="panelTitle">The title of the panel whose drawer to show and expand.</param>
    /// <param name="widthPx">Optional width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> ShowAndExpandDrawerAsync(string panelTitle, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return await DockViewInterop.ShowAndExpandDrawerAsync(DockViewId, panelTitle, widthPx.Value, cancellationToken);
        }

        return await DockViewInterop.ShowAndExpandDrawerAsync(DockViewId, panelTitle, cancellationToken);
    }

    /// <summary>
    /// Sets the width of a drawer panel.
    /// </summary>
    /// <param name="panelTitle">The title of the panel.</param>
    /// <param name="widthPx">The width in pixels.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> SetDrawerWidthAsync(string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        return await DockViewInterop.SetDrawerWidthAsync(DockViewId, panelTitle, widthPx, cancellationToken);
    }

    #region Key-based methods (preferred over title-based)

    /// <summary>
    /// Shows a drawer's tab button by panel key.
    /// </summary>
    /// <param name="panelKey">The unique key of the panel whose tab to show.</param>
    /// <param name="widthPx">Optional width in pixels to set for the drawer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> ShowDrawerTabByKeyAsync(string panelKey, int? widthPx, CancellationToken cancellationToken)
    {
        if (widthPx.HasValue)
        {
            return await DockViewInterop.ShowDrawerTabByKeyAsync(DockViewId, panelKey, widthPx.Value, cancellationToken);
        }

        return await DockViewInterop.ShowDrawerTabByKeyAsync(DockViewId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Hides a drawer's tab button by panel key.
    /// </summary>
    /// <param name="panelKey">The unique key of the panel whose tab to hide.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> HideDrawerTabByKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return await DockViewInterop.HideDrawerTabByKeyAsync(DockViewId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Activates (focuses) a panel by key and expands its drawer if applicable.
    /// </summary>
    /// <param name="panelKey">The unique key of the panel to activate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> ActivatePanelByKeyAsync(string panelKey, CancellationToken cancellationToken)
    {
        return await DockViewInterop.ActivatePanelByKeyAsync(DockViewId, panelKey, cancellationToken);
    }

    /// <summary>
    /// Sets the width of a drawer panel by key.
    /// </summary>
    /// <param name="panelKey">The unique key of the panel.</param>
    /// <param name="widthPx">The width in pixels.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the operation succeeded.</returns>
    public async Task<bool> SetDrawerWidthByKeyAsync(string panelKey, int widthPx, CancellationToken cancellationToken)
    {
        return await DockViewInterop.SetDrawerWidthByKeyAsync(DockViewId, panelKey, widthPx, cancellationToken);
    }

    #endregion

    /// <summary>
    /// Gets the underlying DockView interop service for advanced operations.
    /// </summary>
    public IDockViewInterop GetInterop() => DockViewInterop;

    private string GetCombinedClass()
    {
        var baseClass = Class ?? string.Empty;

        if (!_isInitialized && !string.IsNullOrEmpty(HiddenClass))
        {
            return $"{baseClass} {HiddenClass}".Trim();
        }

        return baseClass;
    }

    private async Task HandleInitializedAsync()
    {
        System.Console.WriteLine("============================================");
        System.Console.WriteLine("[DockViewV2Wrapper] HandleInitializedAsync called.");
        System.Console.WriteLine($"[DockViewV2Wrapper] GroupConfigurations count: {GroupConfigurations.Count}");

        // Small delay to ensure the dockview is fully rendered
        await Task.Delay(InitialDelayMs);

        // Process group configurations in REVERSE order by GroupIndex
        // This is crucial because unpinning a group shifts the indexes of remaining grid groups.
        // By processing highest index first, we avoid index shifting issues.
        foreach (var groupConfig in GroupConfigurations.OrderByDescending(g => g.GroupIndex))
        {
            System.Console.WriteLine($"[DockViewV2Wrapper] Processing group {groupConfig.GroupIndex}, PinState={groupConfig.PinState}");
            if (groupConfig.PinState == DockPanelPinState.Drawer)
            {
                try
                {
                    System.Console.WriteLine($"[DockViewV2Wrapper] Unpinning group {groupConfig.GroupIndex}...");
                    var result = await DockViewInterop.UnpinGroupAsync(DockViewId, groupConfig.GroupIndex, CancellationToken.None);
                    System.Console.WriteLine($"[DockViewV2Wrapper] UnpinGroupAsync result: {result}");
                    await Task.Delay(DefaultDelayBetweenOperationsMs);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DockViewV2Wrapper] Error unpinning group {groupConfig.GroupIndex}: {ex.Message}");
                }
            }
        }

        System.Console.WriteLine("[DockViewV2Wrapper] All groups unpinned. Processing panels...");

        // After all groups are unpinned, process panel configurations
        // Use a longer delay to ensure drawers are fully created
        await Task.Delay(100);

        foreach (var groupConfig in GroupConfigurations.OrderBy(g => g.GroupIndex))
        {
            // Process panel configurations within the group
            foreach (var panelConfig in groupConfig.Panels)
            {
                try
                {
                    // Hide drawer tab if configured
                    if (panelConfig.DrawerTabVisibility == DrawerTabVisibility.Hidden)
                    {
                        await DockViewInterop.HideDrawerTabByKeyAsync(DockViewId, panelConfig.Key, CancellationToken.None);
                        await Task.Delay(DefaultDelayBetweenOperationsMs);
                    }

                    // Set drawer width
                    var width = panelConfig.DrawerWidthPx ?? DefaultDrawerWidthPx;
                    await DockViewInterop.SetDrawerWidthByKeyAsync(DockViewId, panelConfig.Key, width, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DockViewV2Wrapper] Error configuring panel '{panelConfig.Key}': {ex.Message}");
                }
            }

            // Set static group title if configured (only for drawer groups with panels)
            if (groupConfig.PinState == DockPanelPinState.Drawer 
                && !string.IsNullOrEmpty(groupConfig.GroupTitle)
                && groupConfig.Panels.Count > 0)
            {
                try
                {
                    // Use the first panel's key to find the sidebar button
                    var firstPanelKey = groupConfig.Panels[0].Key;
                    await DockViewInterop.SetGroupStaticTitleByKeyAsync(
                        DockViewId,
                        firstPanelKey,
                        groupConfig.GroupTitle,
                        CancellationToken.None);
                    await Task.Delay(DefaultDelayBetweenOperationsMs);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[DockViewV2Wrapper] Error setting static title for group {groupConfig.GroupIndex}: {ex.Message}");
                }
            }
        }

        // Mark as initialized
        _isInitialized = true;
        IsReady = true;
        await IsReadyChanged.InvokeAsync(true);

        // Invoke our completion callback
        if (OnInitializationCompleteAsync.HasDelegate)
        {
            await OnInitializationCompleteAsync.InvokeAsync();
        }

        System.Console.WriteLine("[DockViewV2Wrapper] Initialization complete.");
    }

    private async Task HandleVisibleStateChangedAsync(string title, bool visible)
    {
        // Forward to the callback
        if (OnVisibleStateChangedAsync.HasDelegate)
        {
            await OnVisibleStateChangedAsync.InvokeAsync((title, visible));
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        GC.SuppressFinalize(this);

        await ValueTask.CompletedTask;
    }
}
