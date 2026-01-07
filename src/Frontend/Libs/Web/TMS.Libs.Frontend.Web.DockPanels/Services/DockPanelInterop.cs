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
    public Task<bool> UnpinGroupAsync(string dockViewId, Guid groupId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("unpinGroup", nameof(UnpinGroupAsync), cancellationToken, dockViewId, groupId);
    }

    /// <inheritdoc/>
    public Task<bool> UnpinPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("unpinPanel", nameof(UnpinPanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> PinPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("pinPanel", nameof(PinPanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> FloatPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("floatPanel", nameof(FloatPanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> DockPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("dockPanel", nameof(DockPanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> CollapsePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("collapsePanel", nameof(CollapsePanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> ExpandPanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("expandPanel", nameof(ExpandPanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public async Task<DockPanelState?> GetPanelStateAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        var jsState = await InvokeInteropAsync<JsonElement?>(
            "getPanelState",
            nameof(GetPanelStateAsync),
            cancellationToken,
            dockViewId,
            panelId);

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
    public Task<bool> ShowDrawerAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawer", nameof(ShowDrawerAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> HideDrawerAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideDrawer", nameof(HideDrawerAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> ActivatePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("activatePanel", nameof(ActivatePanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> SetActivePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("setActivePanel", nameof(SetActivePanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> PanelExistsAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("panelExists", nameof(PanelExistsAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> HideDrawerTabAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideDrawerTab", nameof(HideDrawerTabAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerTabAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawerTab", nameof(ShowDrawerTabAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> ShowDrawerTabAsync(string dockViewId, Guid panelId, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showDrawerTab", nameof(ShowDrawerTabAsync), cancellationToken, dockViewId, panelId, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> ShowAndExpandDrawerAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showAndExpandDrawer", nameof(ShowAndExpandDrawerAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> ShowAndExpandDrawerAsync(string dockViewId, Guid panelId, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showAndExpandDrawer", nameof(ShowAndExpandDrawerAsync), cancellationToken, dockViewId, panelId, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> SetDrawerWidthAsync(string dockViewId, Guid panelId, int widthPx, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("setDrawerWidth", nameof(SetDrawerWidthAsync), cancellationToken, dockViewId, panelId, widthPx);
    }

    /// <inheritdoc/>
    public Task<bool> IsDrawerReadyAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("isDrawerReady", nameof(IsDrawerReadyAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> ShowGroupAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("showGroup", nameof(ShowGroupAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> HideGroupAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("hideGroup", nameof(HideGroupAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> IsGroupVisibleAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("isGroupVisible", nameof(IsGroupVisibleAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> RemovePanelAsync(string dockViewId, Guid panelId, CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>("removePanel", nameof(RemovePanelAsync), cancellationToken, dockViewId, panelId);
    }

    /// <inheritdoc/>
    public Task<bool> AddPanelToNewGroupAsync(
        string dockViewId,
        Guid panelId,
        string panelTitle,
        string position,
        Guid? referenceGroupId,
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
            referenceGroupId);
    }

    /// <inheritdoc/>
    public Task<bool> SetGroupStaticTitleAsync(
        string dockViewId,
        Guid panelId,
        string staticTitle,
        CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>(
            "setGroupStaticTitle",
            nameof(SetGroupStaticTitleAsync),
            cancellationToken,
            dockViewId,
            panelId,
            staticTitle);
    }

    /// <inheritdoc/>
    public Task<bool> ClearGroupStaticTitleAsync(
        string dockViewId,
        Guid panelId,
        CancellationToken cancellationToken)
    {
        return InvokeInteropAsync<bool>(
            "clearGroupStaticTitle",
            nameof(ClearGroupStaticTitleAsync),
            cancellationToken,
            dockViewId,
            panelId);
    }

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
