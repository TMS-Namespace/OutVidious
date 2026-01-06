/**
 * DockView Interop - Pin/Unpin Operations
 * Handles pinning and unpinning of panels and groups.
 */

import { getDockview, findGroupByPanelTitle, getGroupByIndex } from './dockview-interop-core.js';

/**
 * Unpins a group by its index (converts it to a drawer that slides in/out).
 * Triggers the autoHide functionality for grid groups.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {number} groupIndex - The 0-based index of the group to unpin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function unpinGroup(dockViewId, groupIndex) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = getGroupByIndex(dockview, groupIndex);
        if (!group) {
            console.warn(`[DockViewInterop] Group at index ${groupIndex} not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        if (locationType !== 'grid') {
            console.warn(`[DockViewInterop] Group at index ${groupIndex} is not in grid mode (current: ${locationType}).`);
            return false;
        }
        
        // Simulate clicking the pin button to trigger autoHide
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const pinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pin, .bb-dockview-control-icon-pushpin');
        
        if (pinBtn) {
            console.log(`[DockViewInterop] Unpinning group at index ${groupIndex} (panels: ${group.panels.map(p => p.title).join(', ')})`);
            pinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pin button not found for group at index ${groupIndex}.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error unpinning group at index ${groupIndex}:`, error);
        return false;
    }
}

/**
 * Unpins a panel (converts it to a drawer that slides in/out).
 * Triggers the autoHide functionality for grid panels.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to unpin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function unpinPanel(dockViewId, panelTitle) {
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
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not in grid mode (current: ${locationType}).`);
            return false;
        }
        
        // Simulate clicking the pin button to trigger autoHide
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const pinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pin, .bb-dockview-control-icon-pushpin');
        
        if (pinBtn) {
            pinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pin button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error unpinning panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Pins a panel (docks a drawer back to the grid).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to pin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function pinPanel(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelTitle(dockview, panelTitle);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        const floatType = group.getParams?.()?.floatType;
        
        if (locationType !== 'floating' || floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not a drawer.`);
            return false;
        }
        
        // Simulate clicking the pushpin button to dock
        const actionContainer = group.header?.rightActionsContainer;
        const pushpinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pushpin, .bb-dockview-control-icon-autohide');
        
        if (pushpinBtn) {
            pushpinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pushpin button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error pinning panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Unpins a panel's group by the panel title (converts it to a drawer).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of a panel in the group to unpin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function unpinGroupByPanelTitle(dockViewId, panelTitle) {
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
            console.log(`[DockViewInterop] Panel '${panelTitle}' is not in grid mode (current: ${locationType}). Already unpinned.`);
            return true; // Already unpinned
        }
        
        // Simulate clicking the pin button to trigger autoHide
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const pinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pin, .bb-dockview-control-icon-pushpin');
        
        if (pinBtn) {
            console.log(`[DockViewInterop] Unpinning group containing panel '${panelTitle}'.`);
            pinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pin button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error unpinning group for panel '${panelTitle}':`, error);
        return false;
    }
}
