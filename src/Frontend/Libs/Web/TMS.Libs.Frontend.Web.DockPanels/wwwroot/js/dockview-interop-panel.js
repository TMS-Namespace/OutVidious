/**
 * DockView Interop - Panel Management
 * Handles panel state, activation, and lifecycle operations.
 */

import { getDockview, findPanelById, findGroupByPanelId, getGroupById } from './dockview-interop-core.js';

/**
 * Finds a sidebar button by group id.
 * @param {HTMLElement} dockviewEl - The dockview container element.
 * @param {string} groupId - The group id to search for.
 * @returns {HTMLElement|null} The matched button or null.
 */
function findSidebarButtonByGroupId(dockviewEl, groupId) {
    if (!dockviewEl) return null;
    return dockviewEl.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
}

/**
 * Collapses a floating panel (shows only the header).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to collapse.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function collapsePanel(dockViewId, panelId) {
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
            console.warn(`[DockViewInterop] Panel '${panelId}' is not floating (collapse only works for floating).`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (params.packup?.isPackup) {
            console.warn(`[DockViewInterop] Panel '${panelId}' is already collapsed.`);
            return true;
        }
        
        // Click the down/collapse button
        const actionContainer = group.header?.rightActionsContainer;
        const downBtn = actionContainer?.querySelector('.bb-dockview-control-icon-down');
        
        if (downBtn) {
            downBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Collapse button not found for panel '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error collapsing panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Expands a collapsed floating panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to expand.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function expandPanel(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (!params.packup?.isPackup) {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not collapsed.`);
            return true;
        }
        
        // Click the up/expand button
        const actionContainer = group.header?.rightActionsContainer;
        const upBtn = actionContainer?.querySelector('.bb-dockview-control-icon-up, .bb-dockview-control-icon-down');
        
        if (upBtn) {
            upBtn.click();
            return true;
        }
        
        console.warn(`[DockViewInterop] Expand button not found for panel '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error expanding panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Activates (focuses) a panel and ensures its group is visible.
 * If the panel is in a drawer, the drawer will be expanded by clicking the sidebar button (if not already expanded).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to activate.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function activatePanel(dockViewId, panelId) {
    console.log(`[DockViewInterop] ========== activatePanel START for '${panelId}' ==========`);
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) {
            console.error(`[DockViewInterop] activatePanel: dockview not found for '${dockViewId}'`);
            return false;
        }
        
        const panel = findPanelById(dockview, panelId);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const group = panel.group;
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' has no group.`);
            return false;
        }
        
        // Check if this is a drawer - if so, expand it via button click
        const params = group.getParams?.() || {};
        
        const isDrawer = params.floatType === 'drawer';
        if (isDrawer) {
            // Find and click the sidebar button to expand the drawer
            const dockviewEl = document.getElementById(dockViewId);
            
            if (dockviewEl) {
                const groupId = `${dockview.id}_${group.id}`;
                const matchedBtn = findSidebarButtonByGroupId(dockviewEl, groupId);
                
                if (matchedBtn) {
                    // Check if button is visible (not hidden by us)
                    const isButtonVisible = matchedBtn.style.display !== 'none';
                    console.log(`[DockViewInterop] activatePanel: button found, isButtonVisible=${isButtonVisible}`);
                    
                    // Ensure button is visible first
                    if (!isButtonVisible) {
                        matchedBtn.style.display = '';
                        delete matchedBtn.dataset.hiddenByFrontTube;
                        console.log(`[DockViewInterop] Made sidebar button visible for '${panelId}'.`);
                        await new Promise(resolve => setTimeout(resolve, 50));
                    }
                    
                    // Check drawer expansion state using reliable heuristics
                    const isActive = matchedBtn.classList.contains('active');
                    
                    // Check if any other drawer button is currently active
                    const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside>.bb-dockview-aside-button');
                    let anyOtherButtonActive = false;
                    for (const btn of asideButtons) {
                        if (btn !== matchedBtn && btn.classList.contains('active') && btn.style.display !== 'none') {
                            anyOtherButtonActive = true;
                            break;
                        }
                    }
                    
                    // Check if there's a visible drawer content in DOM with actual width
                    const drawerWrappers = dockviewEl.querySelectorAll('.bb-dockview-drawer-right, .bb-dockview-drawer-left');
                    let anyDrawerHasWidth = false;
                    for (const wrapper of drawerWrappers) {
                        if (wrapper.offsetWidth > 100) {
                            anyDrawerHasWidth = true;
                            break;
                        }
                    }
                    
                    console.log(`[DockViewInterop] activatePanel: isActive=${isActive}, anyOtherButtonActive=${anyOtherButtonActive}, anyDrawerHasWidth=${anyDrawerHasWidth}`);
                    
                    // Determine if THIS drawer is actually expanded
                    let isExpanded = false;
                    if (isActive && anyDrawerHasWidth && !anyOtherButtonActive) {
                        isExpanded = true;
                    }
                    
                    // Also check justShown flag
                    if (!isExpanded || matchedBtn.dataset.justShown === 'true') {
                        console.log(`[DockViewInterop] activatePanel: Clicking button to expand drawer for '${panelId}'`);
                        matchedBtn.click();
                        delete matchedBtn.dataset.justShown;
                    } else {
                        console.log(`[DockViewInterop] activatePanel: Drawer already visible, skipping click`);
                    }
                } else {
                    console.warn(`[DockViewInterop] Could not find sidebar button for drawer '${panelId}'.`);
                }
            }
        }
        
        panel.api?.setActive();
        
        console.log(`[DockViewInterop] Activated panel '${panelId}'.`);
        console.log(`[DockViewInterop] ========== activatePanel END${isDrawer ? ' (drawer)' : ''} ==========`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error activating panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Sets the active panel by ID without toggling drawer visibility.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to activate.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setActivePanel(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) {
            console.error(`[DockViewInterop] setActivePanel: dockview not found for '${dockViewId}'`);
            return false;
        }
        
        const panel = findPanelById(dockview, panelId);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        panel.api?.setActive();
        console.log(`[DockViewInterop] Set active panel '${panelId}'.`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting active panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Gets the current state of a panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID.
 * @returns {Promise<object|null>} The panel state or null if not found.
 */
export async function getPanelState(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return null;
        
        const panel = findPanelById(dockview, panelId);
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
        console.error(`[DockViewInterop] Error getting panel state for '${panelId}':`, error);
        return null;
    }
}

/**
 * Checks if a panel with the given ID exists.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to check.
 * @returns {Promise<boolean>} True if the panel exists.
 */
export async function panelExists(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelById(dockview, panelId);
        return panel !== null;
    } catch (error) {
        console.error(`[DockViewInterop] Error checking panel '${panelId}':`, error);
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
 * @param {string|null} referenceGroupId - The ID of the reference group (for position context). If null, positions relative to root.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function addPanelToNewGroup(dockViewId, panelId, panelTitle, position, referenceGroupId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        // Check if panel already exists
        const existingPanel = findPanelById(dockview, panelId);
        if (existingPanel) {
            console.log(`[DockViewInterop] Panel '${panelId}' already exists.`);
            return true;
        }
        
        // Get reference group if specified
        let referenceGroup = null;
        if (referenceGroupId) {
            referenceGroup = getGroupById(dockview, referenceGroupId);
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
        console.log(`[DockViewInterop] Adding panel '${panelId}' to new group at position '${position}'.`);
        
        dockview.addPanel({
            id: panelId,
            title: panelTitle,
            component: 'default', // BootstrapBlazor DockViewV2 uses 'default' component
            position: positionObj
        });
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error adding panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Removes a panel by its ID.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to remove.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function removePanel(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelById(dockview, panelId);
        if (!panel) {
            console.log(`[DockViewInterop] Panel '${panelId}' not found, nothing to remove.`);
            return true;
        }
        
        // Remove the panel
        dockview.removePanel(panel);
        console.log(`[DockViewInterop] Removed panel '${panelId}'.`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error removing panel '${panelId}':`, error);
        return false;
    }
}
