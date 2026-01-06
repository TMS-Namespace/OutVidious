using System.Text.Json;
using Microsoft.JSInterop;
using TMS.Libs.Frontend.Web.DockViewWrapper.Enums;
using TMS.Libs.Frontend.Web.DockViewWrapper.Models;

namespace TMS.Libs.Frontend.Web.DockViewWrapper.Services;

/// <summary>
/// Implementation of <see cref="IDockViewInterop"/> that provides JavaScript interop
/// for controlling DockViewV2 panel states.
/// </summary>
internal sealed class DockViewInterop : IDockViewInterop
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockViewInterop"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime for interop calls.</param>
    public DockViewInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_module is not null)
        {
            return;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            "./_content/TMS.Libs.Frontend.Web.DockViewWrapper/js/dockview-interop.js");
    }

    /// <inheritdoc/>
    public async Task<bool> UnpinGroupAsync(string dockViewId, int groupIndex, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("unpinGroup", cancellationToken, dockViewId, groupIndex);
    }

    /// <inheritdoc/>
    public async Task<bool> UnpinPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("unpinPanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> UnpinGroupByPanelTitleAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("unpinGroupByPanelTitle", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> PinPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("pinPanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> FloatPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("floatPanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> DockPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("dockPanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> CollapsePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("collapsePanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ExpandPanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("expandPanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<DockPanelState?> GetPanelStateAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        
        var jsState = await _module!.InvokeAsync<JsonElement?>("getPanelState", cancellationToken, dockViewId, panelTitle);
        
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
    public async Task<bool> ShowDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showDrawer", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> HideDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("hideDrawer", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ActivatePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("activatePanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> PanelExistsAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("panelExists", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> HideDrawerTabAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("hideDrawerTab", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowDrawerTabAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showDrawerTab", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowDrawerTabAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showDrawerTab", cancellationToken, dockViewId, panelTitle, widthPx);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowAndExpandDrawerAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showAndExpandDrawer", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowAndExpandDrawerAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showAndExpandDrawer", cancellationToken, dockViewId, panelTitle, widthPx);
    }

    /// <inheritdoc/>
    public async Task<bool> SetDrawerWidthAsync(string dockViewId, string panelTitle, int widthPx, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("setDrawerWidth", cancellationToken, dockViewId, panelTitle, widthPx);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowGroupAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showGroup", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> HideGroupAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("hideGroup", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> IsGroupVisibleAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("isGroupVisible", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> RemovePanelAsync(string dockViewId, string panelTitle, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("removePanel", cancellationToken, dockViewId, panelTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> AddPanelToNewGroupAsync(
        string dockViewId,
        string panelId,
        string panelTitle,
        string position,
        int? referenceGroupIndex,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>(
            "addPanelToNewGroup",
            cancellationToken,
            dockViewId,
            panelId,
            panelTitle,
            position,
            referenceGroupIndex);
    }

    /// <inheritdoc/>
    public async Task<bool> SetGroupStaticTitleAsync(
        string dockViewId,
        string panelTitle,
        string staticTitle,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>(
            "setGroupStaticTitle",
            cancellationToken,
            dockViewId,
            panelTitle,
            staticTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ClearGroupStaticTitleAsync(
        string dockViewId,
        string panelTitle,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>(
            "clearGroupStaticTitle",
            cancellationToken,
            dockViewId,
            panelTitle);
    }

    #region Key-based methods (preferred over title-based)

    /// <inheritdoc/>
    public async Task<bool> HideDrawerTabByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("hideDrawerTabByKey", cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public async Task<bool> SetDrawerWidthByKeyAsync(string dockViewId, string panelKey, int widthPx, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("setDrawerWidthByKey", cancellationToken, dockViewId, panelKey, widthPx);
    }

    /// <inheritdoc/>
    public async Task<bool> SetGroupStaticTitleByKeyAsync(
        string dockViewId,
        string panelKey,
        string staticTitle,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>(
            "setGroupStaticTitleByKey",
            cancellationToken,
            dockViewId,
            panelKey,
            staticTitle);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowDrawerTabByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showDrawerTabByKey", cancellationToken, dockViewId, panelKey);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowDrawerTabByKeyAsync(string dockViewId, string panelKey, int widthPx, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("showDrawerTabByKey", cancellationToken, dockViewId, panelKey, widthPx);
    }

    /// <inheritdoc/>
    public async Task<bool> ActivatePanelByKeyAsync(string dockViewId, string panelKey, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        return await _module!.InvokeAsync<bool>("activatePanelByKey", cancellationToken, dockViewId, panelKey);
    }

    #endregion

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
