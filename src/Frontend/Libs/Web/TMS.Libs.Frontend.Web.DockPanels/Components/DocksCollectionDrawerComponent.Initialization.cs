using Microsoft.Extensions.Logging;
using TMS.Libs.Frontend.Web.DockPanels.Enums;

namespace TMS.Libs.Frontend.Web.DockPanels.Components;

public partial class DocksCollectionDrawerComponent
{
    private async Task ApplyInitialConfigurationAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        if (GroupConfigurations.Count == 0)
        {
            await MarkReadyAsync();
            return;
        }

        await Task.Delay(InitialDelayMs, cancellationToken);

        foreach (var groupConfig in GroupConfigurations.OrderByDescending(group => group.GroupIndex))
        {
            if (groupConfig.PinState != DockPanelPinState.Drawer)
            {
                continue;
            }

            try
            {
                _logger?.LogDebug(
                    "[{MethodName}] Unpinning group '{GroupIndex}'.",
                    nameof(ApplyInitialConfigurationAsync),
                    groupConfig.GroupIndex);

                await DockPanelInterop.UnpinGroupAsync(DockPanelId, groupConfig.GroupIndex, cancellationToken);
                await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "[{MethodName}] Unexpected error: Failed to unpin group '{GroupIndex}'.",
                    nameof(ApplyInitialConfigurationAsync),
                    groupConfig.GroupIndex);
                // Intentional: continue applying remaining configuration even if this group fails.
            }
        }

        await Task.Delay(DrawerConfigurationDelayMs, cancellationToken);

        foreach (var groupConfig in GroupConfigurations.OrderBy(group => group.GroupIndex))
        {
            foreach (var panelConfig in groupConfig.Panels)
            {
                try
                {
                    if (panelConfig.DrawerTabVisibility == DrawerTabVisibility.Hidden)
                    {
                        await DockPanelInterop.HideDrawerTabByKeyAsync(DockPanelId, panelConfig.Key, cancellationToken);
                        await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                    }

                    var width = panelConfig.DrawerWidthPx ?? DefaultDrawerWidthPx;
                    await DockPanelInterop.SetDrawerWidthByKeyAsync(DockPanelId, panelConfig.Key, width, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(
                        ex,
                        "[{MethodName}] Unexpected error: Failed to configure panel '{PanelKey}'.",
                        nameof(ApplyInitialConfigurationAsync),
                        panelConfig.Key);
                    // Intentional: continue applying remaining panel configurations.
                }
            }

            if (groupConfig.PinState == DockPanelPinState.Drawer
                && !string.IsNullOrWhiteSpace(groupConfig.GroupTitle)
                && groupConfig.Panels.Count > 0)
            {
                try
                {
                    var firstPanelKey = groupConfig.Panels[0].Key;
                    
                    // If all panels in this group have hidden visibility, we need to show the tab first
                    // so that the sidebar button exists before we set the static title.
                    var allPanelsHidden = groupConfig.Panels.All(p => p.DrawerTabVisibility == DrawerTabVisibility.Hidden);
                    
                    if (allPanelsHidden)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] All tabs in group '{GroupIndex}' are hidden; showing tab temporarily to set group title.",
                            nameof(ApplyInitialConfigurationAsync),
                            groupConfig.GroupIndex);
                        
                        // Temporarily show the tab so the sidebar button gets created
                        await DockPanelInterop.ShowDrawerTabByKeyAsync(DockPanelId, firstPanelKey, cancellationToken);
                        await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                    }
                    
                    // Now set the static title (the sidebar button should exist)
                    await DockPanelInterop.SetGroupStaticTitleByKeyAsync(
                        DockPanelId,
                        firstPanelKey,
                        groupConfig.GroupTitle,
                        cancellationToken);

                    await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                    
                    // If we showed the tab, hide it again to restore the original hidden state
                    if (allPanelsHidden)
                    {
                        _logger?.LogDebug(
                            "[{MethodName}] Hiding tab in group '{GroupIndex}' to restore hidden state.",
                            nameof(ApplyInitialConfigurationAsync),
                            groupConfig.GroupIndex);
                        
                        await DockPanelInterop.HideDrawerTabByKeyAsync(DockPanelId, firstPanelKey, cancellationToken);
                        await Task.Delay(DefaultDelayBetweenOperationsMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(
                        ex,
                        "[{MethodName}] Unexpected error: Failed to set static title for group '{GroupIndex}'.",
                        nameof(ApplyInitialConfigurationAsync),
                        groupConfig.GroupIndex);
                    // Intentional: continue applying remaining group configurations.
                }
            }
        }

        await MarkReadyAsync();
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
