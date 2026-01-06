/**
 * DockView Interop - Panel Management
 * Handles panel state, activation, and lifecycle operations.
 */

import { getDockview, findPanelByTitle, findGroupByPanelTitle, getGroupByIndex } from './dockview-interop-core.js';

/**
 * Collapses a floating panel (shows only the header).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to collapse.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function collapsePanel(dockViewId, panelTitle) {
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
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not floating (collapse only works for floating).`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (params.packup?.isPackup) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is already collapsed.`);
            return true;
        }
        
        // Click the down/collapse button
        const actionContainer = group.header?.rightActionsContainer;
        const downBtn = actionContainer?.querySelector('.bb-dockview-control-icon-down');
        
        if (downBtn) {
            downBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Collapse button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error collapsing panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Expands a collapsed floating panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to expand.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function expandPanel(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelTitle(dockview, panelTitle);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (!params.packup?.isPackup) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not collapsed.`);
            return true;
        }
        
        // Click the up/expand button
        const actionContainer = group.header?.rightActionsContainer;
        const upBtn = actionContainer?.querySelector('.bb-dockview-control-icon-up, .bb-dockview-control-icon-down');
        
        if (upBtn) {
            upBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Expand button not found for panel '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error expanding panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Activates (focuses) a panel and ensures its group is visible.
 * If the panel is in a drawer, the drawer will be expanded by clicking the sidebar button (if not already expanded).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to activate.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function activatePanel(dockViewId, panelTitle) {
    console.log(`[DockViewInterop] ========== activatePanel START for '${panelTitle}' ==========`);
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) {
            console.error(`[DockViewInterop] activatePanel: dockview not found for '${dockViewId}'`);
            return false;
        }
        
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
        
        // Check if this is a drawer - if so, expand it via button click
        const params = group.getParams?.() || {};
        
        if (params.floatType === 'drawer') {
            // Don't trust group.api.isVisible - it doesn't reflect actual DOM state after our hide/show operations
            // Instead, check if the drawer container is actually visible in the DOM
            const drawerContainer = group.element?.closest('.dv-resize-container');
            const isActuallyVisible = drawerContainer && 
                                      drawerContainer.style.display !== 'none' && 
                                      drawerContainer.offsetWidth > 0 &&
                                      drawerContainer.offsetHeight > 0;
            
            console.log(`[DockViewInterop] activatePanel: API isVisible=${group.api?.isVisible}, DOM isActuallyVisible=${isActuallyVisible}`);
            console.log(`[DockViewInterop] activatePanel: drawerContainer found=${!!drawerContainer}, display='${drawerContainer?.style.display}', offsetWidth=${drawerContainer?.offsetWidth}`);
            
            // Find and click the sidebar button to expand the drawer
            const dockviewEl = document.getElementById(dockViewId);
            
            if (dockviewEl) {
                const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
                let matchedBtn = null;
                
                for (const btn of asideButtons) {
                    const btnText = btn.textContent?.trim();
                    const spanText = btn.querySelector('span')?.textContent?.trim();
                    const isMatch = btnText === panelTitle || spanText === panelTitle || btnText?.includes(panelTitle);
                    
                    if (isMatch) {
                        matchedBtn = btn;
                        break;
                    }
                }
                
                if (matchedBtn) {
                    // Check if button is visible (not hidden by us)
                    const isButtonVisible = matchedBtn.style.display !== 'none';
                    console.log(`[DockViewInterop] activatePanel: button found, isButtonVisible=${isButtonVisible}`);
                    
                    // If drawer is NOT actually visible in DOM, click to expand
                    // Also click if the button was previously hidden (we just showed it)
                    if (!isActuallyVisible || matchedBtn.dataset.justShown === 'true') {
                        // Ensure the button is visible before clicking
                        if (!isButtonVisible) {
                            matchedBtn.style.display = '';
                            delete matchedBtn.dataset.hiddenByFrontTube;
                            delete matchedBtn.dataset.hiddenPanelTitle;
                            console.log(`[DockViewInterop] Made sidebar button visible for '${panelTitle}'.`);
                            await new Promise(resolve => setTimeout(resolve, 50));
                        }
                        
                        console.log(`[DockViewInterop] activatePanel: Clicking button to expand drawer for '${panelTitle}'`);
                        matchedBtn.click();
                        
                        // Clear the justShown flag
                        delete matchedBtn.dataset.justShown;
                        
                        console.log(`[DockViewInterop] ========== activatePanel END (clicked) ==========`);
                        return true;
                    } else {
                        console.log(`[DockViewInterop] activatePanel: Drawer already visible in DOM, skipping click`);
                    }
                } else {
                    console.warn(`[DockViewInterop] Could not find sidebar button for drawer '${panelTitle}'.`);
                }
            }
            
            console.log(`[DockViewInterop] ========== activatePanel END (drawer) ==========`);
            return true;
        }
        
        // Activate the panel (for non-drawer panels)
        panel.api?.setActive();
        
        console.log(`[DockViewInterop] Activated panel '${panelTitle}'.`);
        console.log(`[DockViewInterop] ========== activatePanel END ==========`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error activating panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Gets the current state of a panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel.
 * @returns {Promise<object|null>} The panel state or null if not found.
 */
export async function getPanelState(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return null;
        
        const panel = findPanelByTitle(dockview, panelTitle);
        if (!panel) {
            return null;
        }
        
        const group = panel.group;
        const locationType = group?.model?.location?.type || 'unknown';
        const params = group?.getParams?.() || {};
        
        return {
            locationType: locationType,
            isDrawer: params.floatType === 'drawer',
            isDrawerVisible: params.drawer?.visible ?? false,
            isCollapsed: params.packup?.isPackup ?? false,
            isMaximized: params.isMaximized ?? group?.api?.isMaximized?.() ?? false,
            isLocked: params.isLock ?? group?.locked ?? false,
            isVisible: panel.params?.visible !== false
        };
    } catch (error) {
        console.error(`[DockViewInterop] Error getting panel state for '${panelTitle}':`, error);
        return null;
    }
}

/**
 * Checks if a panel with the given title exists.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to check.
 * @returns {Promise<boolean>} True if the panel exists.
 */
export async function panelExists(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelByTitle(dockview, panelTitle);
        return panel !== null;
    } catch (error) {
        console.error(`[DockViewInterop] Error checking panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Adds a new panel to a new group at the specified position.
 * This creates an isolated group for the panel, not merged with existing groups.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The unique ID for the new panel.
 * @param {string} panelTitle - The title for the new panel.
 * @param {string} position - Where to place the group: 'top', 'bottom', 'left', 'right', or 'within' (relative to referenceGroup).
 * @param {number|null} referenceGroupIndex - The index of the reference group (for position context). If null, positions relative to root.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function addPanelToNewGroup(dockViewId, panelId, panelTitle, position, referenceGroupIndex) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        // Check if panel already exists
        const existingPanel = findPanelByTitle(dockview, panelTitle);
        if (existingPanel) {
            console.log(`[DockViewInterop] Panel '${panelTitle}' already exists.`);
            return true;
        }
        
        // Get reference group if specified
        let referenceGroup = null;
        if (referenceGroupIndex !== null && referenceGroupIndex !== undefined) {
            referenceGroup = getGroupByIndex(dockview, referenceGroupIndex);
        }
        
        // Build the position object for dockview
        // DockView uses: { direction: 'above'|'below'|'left'|'right'|'within', referenceGroup?, referencePanel? }
        const directionMap = {
            'top': 'above',
            'bottom': 'below',
            'left': 'left',
            'right': 'right',
            'within': 'within'
        };
        
        const positionObj = {
            direction: directionMap[position] || 'above'
        };
        
        if (referenceGroup) {
            positionObj.referenceGroup = referenceGroup;
        }
        
        // Add the panel - DockView will create a new group for it
        console.log(`[DockViewInterop] Adding panel '${panelTitle}' to new group at position '${position}'.`);
        
        dockview.addPanel({
            id: panelId,
            title: panelTitle,
            component: 'default', // BootstrapBlazor DockViewV2 uses 'default' component
            position: positionObj
        });
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error adding panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Removes a panel by its title.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to remove.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function removePanel(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelByTitle(dockview, panelTitle);
        if (!panel) {
            console.log(`[DockViewInterop] Panel '${panelTitle}' not found, nothing to remove.`);
            return true;
        }
        
        // Remove the panel
        dockview.removePanel(panel);
        console.log(`[DockViewInterop] Removed panel '${panelTitle}'.`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error removing panel '${panelTitle}':`, error);
        return false;
    }
}
