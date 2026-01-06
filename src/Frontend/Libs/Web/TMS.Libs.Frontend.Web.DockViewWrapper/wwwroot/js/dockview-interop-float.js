/**
 * DockView Interop - Float/Dock Operations
 * Handles floating panels and docking them back to the grid.
 */

import { getDockview, findGroupByPanelTitle } from './dockview-interop-core.js';

/**
 * Floats a panel (converts a grid panel to a floating window).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to float.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function floatPanel(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelTitle(dockview, panelTitle);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        if (locationType !== 'grid') {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not in grid mode.`);
            return false;
        }
        
        // Simulate clicking the float button
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const floatBtn = actionContainer?.querySelector('.bb-dockview-control-icon-float');
        
        if (floatBtn) {
            floatBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Float button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error floating panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Docks a floating panel back to the grid.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to dock.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function dockPanel(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelTitle(dockview, panelTitle);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        if (locationType !== 'floating') {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not floating.`);
            return false;
        }
        
        // For floating panels (not drawers), click the dock button
        const actionContainer = group.header?.rightActionsContainer;
        const dockBtn = actionContainer?.querySelector('.bb-dockview-control-icon-dock, .bb-dockview-control-icon-pushpin');
        
        if (dockBtn) {
            dockBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Dock button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error docking panel '${panelTitle}':`, error);
        return false;
    }
}
