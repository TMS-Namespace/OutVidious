/**
 * DockView Interop - Float/Dock Operations
 * Handles floating panels and docking them back to the grid.
 */

import { getDockview, findGroupByPanelId } from './dockview-interop-core.js';

/**
 * Floats a panel (converts a grid panel to a floating window).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to float.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function floatPanel(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        if (locationType !== 'grid') {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not in grid mode.`);
            return false;
        }
        
        // Simulate clicking the float button
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const floatBtn = actionContainer?.querySelector('.bb-dockview-control-icon-float');
        
        if (floatBtn) {
            floatBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Float button not found for panel '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error floating panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Docks a floating panel back to the grid.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to dock.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function dockPanel(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        if (locationType !== 'floating') {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not floating.`);
            return false;
        }
        
        // For floating panels (not drawers), click the dock button
        const actionContainer = group.header?.rightActionsContainer;
        const dockBtn = actionContainer?.querySelector('.bb-dockview-control-icon-dock, .bb-dockview-control-icon-pushpin');
        
        if (dockBtn) {
            dockBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Dock button not found for panel '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error docking panel '${panelId}':`, error);
        return false;
    }
}
