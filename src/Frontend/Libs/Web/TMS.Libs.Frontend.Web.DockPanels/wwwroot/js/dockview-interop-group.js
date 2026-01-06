/**
 * DockView Interop - Group Visibility Operations
 * Handles showing/hiding and visibility checks for panel groups.
 */

import { getDockview, findPanelByTitle } from './dockview-interop-core.js';

/**
 * Shows a panel's group (makes it visible in the dock).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel whose group to show.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showGroup(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelByTitle(dockview, panelTitle);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const group = panel.group;
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' has no group.`);
            return false;
        }
        
        // Set the group to visible
        if (group.api && typeof group.api.setVisible === 'function') {
            group.api.setVisible(true);
            console.log(`[DockViewInterop] Showed group containing panel '${panelTitle}'.`);
            return true;
        }
        
        console.warn(`[DockViewInterop] Could not show group for panel '${panelTitle}' - no setVisible API.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing group for panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Hides a panel's group (makes it invisible in the dock).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel whose group to hide.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function hideGroup(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelByTitle(dockview, panelTitle);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const group = panel.group;
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' has no group.`);
            return false;
        }
        
        // Set the group to hidden
        if (group.api && typeof group.api.setVisible === 'function') {
            group.api.setVisible(false);
            console.log(`[DockViewInterop] Hid group containing panel '${panelTitle}'.`);
            return true;
        }
        
        console.warn(`[DockViewInterop] Could not hide group for panel '${panelTitle}' - no setVisible API.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error hiding group for panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Checks if a panel's group is visible.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to check.
 * @returns {Promise<boolean>} True if the group is visible, false otherwise.
 */
export async function isGroupVisible(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelByTitle(dockview, panelTitle);
        if (!panel) return false;
        
        const group = panel.group;
        if (!group) return false;
        
        return group.api?.isVisible ?? true;
    } catch (error) {
        console.error(`[DockViewInterop] Error checking visibility for panel '${panelTitle}':`, error);
        return false;
    }
}
