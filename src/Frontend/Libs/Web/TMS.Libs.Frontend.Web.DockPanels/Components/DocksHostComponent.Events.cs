using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

public partial class DocksHostComponent
{
    private readonly object _waitersLock = new();
    private readonly Dictionary<Guid, List<TaskCompletionSource<bool>>> _panelReadyWaiters = [];
    private readonly Dictionary<Guid, List<TaskCompletionSource<bool>>> _drawerReadyWaiters = [];

    /// <summary>
    /// JS-invokable callback when a panel is registered in DockView.
    /// </summary>
    [JSInvokable]
    public async Task PanelAddedCallbackAsync(Guid panelId)
    {
        SignalPanelReady(panelId);

        var panelComponent = FindPanelComponent(panelId);
        if (panelComponent is null)
        {
            _logger?.LogDebug(
                "[{MethodName}] Panel '{PanelId}' not found for registration callback.",
                nameof(PanelAddedCallbackAsync),
                panelId);
            return;
        }

        if (OnPanelAddedAsync.HasDelegate)
        {
            await OnPanelAddedAsync.InvokeAsync(panelComponent);
        }
    }

    /// <summary>
    /// JS-invokable callback when a drawer group is ready.
    /// </summary>
    [JSInvokable]
    public async Task DrawerReadyCallbackAsync(Guid panelId, Guid groupId)
    {
        SignalDrawerReady(panelId, groupId);

        var panelComponent = FindPanelComponent(panelId)
            ?? FindPanelComponentByGroupId(groupId);

        if (panelComponent is null)
        {
            _logger?.LogDebug(
                "[{MethodName}] Panel '{PanelId}' not found for drawer ready callback.",
                nameof(DrawerReadyCallbackAsync),
                panelId);
            return;
        }

        if (OnDrawerReadyAsync.HasDelegate)
        {
            await OnDrawerReadyAsync.InvokeAsync((panelComponent, groupId));
        }
    }

    /// <summary>
    /// Waits until the specified panel is registered with DockView.
    /// </summary>
    public Task<bool> WaitForPanelReadyAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return panel is null
            ? Task.FromResult(false)
            : WaitForPanelReadyAsync(panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Waits until the specified panel ID is registered with DockView.
    /// </summary>
    public async Task<bool> WaitForPanelReadyAsync(Guid panelId, CancellationToken cancellationToken)
    {
        if (panelId == Guid.Empty)
        {
            return false;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        RegisterWaiter(_panelReadyWaiters, panelId, tcs);

        try
        {
            if (await PanelExistsAsync(panelId, cancellationToken))
            {
                SignalPanelReady(panelId);
                return true;
            }

            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            await tcs.Task;
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            RemoveWaiter(_panelReadyWaiters, panelId, tcs);
        }
    }

    /// <summary>
    /// Waits until the drawer for the specified panel is ready.
    /// </summary>
    public Task<bool> WaitForDrawerReadyAsync(DockPanelComponent panel, CancellationToken cancellationToken)
    {
        return panel is null
            ? Task.FromResult(false)
            : WaitForDrawerReadyAsync(panel.PanelId, cancellationToken);
    }

    /// <summary>
    /// Waits until the drawer for the specified panel ID is ready.
    /// </summary>
    public async Task<bool> WaitForDrawerReadyAsync(Guid panelId, CancellationToken cancellationToken)
    {
        if (!await WaitForPanelReadyAsync(panelId, cancellationToken))
        {
            return false;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        RegisterWaiter(_drawerReadyWaiters, panelId, tcs);

        try
        {
            if (await IsDrawerReadyAsync(panelId, cancellationToken))
            {
                SignalDrawerReady(panelId, Guid.Empty);
                return true;
            }

            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            await tcs.Task;
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            RemoveWaiter(_drawerReadyWaiters, panelId, tcs);
        }
    }

    /// <summary>
    /// Ensures a drawer panel is visible and activated.
    /// </summary>
    public async Task<bool> OpenDrawerPanelAsync(
        DockPanelComponent panel,
        int? widthPx,
        string? staticGroupTitle,
        CancellationToken cancellationToken)
    {
        if (panel is null)
        {
            return false;
        }

        if (!await WaitForPanelReadyAsync(panel, cancellationToken))
        {
            _logger?.LogDebug(
                "[{MethodName}] Panel '{PanelId}' was not ready before drawer open.",
                nameof(OpenDrawerPanelAsync),
                panel.PanelId);
            return false;
        }

        var state = await GetPanelStateAsync(panel, cancellationToken);
        if (state is null || !state.IsDrawer)
        {
            var unpinResult = await UnpinGroupAsync(panel, cancellationToken);
            if (!unpinResult && state is not null)
            {
                _logger?.LogDebug(
                    "[{MethodName}] Failed to unpin group for panel '{PanelId}'.",
                    nameof(OpenDrawerPanelAsync),
                    panel.PanelId);
            }
        }

        if (!await WaitForDrawerReadyAsync(panel, cancellationToken))
        {
            _logger?.LogDebug(
                "[{MethodName}] Drawer was not ready for panel '{PanelId}'.",
                nameof(OpenDrawerPanelAsync),
                panel.PanelId);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(staticGroupTitle))
        {
            await SetGroupStaticTitleAsync(panel, staticGroupTitle, cancellationToken);
        }

        var showTabResult = await ShowDrawerTabAsync(panel, widthPx, cancellationToken);
        if (!showTabResult)
        {
            _logger?.LogDebug(
                "[{MethodName}] Drawer tab could not be shown for panel '{PanelId}'.",
                nameof(OpenDrawerPanelAsync),
                panel.PanelId);
        }

        var activateResult = await ActivatePanelAsync(panel, cancellationToken);

        return showTabResult && activateResult;
    }

    private DockPanelComponent? FindPanelComponentByGroupId(Guid groupId)
    {
        if (groupId == Guid.Empty)
        {
            return null;
        }

        var panels = FindPanelsByGroupId(groupId);
        return panels.Count == 0 ? null : panels[0];
    }

    private List<DockPanelComponent> FindPanelsByGroupId(Guid groupId)
    {
        var results = new List<DockPanelComponent>();
        CollectPanelsByGroupId(_components, groupId, results);
        return results;
    }

    private static void CollectPanelsByGroupId(
        IEnumerable<DockPanelComponentBase> components,
        Guid groupId,
        ICollection<DockPanelComponent> results)
    {
        foreach (var component in components)
        {
            if (component is DockPanelComponent panel)
            {
                if (panel.GroupId == groupId)
                {
                    results.Add(panel);
                }
            }
            else if (component is DocksCollectionComponent collection)
            {
                CollectPanelsByGroupId(collection.Items, groupId, results);
            }
        }
    }

    private void SignalPanelReady(Guid panelId)
    {
        SignalWaiters(_panelReadyWaiters, panelId);
    }

    private void SignalDrawerReady(Guid panelId, Guid groupId)
    {
        if (groupId != Guid.Empty)
        {
            var panels = FindPanelsByGroupId(groupId);
            if (panels.Count > 0)
            {
                foreach (var panel in panels)
                {
                    SignalWaiters(_drawerReadyWaiters, panel.PanelId);
                }
                return;
            }
        }

        SignalWaiters(_drawerReadyWaiters, panelId);
    }

    private void RegisterWaiter(
        Dictionary<Guid, List<TaskCompletionSource<bool>>> waiters,
        Guid key,
        TaskCompletionSource<bool> waiter)
    {
        lock (_waitersLock)
        {
            if (!waiters.TryGetValue(key, out var list))
            {
                list = [];
                waiters[key] = list;
            }

            list.Add(waiter);
        }
    }

    private void RemoveWaiter(
        Dictionary<Guid, List<TaskCompletionSource<bool>>> waiters,
        Guid key,
        TaskCompletionSource<bool> waiter)
    {
        lock (_waitersLock)
        {
            if (!waiters.TryGetValue(key, out var list))
            {
                return;
            }

            list.Remove(waiter);
            if (list.Count == 0)
            {
                waiters.Remove(key);
            }
        }
    }

    private void SignalWaiters(
        Dictionary<Guid, List<TaskCompletionSource<bool>>> waiters,
        Guid key)
    {
        List<TaskCompletionSource<bool>>? toRelease = null;

        lock (_waitersLock)
        {
            if (!waiters.TryGetValue(key, out var list))
            {
                return;
            }

            waiters.Remove(key);
            toRelease = list;
        }

        if (toRelease is null)
        {
            return;
        }

        foreach (var waiter in toRelease)
        {
            waiter.TrySetResult(true);
        }
    }

    private void CancelPendingWaiters()
    {
        List<TaskCompletionSource<bool>> waiters;

        lock (_waitersLock)
        {
            waiters = _panelReadyWaiters.Values.SelectMany(list => list)
                .Concat(_drawerReadyWaiters.Values.SelectMany(list => list))
                .ToList();

            _panelReadyWaiters.Clear();
            _drawerReadyWaiters.Clear();
        }

        foreach (var waiter in waiters)
        {
            waiter.TrySetCanceled();
        }
    }
}
