using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using TMS.Libs.Frontend.Web.DockPanels.Enums;
using TMS.Libs.Frontend.Web.DockPanels.Models;

namespace TMS.Libs.Frontend.Web.DockPanels.Services;

/// <summary>
/// Implementation of <see cref="IDockPanelInterop"/> that provides JavaScript interop
/// for controlling dock panel states.
/// </summary>
internal sealed class DockPanelInterop : IDockPanelInterop
{
    private const string InteropModulePath = "./_content/TMS.Libs.Frontend.Web.DockPanels/js/dockview-interop.js";

    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<DockPanelInterop> _logger;
    private IJSObjectReference? _module;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockPanelInterop"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime for interop calls.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public DockPanelInterop(IJSRuntime jsRuntime, ILoggerFactory loggerFactory)
    {
        _jsRuntime = jsRuntime;
        _logger = loggerFactory.CreateLogger<DockPanelInterop>();
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_module is not null)
        {
            return;
        }

        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                cancellationToken,
                InteropModulePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{MethodName}] Unexpected error: Failed to load dock panel interop module.", nameof(InitializeAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<bool> UnpinGroupAsync(string dockViewId, int groupIndex, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("unpinGroup", nameof(UnpinGroupAsync), cancellationToken, dockViewId, groupIndex);
    }

    /// <inheritdoc/>
    public Task<bool> UnpinPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("unpinPanel", nameof(UnpinPanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> UnpinGroupByPanelTitleAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("unpinGroupByPanelTitle", nameof(UnpinGroupByPanelTitleAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> PinPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("pinPanel", nameof(PinPanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> FloatPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("floatPanel", nameof(FloatPanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> DockPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("dockPanel", nameof(DockPanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> CollapsePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("collapsePanel", nameof(CollapsePanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ExpandPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("expandPanel", nameof(ExpandPanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<DockPanelState?> GetPanelStateAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        var jsState = await InvokeInteropAsync<JsonElement?>(
            "getPanelState",
            nameof(GetPanelStateAsync),
            cancellationToken,
            dockViewId,
            panelTitle);

        if (jsState is null || jsState.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var state = jsState.Value;
        var locationTypeStr = state.GetProperty("locationType").GetString() ?? "unknown";
        var locationType = locationTypeStr switch
        {
            "grid" => DockPanelLocationType.Grid,
            "floating" => DockPanelLocationType.Floating,
            "popout" => DockPanelLocationType.Popout,
            _ => DockPanelLocationType.Unknown
        };

        return new DockPanelState
        {
            LocationType = locationType,
            IsDrawer = state.TryGetProperty("isDrawer", out var isDrawer) && isDrawer.GetBoolean(),
            IsDrawerVisible = state.TryGetProperty("isDrawerVisible", out var isDrawerVisible) && isDrawerVisible.GetBoolean(),
            IsCollapsed = state.TryGetProperty("isCollapsed", out var isCollapsed) && isCollapsed.GetBoolean(),
            IsMaximized = state.TryGetProperty("isMaximized", out var isMaximized) && isMaximized.GetBoolean(),
            IsLocked = state.TryGetProperty("isLocked", out var isLocked) && isLocked.GetBoolean(),
            IsVisible = !state.TryGetProperty("isVisible", out var isVisible) || isVisible.GetBoolean()
        };
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawer", nameof(ShowDrawerAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> HideDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideDrawer", nameof(HideDrawerAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ActivatePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("activatePanel", nameof(ActivatePanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> SetActivePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("setActivePanel", nameof(SetActivePanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> PanelExistsAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("panelExists", nameof(PanelExistsAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> PanelExistsByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("panelExistsByKey", nameof(PanelExistsByKeyAsync), cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public Task<bool> HideDrawerTabAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideDrawerTab", nameof(HideDrawerTabAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerTabAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawerTab", nameof(ShowDrawerTabAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerTabAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawerTab", nameof(ShowDrawerTabAsync), cancellationToken, dockViewId, panelTitle, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> ShowAndExpandDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showAndExpandDrawer", nameof(ShowAndExpandDrawerAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ShowAndExpandDrawerAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showAndExpandDrawer", nameof(ShowAndExpandDrawerAsync), cancellationToken, dockViewId, panelTitle, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> SetDrawerWidthAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("setDrawerWidth", nameof(SetDrawerWidthAsync), cancellationToken, dockViewId, panelTitle, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> IsDrawerReadyAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("isDrawerReady", nameof(IsDrawerReadyAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ShowGroupAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showGroup", nameof(ShowGroupAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> HideGroupAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideGroup", nameof(HideGroupAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> IsGroupVisibleAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("isGroupVisible", nameof(IsGroupVisibleAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> RemovePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("removePanel", nameof(RemovePanelAsync), cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public Task<bool> AddPanelToNewGroupAsync(
        string dockViewId,
        string panelId,
        string panelTitle,
        string position,
        int? referenceGroupIndex,
        CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>(
            "addPanelToNewGroup",
            nameof(AddPanelToNewGroupAsync),
            cancellationToken,
            dockViewId,
            panelId,
            panelTitle,
            position,
            referenceGroupIndex);
    }

    /// <inheritdoc/>
    public Task<bool> SetGroupStaticTitleAsync(
        string dockViewId,
        string panelTitle,
        string staticTitle,
        CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>(
            "setGroupStaticTitle",
            nameof(SetGroupStaticTitleAsync),
            cancellationToken,
            dockViewId,
            panelTitle,
            staticTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ClearGroupStaticTitleAsync(
        string dockViewId,
        string panelTitle,
        CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>(
            "clearGroupStaticTitle",
            nameof(ClearGroupStaticTitleAsync),
            cancellationToken,
            dockViewId,
            panelTitle);
    }

    #region Key-based methods (preferred over title-based)

    /// <inheritdoc/>
    public Task<bool> UnpinGroupByPanelKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("unpinGroupByPanelKey", nameof(UnpinGroupByPanelKeyAsync), cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public Task<bool> HideDrawerTabByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideDrawerTabByKey", nameof(HideDrawerTabByKeyAsync), cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public Task<bool> SetDrawerWidthByKeyAsync(string dockViewId, string panelKey, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("setDrawerWidthByKey", nameof(SetDrawerWidthByKeyAsync), cancellationToken, dockViewId, panelKey, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> SetGroupStaticTitleByKeyAsync(
        string dockViewId,
        string panelKey,
        string staticTitle,
        CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>(
            "setGroupStaticTitleByKey",
            nameof(SetGroupStaticTitleByKeyAsync),
            cancellationToken,
            dockViewId,
            panelKey,
            staticTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerTabByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawerTabByKey", nameof(ShowDrawerTabByKeyAsync), cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerTabByKeyAsync(string dockViewId, string panelKey, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawerTabByKey", nameof(ShowDrawerTabByKeyAsync), cancellationToken, dockViewId, panelKey, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> ActivatePanelByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("activatePanelByKey", nameof(ActivatePanelByKeyAsync), cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public Task<bool> SetActivePanelByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("setActivePanelByKey", nameof(SetActivePanelByKeyAsync), cancellationToken, dockViewId, panelKey);
    }

    #endregion

    private async Task<T> InvokeInteropAsync<T>(
        string identifier,
        string methodName,
        CancellationToken cancellationToken,
        params object?[] args)
    {
        try
        {
            await EnsureInitializedAsync(cancellationToken);
            return await _module!.InvokeAsync<T>(identifier, cancellationToken, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{MethodName}] Unexpected error: Failed to invoke interop '{InteropMethod}'.",
                methodName,
                identifier);
            throw;
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_module is null)
        {
            await InitializeAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }

        _isDisposed = true;
    }
}
