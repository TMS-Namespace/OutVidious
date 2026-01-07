/**
 * DockView Interop - Pin/Unpin Operations
 * Handles pinning and unpinning of panels and groups.
 */

import { getDockview, findGroupByPanelId, getGroupById } from './dockview-interop-core.js';

/**
 * Unpins a group by its ID (converts it to a drawer that slides in/out).
 * Triggers the autoHide functionality for grid groups.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} groupId - The ID of the group to unpin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function unpinGroup(dockViewId, groupId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = getGroupById(dockview, groupId);
        if (!group) {
            console.warn(`[DockViewInterop] Group '${groupId}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        if (locationType !== 'grid') {
            console.debug(`[DockViewInterop] Group '${groupId}' is not in grid mode (current: ${locationType}).`);
            return true;
        }
        
        // Simulate clicking the pin button to trigger autoHide
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const pinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pin, .bb-dockview-control-icon-pushpin');
        
        if (pinBtn) {
            console.log(`[DockViewInterop] Unpinning group '${groupId}' (panels: ${group.panels.map(p => p.title).join(', ')})`);
            pinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pin button not found for group '${groupId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error unpinning group '${groupId}':`, error);
        return false;
    }
}

/**
 * Unpins a panel (converts it to a drawer that slides in/out).
 * Triggers the autoHide functionality for grid panels.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to unpin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function unpinPanel(dockViewId, panelId) {
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
            console.debug(`[DockViewInterop] Panel '${panelId}' is not in grid mode (current: ${locationType}).`);
            return true;
        }
        
        // Simulate clicking the pin button to trigger autoHide
        const actionContainer = group.header?.element?.querySelector('.dv-right-actions-container');
        const pinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pin, .bb-dockview-control-icon-pushpin');
        
        if (pinBtn) {
            pinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pin button not found for panel '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error unpinning panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Pins a panel (docks a drawer back to the grid).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to pin.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function pinPanel(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const locationType = group.model?.location?.type;
        const floatType = group.getParams?.()?.floatType;
        
        if (locationType !== 'floating' || floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not a drawer.`);
            return false;
        }
        
        // Simulate clicking the pushpin button to dock
        const actionContainer = group.header?.rightActionsContainer;
        const pushpinBtn = actionContainer?.querySelector('.bb-dockview-control-icon-pushpin, .bb-dockview-control-icon-autohide');
        
        if (pushpinBtn) {
            pushpinBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Pushpin button not found for panel '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error pinning panel '${panelId}':`, error);
        return false;
    }
}
