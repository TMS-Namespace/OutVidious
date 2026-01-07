using Microsoft.Extensions.Logging;
using TMS.Libs.Frontend.Web.DockPanels.Enums;
using TMS.Libs.Frontend.Web.DockPanels.Models;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

public partial class DocksHostComponent
{
    private async Task ApplyInitialConfigurationAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await ApplyGroupConfigurationsAsync(GroupConfigurations, cancellationToken);
        await MarkReadyAsync();
    }

    public async Task ApplyGroupConfigurationsAsync(
        IReadOnlyList<DockGroupConfiguration> groupConfigurations,
        CancellationToken cancellationToken)
    {
        if (groupConfigurations.Count == 0)
        {
            return;
        }

        async Task<DockPanelState?> TryGetPanelStateAsync(DockPanelComponent panel)
        {
            try
            {
                return await GetPanelStateAsync(panel, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "[{MethodName}] Unexpected error: Failed to read state for panel '{PanelId}'.",
                    nameof(ApplyGroupConfigurationsAsync),
                    panel.PanelId);
                return null;
            }
        }

        await Task.Delay(InitialDelayMs, cancellationToken);

        var unpinPerformed = false;

        foreach (var groupConfig in groupConfigurations)
        {
            if (groupConfig.PinState != DocksCollectionPinState.Drawer)
            {
                continue;
            }

            try
            {
                if (groupConfig.GroupPanel is null)
                {
                    _logger?.LogDebug(
                        "[{MethodName}] Skipping group configuration with missing panel reference.",
                        nameof(ApplyGroupConfigurationsAsync));
                    continue;
                }

                if (!await WaitForPanelReadyAsync(groupConfig.GroupPanel, cancellationToken))
                {
                    _logger?.LogDebug(
                        "[{MethodName}] Panel '{PanelId}' was not ready for group configuration.",
                        nameof(ApplyGroupConfigurationsAsync),
                        groupConfig.GroupPanel.PanelId);
                    continue;
                }

                var panelState = await TryGetPanelStateAsync(groupConfig.GroupPanel);
                if (panelState is null)
                {
                    _logger?.LogDebug(
                        "[{MethodName}] Panel state unavailable for panel '{PanelId}'.",
                        nameof(ApplyGroupConfigurationsAsync),
                        groupConfig.GroupPanel.PanelId);
                    continue;
                }

                if (panelState.IsDrawer)
                {
                    _logger?.LogDebug(
                        "[{MethodName}] Panel '{PanelId}' already in drawer mode; skipping unpin.",
                        nameof(ApplyGroupConfigurationsAsync),
                        groupConfig.GroupPanel.PanelId);
                    continue;
                }

                if (panelState.LocationType != DockPanelLocationType.Grid)
                {
                    _logger?.LogDebug(
                        "[{MethodName}] Panel '{PanelId}' is not in grid mode; skipping unpin.",
                        nameof(ApplyGroupConfigurationsAsync),
                        groupConfig.GroupPanel.PanelId);
                    continue;
                }

                _logger?.LogDebug(
                    "[{MethodName}] Unpinning group for panel '{PanelId}'.",
                    nameof(ApplyGroupConfigurationsAsync),
                    groupConfig.GroupPanel.PanelId);

                var unpinned = await DockPanelInterop.UnpinPanelAsync(
                    DockPanelId,
                    groupConfig.GroupPanel.PanelId,
                    cancellationToken);
                if (unpinned)
                {
                    unpinPerformed = true;
                    await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "[{MethodName}] Unexpected error: Failed to unpin group for panel '{PanelId}'.",
                    nameof(ApplyGroupConfigurationsAsync),
                    groupConfig.GroupPanel?.PanelId);
                // Intentional: continue applying remaining configuration even if this group fails.
            }
        }

        if (unpinPerformed)
        {
            await Task.Delay(DrawerConfigurationDelayMs, cancellationToken);
        }

        foreach (var groupConfig in groupConfigurations)
        {
            foreach (var panelConfig in groupConfig.Panels)
            {
                try
                {
                    if (panelConfig.Panel is null)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Skipping panel configuration with missing panel reference.",
                            nameof(ApplyGroupConfigurationsAsync));
                        continue;
                    }

                    if (!await WaitForPanelReadyAsync(panelConfig.Panel, cancellationToken))
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Panel '{PanelId}' was not ready for drawer configuration.",
                            nameof(ApplyGroupConfigurationsAsync),
                            panelConfig.Panel.PanelId);
                        continue;
                    }

                    var panelState = await TryGetPanelStateAsync(panelConfig.Panel);
                    if (panelState is null)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Panel state unavailable for panel '{PanelId}'.",
                            nameof(ApplyGroupConfigurationsAsync),
                            panelConfig.Panel.PanelId);
                        continue;
                    }

                    if (!panelState.IsDrawer)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Panel '{PanelId}' is not in drawer mode; skipping drawer configuration.",
                            nameof(ApplyGroupConfigurationsAsync),
                            panelConfig.Panel.PanelId);
                        continue;
                    }

                    if (!await WaitForDrawerReadyAsync(panelConfig.Panel, cancellationToken))
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Drawer not ready for panel '{PanelId}'.",
                            nameof(ApplyGroupConfigurationsAsync),
                            panelConfig.Panel.PanelId);
                        continue;
                    }

                    if (panelConfig.DrawerTabVisibility == DrawerTabVisibility.Hidden)
                    {
                        var hidden = await DockPanelInterop.HideDrawerTabAsync(
                            DockPanelId,
                            panelConfig.Panel.PanelId,
                            cancellationToken);
                        if (hidden)
                        {
                            await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                        }
                    }

                    var width = panelConfig.DrawerWidthPx ?? DefaultDrawerWidthPx;
                    await DockPanelInterop.SetDrawerWidthAsync(
                        DockPanelId,
                        panelConfig.Panel.PanelId,
                        width,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(
                        ex,
                        "[{MethodName}] Unexpected error: Failed to configure panel '{PanelId}'.",
                        nameof(ApplyGroupConfigurationsAsync),
                        panelConfig.Panel?.PanelId);
                    // Intentional: continue applying remaining panel configurations.
                }
            }

            if (groupConfig.PinState == DocksCollectionPinState.Drawer
                && !string.IsNullOrWhiteSpace(groupConfig.GroupTitle)
                && groupConfig.Panels.Count > 0
                && groupConfig.Panels[0].Panel is not null)
            {
                try
                {
                    var firstPanel = groupConfig.Panels[0].Panel;
                    if (!await WaitForPanelReadyAsync(firstPanel, cancellationToken))
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Panel '{PanelId}' was not ready for static title configuration.",
                            nameof(ApplyGroupConfigurationsAsync),
                            firstPanel.PanelId);
                        continue;
                    }

                    var panelState = await TryGetPanelStateAsync(firstPanel);
                    if (panelState is null)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Panel state unavailable for panel '{PanelId}'.",
                            nameof(ApplyGroupConfigurationsAsync),
                            firstPanel.PanelId);
                        continue;
                    }

                    if (!panelState.IsDrawer)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Panel '{PanelId}' is not in drawer mode; skipping static title.",
                            nameof(ApplyGroupConfigurationsAsync),
                            firstPanel.PanelId);
                        continue;
                    }

                    if (!await WaitForDrawerReadyAsync(firstPanel, cancellationToken))
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Drawer not ready for panel '{PanelId}'.",
                            nameof(ApplyGroupConfigurationsAsync),
                            firstPanel.PanelId);
                        continue;
                    }

                    var firstPanelId = firstPanel.PanelId;

                    // If all panels in this group have hidden visibility, we need to show the tab first
                    // so that the sidebar button exists before we set the static title.
                    var allPanelsHidden = groupConfig.Panels.All(p => p.DrawerTabVisibility == DrawerTabVisibility.Hidden);

                    if (allPanelsHidden)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] All tabs in group for panel '{PanelId}' are hidden; showing tab temporarily to set group title.",
                            nameof(ApplyGroupConfigurationsAsync),
                            firstPanelId);

                        // Temporarily show the tab so the sidebar button gets created
                        await DockPanelInterop.ShowDrawerTabAsync(DockPanelId, firstPanelId, cancellationToken);
                        await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                    }

                    // Now set the static title (the sidebar button should exist)
                    await DockPanelInterop.SetGroupStaticTitleAsync(
                        DockPanelId,
                        firstPanelId,
                        groupConfig.GroupTitle,
                        cancellationToken);

                    await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);

                    // If we showed the tab, hide it again to restore the original hidden state
                    if (allPanelsHidden)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Hiding tab in group for panel '{PanelId}' to restore hidden state.",
                            nameof(ApplyGroupConfigurationsAsync),
                            firstPanelId);

                        await DockPanelInterop.HideDrawerTabAsync(DockPanelId, firstPanelId, cancellationToken);
                        await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(
                        ex,
                        "[{MethodName}] Unexpected error: Failed to set static title for group for panel '{PanelId}'.",
                        nameof(ApplyGroupConfigurationsAsync),
                        groupConfig.Panels[0].Panel?.PanelId);
                    // Intentional: continue applying remaining group configurations.
                }
            }
        }
    }

    private async Task MarkReadyAsync()
    {
        _isInitialized = true;
        IsReady = true;

        if (IsReadyChanged.HasDelegate)
        {
            await IsReadyChanged.InvokeAsync(true);
        }

        if (OnInitializationCompleteAsync.HasDelegate)
        {
            await OnInitializationCompleteAsync.InvokeAsync();
        }
    }
}
