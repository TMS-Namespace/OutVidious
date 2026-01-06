/**
 * DockView Interop - Panel Management
 * Handles panel state, activation, and lifecycle operations.
 */

import { getDockview, findPanelByTitle, findPanelByKey, findGroupByPanelTitle, getGroupByIndex } from './dockview-interop-core.js';

/**
 * Finds a sidebar button by panel key using data-panel-key attribute.
 * Falls back to checking the panel's title if no key-based match is found.
 * @param {HTMLElement} dockviewEl - The dockview container element.
 * @param {string} panelKey - The panel key to search for.
 * @param {object} panel - The panel object (for title fallback).
 * @returns {HTMLElement|null} The matched button or null.
 */
function findSidebarButtonByKey(dockviewEl, panelKey, panel) {
    const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
    console.log(`[DockViewInterop] findSidebarButtonByKey: Looking for key '${panelKey}', found ${asideButtons.length} buttons`);
    
    for (const btn of asideButtons) {
        // Check by data-panel-key attribute (most reliable)
        if (btn.dataset.panelKey === panelKey) {
            console.log(`[DockViewInterop] findSidebarButtonByKey: Found by panelKey attr`);
            return btn;
        }
        
        // Check by data-hidden-panel-key for hidden buttons
        if (btn.dataset.hiddenPanelKey === panelKey) {
            console.log(`[DockViewInterop] findSidebarButtonByKey: Found by hiddenPanelKey attr`);
            return btn;
        }
    }
    
    // Second pass: check by panel title if available
    if (panel) {
        const panelTitle = panel.title;
        console.log(`[DockViewInterop] findSidebarButtonByKey: Trying title fallback for '${panelTitle}'`);
        
        for (const btn of asideButtons) {
            const btnText = btn.textContent?.trim();
            const spanText = btn.querySelector('span')?.textContent?.trim();
            const dataPanelTitle = btn.dataset.panelTitle;
            const hiddenPanelTitle = btn.dataset.hiddenPanelTitle;
            const staticTitle = btn.dataset.staticTitle;
            
            const isMatch = btnText === panelTitle || 
                            spanText === panelTitle || 
                            btnText?.includes(panelTitle) || 
                            dataPanelTitle === panelTitle || 
                            hiddenPanelTitle === panelTitle;
            
            if (isMatch) {
                console.log(`[DockViewInterop] findSidebarButtonByKey: Found by title match, btnText='${btnText}'`);
                // Tag this button with the key for future lookups
                btn.dataset.panelKey = panelKey;
                return btn;
            }
        }
    }
    
    console.log(`[DockViewInterop] findSidebarButtonByKey: No match found. Button details:`, 
        Array.from(asideButtons).map(btn => ({
            text: btn.textContent?.trim()?.substring(0, 30),
            panelKey: btn.dataset.panelKey,
            panelTitle: btn.dataset.panelTitle,
            hiddenPanelKey: btn.dataset.hiddenPanelKey,
            display: btn.style.display
        })));
    
    return null;
}

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
            // Find and click the sidebar button to expand the drawer
            const dockviewEl = document.getElementById(dockViewId);
            
            if (dockviewEl) {
                const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
                let matchedBtn = null;
                
                for (const btn of asideButtons) {
                    const btnText = btn.textContent?.trim();
                    const spanText = btn.querySelector('span')?.textContent?.trim();
                    // Also check data-panel-title for static title groups and data-hidden-panel-title for hidden buttons
                    const dataPanelTitle = btn.dataset.panelTitle;
                    const hiddenPanelTitle = btn.dataset.hiddenPanelTitle;
                    const isMatch = btnText === panelTitle || spanText === panelTitle || btnText?.includes(panelTitle) || dataPanelTitle === panelTitle || hiddenPanelTitle === panelTitle;
                    
                    if (isMatch) {
                        matchedBtn = btn;
                        break;
                    }
                }
                
                if (matchedBtn) {
                    // Check if button is visible (not hidden by us)
                    const isButtonVisible = matchedBtn.style.display !== 'none';
                    console.log(`[DockViewInterop] activatePanel: button found, isButtonVisible=${isButtonVisible}`);
                    
                    // Ensure button is visible first
                    if (!isButtonVisible) {
                        matchedBtn.style.display = '';
                        delete matchedBtn.dataset.hiddenByFrontTube;
                        delete matchedBtn.dataset.hiddenPanelTitle;
                        console.log(`[DockViewInterop] Made sidebar button visible for '${panelTitle}'.`);
                        await new Promise(resolve => setTimeout(resolve, 50));
                    }
                    
                    // Check drawer expansion state using reliable heuristics
                    const isActive = matchedBtn.classList.contains('active');
                    
                    // Check if any other drawer button is currently active
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
                        console.log(`[DockViewInterop] activatePanel: Clicking button to expand drawer for '${panelTitle}'`);
                        matchedBtn.click();
                        delete matchedBtn.dataset.justShown;
                        console.log(`[DockViewInterop] ========== activatePanel END (clicked) ==========`);
                        return true;
                    } else {
                        console.log(`[DockViewInterop] activatePanel: Drawer already visible, skipping click`);
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

// ============================================================================
// Key-based functions (preferred over title-based)
// ============================================================================

/**
 * Checks if a drawer is currently expanded by examining the DOM.
 * @param {object} group - The dockview group object.
 * @returns {boolean} True if the drawer is expanded.
 */
function isDrawerExpanded(group) {
    // The drawer content is in a .bb-dockview-drawer-right container
    // When expanded, it has a significant width and is visible; when collapsed, it's hidden or has no width
    const groupElement = group.element;
    console.log(`[DockViewInterop] isDrawerExpanded: groupElement exists=${!!groupElement}`);
    
    if (!groupElement) {
        console.log(`[DockViewInterop] isDrawerExpanded: No group element, returning false`);
        return false;
    }
    
    // Check the group's parent container for the drawer wrapper
    const drawerWrapper = groupElement.closest('.bb-dockview-drawer-right, .bb-dockview-drawer-left');
    console.log(`[DockViewInterop] isDrawerExpanded: drawerWrapper exists=${!!drawerWrapper}`);
    
    if (drawerWrapper) {
        const width = drawerWrapper.offsetWidth;
        const style = window.getComputedStyle(drawerWrapper);
        const display = style.display;
        const visibility = style.visibility;
        console.log(`[DockViewInterop] isDrawerExpanded: drawerWrapper width=${width}, display=${display}, visibility=${visibility}`);
        
        // Check if actually visible
        if (display === 'none' || visibility === 'hidden') {
            return false;
        }
        return width > 50;
    }
    
    // Alternative: check the resize container
    const resizeContainer = groupElement.closest('.dv-resize-container');
    console.log(`[DockViewInterop] isDrawerExpanded: resizeContainer exists=${!!resizeContainer}`);
    
    if (resizeContainer) {
        const width = resizeContainer.offsetWidth;
        const style = window.getComputedStyle(resizeContainer);
        const display = style.display;
        const visibility = style.visibility;
        console.log(`[DockViewInterop] isDrawerExpanded: resizeContainer width=${width}, display=${display}, visibility=${visibility}`);
        
        // Check if actually visible
        if (display === 'none' || visibility === 'hidden') {
            return false;
        }
        return width > 50;
    }
    
    console.log(`[DockViewInterop] isDrawerExpanded: No container found, returning false`);
    return false;
}

/**
 * Activates (focuses) a panel by key and ensures its group is visible.
 * If the panel is in a drawer, the drawer will be expanded by clicking the sidebar button.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelKey - The unique key of the panel to activate.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function activatePanelByKey(dockViewId, panelKey) {
    console.log(`[DockViewInterop] ========== activatePanelByKey START for key '${panelKey}' ==========`);
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) {
            console.error(`[DockViewInterop] activatePanelByKey: dockview not found for '${dockViewId}'`);
            return false;
        }
        
        const panel = findPanelByKey(dockview, panelKey);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel with key '${panelKey}' not found.`);
            return false;
        }
        
        const group = panel.group;
        if (!group) {
            console.warn(`[DockViewInterop] Panel with key '${panelKey}' has no group.`);
            return false;
        }
        
        // Check if this is a drawer
        const params = group.getParams?.() || {};
        console.log(`[DockViewInterop] activatePanelByKey: group params floatType='${params.floatType}'`);
        
        if (params.floatType === 'drawer') {
            const dockviewEl = document.getElementById(dockViewId);
            
            if (dockviewEl) {
                // Find sidebar button by key
                const matchedBtn = findSidebarButtonByKey(dockviewEl, panelKey, panel);
                
                if (matchedBtn) {
                    const isButtonVisible = matchedBtn.style.display !== 'none';
                    console.log(`[DockViewInterop] activatePanelByKey: button found, isButtonVisible=${isButtonVisible}`);
                    
                    // Ensure button is visible first
                    if (!isButtonVisible) {
                        matchedBtn.style.display = '';
                        delete matchedBtn.dataset.hiddenByFrontTube;
                        delete matchedBtn.dataset.hiddenPanelKey;
                        delete matchedBtn.dataset.hiddenPanelTitle;
                        console.log(`[DockViewInterop] Made sidebar button visible for key '${panelKey}'.`);
                        await new Promise(resolve => setTimeout(resolve, 50));
                    }
                    
                    // Check drawer expansion state
                    // The button's 'active' class indicates if the drawer was opened
                    // BUT: when user closes drawer by clicking outside, the button keeps 'active' class incorrectly
                    // SO: we need to check if ANY button is active and if the drawer content is actually visible
                    const isActive = matchedBtn.classList.contains('active');
                    
                    // Check if any other drawer button is currently active
                    const allButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
                    let anyOtherButtonActive = false;
                    for (const btn of allButtons) {
                        if (btn !== matchedBtn && btn.classList.contains('active') && btn.style.display !== 'none') {
                            anyOtherButtonActive = true;
                            break;
                        }
                    }
                    
                    // Check if there's a visible drawer content in DOM with actual width
                    // The drawer wrapper gains width when expanded
                    const drawerWrappers = dockviewEl.querySelectorAll('.bb-dockview-drawer-right, .bb-dockview-drawer-left');
                    let anyDrawerHasWidth = false;
                    for (const wrapper of drawerWrappers) {
                        // Check if the wrapper has substantial width (indicating an open drawer)
                        if (wrapper.offsetWidth > 100) {
                            anyDrawerHasWidth = true;
                            break;
                        }
                    }
                    
                    console.log(`[DockViewInterop] activatePanelByKey: isActive=${isActive}, anyOtherButtonActive=${anyOtherButtonActive}, anyDrawerHasWidth=${anyDrawerHasWidth}`);
                    
                    // Determine if THIS drawer is actually expanded:
                    // - If this button is NOT active, drawer is definitely closed -> click to open
                    // - If this button IS active AND there's a drawer with width, it might be this one -> check further
                    // - If this button IS active BUT no drawer has width, drawer was closed externally -> click to open
                    let isExpanded = false;
                    if (isActive && anyDrawerHasWidth && !anyOtherButtonActive) {
                        // This button is active, a drawer is visible, and no other button is active
                        // So this drawer is likely the one that's open
                        isExpanded = true;
                    }
                    
                    console.log(`[DockViewInterop] activatePanelByKey: isExpanded=${isExpanded}`);
                    
                    if (!isExpanded) {
                        console.log(`[DockViewInterop] activatePanelByKey: Clicking button to expand drawer for key '${panelKey}'`);
                        matchedBtn.click();
                        console.log(`[DockViewInterop] ========== activatePanelByKey END (clicked) ==========`);
                        return true;
                    } else {
                        console.log(`[DockViewInterop] activatePanelByKey: Drawer already expanded, skipping click`);
                    }
                } else {
                    // No sidebar button found - the group may have been closed via X button
                    // The panel is in a "zombie" state - exists in dockview.panels but disconnected from layout
                    // Remove it so Blazor can recreate it properly
                    console.warn(`[DockViewInterop] Could not find sidebar button for drawer key '${panelKey}'. Panel is orphaned, removing...`);
                    
                    try {
                        dockview.removePanel(panel);
                        console.log(`[DockViewInterop] Removed orphaned panel '${panelKey}'. Blazor should recreate it.`);
                    } catch (removeError) {
                        console.warn(`[DockViewInterop] Failed to remove orphaned panel: ${removeError.message}`);
                    }
                    
                    console.log(`[DockViewInterop] ========== activatePanelByKey END (orphan removed) ==========`);
                    return false; // Signal failure so caller knows to retry/recreate
                }
            }
            
            console.log(`[DockViewInterop] ========== activatePanelByKey END (drawer) ==========`);
            return true;
        }
        
        // Activate the panel (for non-drawer panels)
        panel.api?.setActive();
        
        console.log(`[DockViewInterop] Activated panel with key '${panelKey}'.`);
        console.log(`[DockViewInterop] ========== activatePanelByKey END ==========`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error activating panel with key '${panelKey}':`, error);
        return false;
    }
}
