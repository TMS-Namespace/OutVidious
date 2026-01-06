/**
 * DockView Interop - Drawer Operations
 * Handles drawer-specific operations like show, hide, width adjustment, and expansion.
 */

import { getDockview, findPanelByTitle, findPanelByKey, findGroupByPanelTitle, findGroupByPanelKey } from './dockview-interop-core.js';

/**
 * Shows a drawer panel (makes it slide in).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to show.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showDrawer(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelTitle(dockview, panelTitle);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not a drawer.`);
            return false;
        }
        
        // Find the drawer button and click it, or directly manipulate the drawer visibility
        const dockviewEl = dockview.element;
        const drawerContainer = group.element?.parentElement;
        
        if (drawerContainer) {
            drawerContainer.classList.add('active');
            const contentEl = group.activePanel?.view?.content?.element?.parentElement;
            if (contentEl) {
                contentEl.classList.add('active');
            }
            return true;
        }
        
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing drawer '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Hides a drawer panel (makes it slide out).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel to hide.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function hideDrawer(dockViewId, panelTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelTitle(dockview, panelTitle);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' not found.`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not a drawer.`);
            return false;
        }
        
        // Remove the active class to hide the drawer
        const drawerContainer = group.element?.parentElement;
        
        if (drawerContainer) {
            drawerContainer.classList.remove('active');
            const contentEl = group.activePanel?.view?.content?.element?.parentElement;
            if (contentEl) {
                contentEl.classList.remove('active');
            }
            return true;
        }
        
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error hiding drawer '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Hides a drawer - first collapses it properly (so dockview knows it's collapsed),
 * then optionally hides just the sidebar tab button.
 * This approach works WITH dockview instead of against it.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel whose drawer to hide.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function hideDrawerTab(dockViewId, panelTitle) {
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
        
        // Check if it's a drawer
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelTitle}' is not a drawer.`);
            return false;
        }
        
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        // 1. Find the sidebar button first
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
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Could not find sidebar button for drawer '${panelTitle}'.`);
            return false;
        }
        
        // 2. Check if drawer is currently expanded by checking button's active state
        const isActive = matchedBtn.classList.contains('active') || matchedBtn.getAttribute('aria-pressed') === 'true';
        console.log(`[DockViewInterop] hideDrawerTab: button isActive=${isActive}`);
        
        if (isActive) {
            console.log(`[DockViewInterop] Collapsing drawer '${panelTitle}' (was expanded).`);
            matchedBtn.click();
            // Wait a bit for the collapse animation/state change
            await new Promise(resolve => setTimeout(resolve, 150));
        }
        
        // 3. Now hide just the sidebar button with CSS (drawer is now properly collapsed)
        matchedBtn.style.display = 'none';
        matchedBtn.dataset.hiddenByFrontTube = 'true';
        matchedBtn.dataset.hiddenPanelTitle = panelTitle;
        console.log(`[DockViewInterop] Hid sidebar button for '${panelTitle}'.`);
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error hiding drawer for panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Shows a drawer that was previously hidden - both the sidebar tab button and the drawer panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel whose drawer to show.
 * @param {number} [widthPx] - Optional width in pixels to set for the drawer.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showDrawerTab(dockViewId, panelTitle, widthPx) {
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
        
        let shownCount = 0;
        
        // 1. Show the drawer panel container
        const drawerContainer = group.element?.parentElement;
        if (drawerContainer && drawerContainer.dataset.hiddenByFrontTube === 'true') {
            drawerContainer.style.display = '';
            delete drawerContainer.dataset.hiddenByFrontTube;
            delete drawerContainer.dataset.hiddenPanelTitle;
            shownCount++;
            console.log(`[DockViewInterop] Showed drawer panel container for '${panelTitle}'.`);
        }
        
        // 2. Show the sidebar aside-button
        const dockviewEl = document.getElementById(dockViewId);
        if (dockviewEl) {
            const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
            for (const btn of asideButtons) {
                // First try by data attribute (if previously hidden by us)
                if (btn.dataset.hiddenPanelTitle === panelTitle && btn.dataset.hiddenByFrontTube === 'true') {
                    btn.style.display = '';
                    delete btn.dataset.hiddenByFrontTube;
                    delete btn.dataset.hiddenPanelTitle;
                    // Mark as just shown so activatePanel knows to click it
                    btn.dataset.justShown = 'true';
                    shownCount++;
                    console.log(`[DockViewInterop] Showed sidebar aside-button for '${panelTitle}' (by hidden data attribute).`);
                }
                // Also try by data-panel-title for static title groups
                else if (btn.dataset.panelTitle === panelTitle && btn.style.display === 'none') {
                    btn.style.display = '';
                    // Mark as just shown so activatePanel knows to click it
                    btn.dataset.justShown = 'true';
                    shownCount++;
                    console.log(`[DockViewInterop] Showed sidebar aside-button for '${panelTitle}' (by static title data attribute).`);
                }
                // Also try by text content match (in case it wasn't hidden by us)
                else if (btn.style.display === 'none') {
                    const btnText = btn.textContent?.trim();
                    const spanText = btn.querySelector('span')?.textContent?.trim();
                    const isMatch = btnText === panelTitle || spanText === panelTitle || btnText?.includes(panelTitle);
                    if (isMatch) {
                        btn.style.display = '';
                        // Mark as just shown so activatePanel knows to click it
                        btn.dataset.justShown = 'true';
                        shownCount++;
                        console.log(`[DockViewInterop] Showed sidebar aside-button for '${panelTitle}' (by text match).`);
                    }
                }
            }
            
            // Also check for any other hidden elements with this panel title
            const hiddenElements = dockviewEl.querySelectorAll('[data-hidden-panel-title="' + panelTitle + '"]');
            for (const el of hiddenElements) {
                if (el.dataset.hiddenByFrontTube === 'true') {
                    el.style.display = '';
                    delete el.dataset.hiddenByFrontTube;
                    delete el.dataset.hiddenPanelTitle;
                    shownCount++;
                    console.log(`[DockViewInterop] Showed hidden element for '${panelTitle}':`, el.className);
                }
            }
        }
        
        // 3. Set drawer width if specified
        if (widthPx && widthPx > 0) {
            await setDrawerWidth(dockViewId, panelTitle, widthPx);
        }
        
        console.log(`[DockViewInterop] Showed ${shownCount} elements for drawer '${panelTitle}'.`);
        return shownCount > 0 || drawerContainer !== null;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing drawer for panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Shows a drawer that was previously hidden, sets its width, and expands it.
 * This function makes the sidebar button visible, sets the drawer width,
 * and clicks the button to expand the drawer.
 * Works with the new hideDrawerTab that properly collapses drawers.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel whose drawer to show and expand.
 * @param {number} [widthPx] - Optional width in pixels to set for the drawer.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showAndExpandDrawer(dockViewId, panelTitle, widthPx) {
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
        
        // Log current group state for debugging
        const params = group.getParams?.() || {};
        const locationType = group.model?.location?.type;
        const isAlreadyExpanded = group.api?.isVisible === true;
        console.log(`[DockViewInterop] Panel '${panelTitle}' group state: floatType='${params.floatType}', locationType='${locationType}', isVisible=${isAlreadyExpanded}`);
        
        // Find the sidebar button for this drawer
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
        console.log(`[DockViewInterop] Found ${asideButtons.length} aside buttons.`);
        
        let matchedBtn = null;
        for (const btn of asideButtons) {
            const btnText = btn.textContent?.trim();
            const spanText = btn.querySelector('span')?.textContent?.trim();
            // Also check data-panel-title for static title groups and data-hidden-panel-title for hidden buttons
            const dataPanelTitle = btn.dataset.panelTitle;
            const hiddenPanelTitle = btn.dataset.hiddenPanelTitle;
            const isHidden = btn.style.display === 'none';
            console.log(`[DockViewInterop]   Button: text='${btnText}', spanText='${spanText}', dataPanelTitle='${dataPanelTitle}', hiddenPanelTitle='${hiddenPanelTitle}', hidden=${isHidden}`);
            
            const isMatch = btnText === panelTitle || spanText === panelTitle || btnText?.includes(panelTitle) || dataPanelTitle === panelTitle || hiddenPanelTitle === panelTitle;
            if (isMatch) {
                matchedBtn = btn;
                // Don't break - log all buttons for debugging
            }
        }
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Could not find sidebar button for drawer '${panelTitle}'.`);
            return false;
        }
        
        // Make the button visible if it was hidden
        let wasButtonHidden = false;
        if (matchedBtn.style.display === 'none') {
            matchedBtn.style.display = '';
            delete matchedBtn.dataset.hiddenByFrontTube;
            delete matchedBtn.dataset.hiddenPanelTitle;
            wasButtonHidden = true;
            console.log(`[DockViewInterop] Made sidebar button visible for '${panelTitle}'.`);
        }
        
        // Set drawer width if specified
        if (widthPx && widthPx > 0) {
            await setDrawerWidth(dockViewId, panelTitle, widthPx);
        }
        
        // If drawer is already expanded, nothing more to do
        if (isAlreadyExpanded) {
            console.log(`[DockViewInterop] Drawer '${panelTitle}' is already expanded. Done.`);
            return true;
        }
        
        // Drawer is collapsed - click button to expand
        // Need to wait for the DOM to fully update before the click will work
        if (wasButtonHidden) {
            // Wait for layout to complete
            await new Promise(resolve => requestAnimationFrame(() => {
                requestAnimationFrame(() => resolve());
            }));
            // Additional delay for BootstrapBlazor event handlers to be attached
            await new Promise(resolve => setTimeout(resolve, 200));
        }
        
        console.log(`[DockViewInterop] Clicking sidebar button to expand drawer for '${panelTitle}' (using setTimeout approach).`);
        
        // Use setTimeout to let BootstrapBlazor's event handlers fully initialize
        // This approach works better than immediate synthetic clicks
        await new Promise(resolve => {
            setTimeout(() => {
                try {
                    // Focus and click with a slight delay between
                    if (typeof matchedBtn.focus === 'function') {
                        matchedBtn.focus();
                    }
                    
                    // Use native click() method - BootstrapBlazor binds to this
                    matchedBtn.click();
                    console.log(`[DockViewInterop] Native click() called on sidebar button for '${panelTitle}'.`);
                } catch (e) {
                    console.error(`[DockViewInterop] Error during click:`, e);
                }
                resolve();
            }, wasButtonHidden ? 100 : 0);
        });
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing and expanding drawer for panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Sets the width of a drawer panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of the panel whose drawer width to set.
 * @param {number} widthPx - The width in pixels.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setDrawerWidth(dockViewId, panelTitle, widthPx) {
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
        
        // Try to set width via the group API if available
        if (group.api && typeof group.api.setSize === 'function') {
            group.api.setSize({ width: widthPx });
            console.log(`[DockViewInterop] Set drawer width for '${panelTitle}' to ${widthPx}px via API.`);
            return true;
        }
        
        // Fallback: set width directly on the group element 
        if (group.element) {
            // Find the drawer container (floating container with drawer class)
            let drawerEl = group.element.closest('.bb-dockview-drawer-right');
            if (!drawerEl) {
                drawerEl = group.element.closest('.dv-resize-container');
            }
            if (!drawerEl) {
                drawerEl = group.element.parentElement;
            }
            
            if (drawerEl) {
                drawerEl.style.width = `${widthPx}px`;
                drawerEl.style.minWidth = `${widthPx}px`;
                console.log(`[DockViewInterop] Set drawer width for '${panelTitle}' to ${widthPx}px via element style.`);
                return true;
            }
        }
        
        // Second fallback: find the drawer wrapper element by searching
        const dockviewEl = document.getElementById(dockViewId);
        if (dockviewEl) {
            // Find all drawer containers on the right
            const drawerWrappers = dockviewEl.querySelectorAll('.bb-dockview-drawer-right, .dv-resize-container');
            for (const wrapper of drawerWrappers) {
                // Check if this wrapper contains our panel by looking for title text
                const tabContent = wrapper.querySelector('.dv-default-tab-content');
                if (tabContent && (tabContent.textContent?.includes(panelTitle) || 
                    tabContent.querySelector('span')?.textContent?.trim() === panelTitle)) {
                    wrapper.style.width = `${widthPx}px`;
                    wrapper.style.minWidth = `${widthPx}px`;
                    console.log(`[DockViewInterop] Set drawer wrapper width for '${panelTitle}' to ${widthPx}px.`);
                    return true;
                }
            }
        }
        
        console.warn(`[DockViewInterop] Could not find element to set width for drawer '${panelTitle}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting drawer width for '${panelTitle}':`, error);
        return false;
    }
}

// ============================================================================
// Key-based functions (preferred over title-based)
// ============================================================================

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
    
    for (const btn of asideButtons) {
        // Check by data-panel-key attribute (most reliable)
        if (btn.dataset.panelKey === panelKey) {
            return btn;
        }
        
        // Check by data-hidden-panel-key for hidden buttons
        if (btn.dataset.hiddenPanelKey === panelKey) {
            return btn;
        }
        
        // Fallback: check by panel title if available
        if (panel) {
            const btnText = btn.textContent?.trim();
            const spanText = btn.querySelector('span')?.textContent?.trim();
            const dataPanelTitle = btn.dataset.panelTitle;
            const hiddenPanelTitle = btn.dataset.hiddenPanelTitle;
            const panelTitle = panel.title;
            
            const isMatch = btnText === panelTitle || 
                            spanText === panelTitle || 
                            btnText?.includes(panelTitle) || 
                            dataPanelTitle === panelTitle || 
                            hiddenPanelTitle === panelTitle;
            
            if (isMatch) {
                // Tag this button with the key for future lookups
                btn.dataset.panelKey = panelKey;
                return btn;
            }
        }
    }
    
    return null;
}

/**
 * Hides a drawer tab by panel key.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelKey - The unique key of the panel whose tab to hide.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function hideDrawerTabByKey(dockViewId, panelKey) {
    console.log(`[DockViewInterop] hideDrawerTabByKey called for key '${panelKey}'`);
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) {
            console.warn(`[DockViewInterop] hideDrawerTabByKey: dockview not found`);
            return false;
        }
        
        // Log all available panels for debugging
        console.log(`[DockViewInterop] hideDrawerTabByKey: Available panels:`, 
            dockview.panels.map(p => ({ 
                id: p.id, 
                title: p.title, 
                paramsKey: p.params?.key || p.params?.Key,
                groupFloatType: p.group?.getParams?.()?.floatType
            })));
        
        const panel = findPanelByKey(dockview, panelKey);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel with key '${panelKey}' not found.`);
            return false;
        }
        
        console.log(`[DockViewInterop] hideDrawerTabByKey: Found panel '${panel.title}' with id '${panel.id}'`);
        
        const group = panel.group;
        if (!group) {
            console.warn(`[DockViewInterop] Panel with key '${panelKey}' has no group.`);
            return false;
        }
        
        // Check if it's a drawer
        const params = group.getParams?.() || {};
        console.log(`[DockViewInterop] hideDrawerTabByKey: Group params:`, params);
        
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel with key '${panelKey}' is not a drawer (floatType='${params.floatType}'). Skipping check for now...`);
            // Don't return early - try to find and hide the button anyway
        }
        
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        // Find the sidebar button
        const matchedBtn = findSidebarButtonByKey(dockviewEl, panelKey, panel);
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Could not find sidebar button for panel key '${panelKey}'. Looking for buttons...`);
            const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
            console.log(`[DockViewInterop] Found ${asideButtons.length} aside buttons:`, 
                Array.from(asideButtons).map(btn => ({
                    text: btn.textContent?.trim(),
                    display: btn.style.display,
                    panelKey: btn.dataset.panelKey,
                    panelTitle: btn.dataset.panelTitle
                })));
            return false;
        }
        
        // Tag this button with the key for future lookups
        matchedBtn.dataset.panelKey = panelKey;
        
        // Check if drawer is currently expanded by checking button's active state
        const isActive = matchedBtn.classList.contains('active') || matchedBtn.getAttribute('aria-pressed') === 'true';
        console.log(`[DockViewInterop] hideDrawerTabByKey: button isActive=${isActive}`);
        
        if (isActive) {
            console.log(`[DockViewInterop] Collapsing drawer with key '${panelKey}' (was expanded).`);
            matchedBtn.click();
            // Wait for the collapse animation to complete
            await new Promise(resolve => setTimeout(resolve, 150));
            
            // Verify it collapsed
            const stillActive = matchedBtn.classList.contains('active') || matchedBtn.getAttribute('aria-pressed') === 'true';
            console.log(`[DockViewInterop] After collapse click, stillActive=${stillActive}`);
        }
        
        // Hide the sidebar button
        matchedBtn.style.display = 'none';
        matchedBtn.dataset.hiddenByFrontTube = 'true';
        matchedBtn.dataset.hiddenPanelKey = panelKey;
        matchedBtn.dataset.hiddenPanelTitle = panel.title; // Also store title for legacy compatibility
        console.log(`[DockViewInterop] Hid sidebar button for panel key '${panelKey}'.`);
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error hiding drawer for panel key '${panelKey}':`, error);
        return false;
    }
}

/**
 * Shows a drawer tab by panel key.
 * If the button is already visible, this is a no-op (returns true).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelKey - The unique key of the panel whose tab to show.
 * @param {number} [widthPx] - Optional width in pixels.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showDrawerTabByKey(dockViewId, panelKey, widthPx) {
    console.log(`[DockViewInterop] showDrawerTabByKey called for key '${panelKey}'`);
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
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
        
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) return false;
        
        // Find the sidebar button using our helper
        let matchedBtn = findSidebarButtonByKey(dockviewEl, panelKey, panel);
        
        if (!matchedBtn) {
            // No sidebar button found - the group may have been closed via X button
            // The panel is in a "zombie" state - exists in dockview.panels but disconnected from layout
            // Remove it so Blazor can recreate it properly
            console.warn(`[DockViewInterop] Could not find sidebar button for panel key '${panelKey}'. Panel is orphaned, removing...`);
            
            try {
                dockview.removePanel(panel);
                console.log(`[DockViewInterop] Removed orphaned panel '${panelKey}'. Blazor should recreate it.`);
            } catch (removeError) {
                console.warn(`[DockViewInterop] Failed to remove orphaned panel: ${removeError.message}`);
            }
            
            return false; // Signal failure so caller knows to retry/recreate
        }
        
        // Check if button is currently hidden
        const isHidden = matchedBtn.style.display === 'none' || matchedBtn.dataset.hiddenByFrontTube === 'true';
        
        if (isHidden) {
            // Show the button
            matchedBtn.style.display = '';
            delete matchedBtn.dataset.hiddenByFrontTube;
            delete matchedBtn.dataset.hiddenPanelKey;
            delete matchedBtn.dataset.hiddenPanelTitle;
            matchedBtn.dataset.panelKey = panelKey;
            console.log(`[DockViewInterop] Showed hidden sidebar button for panel key '${panelKey}'.`);
        } else {
            console.log(`[DockViewInterop] Sidebar button for panel key '${panelKey}' is already visible.`);
        }
        
        // Ensure the panelKey is set on the button for future lookups
        matchedBtn.dataset.panelKey = panelKey;
        
        // Set drawer width if specified
        if (widthPx && widthPx > 0) {
            await setDrawerWidthByKey(dockViewId, panelKey, widthPx);
        }
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing drawer for panel key '${panelKey}':`, error);
        return false;
    }
}

/**
 * Sets the width of a drawer panel by key.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelKey - The unique key of the panel.
 * @param {number} widthPx - The width in pixels.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setDrawerWidthByKey(dockViewId, panelKey, widthPx) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
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
        
        // Try to set width via the group API
        if (group.api && typeof group.api.setSize === 'function') {
            group.api.setSize({ width: widthPx });
            console.log(`[DockViewInterop] Set drawer width for key '${panelKey}' to ${widthPx}px via API.`);
            return true;
        }
        
        // Fallback: set width directly on the group element
        if (group.element) {
            let drawerEl = group.element.closest('.bb-dockview-drawer-right');
            if (!drawerEl) {
                drawerEl = group.element.closest('.dv-resize-container');
            }
            if (!drawerEl) {
                drawerEl = group.element.parentElement;
            }
            
            if (drawerEl) {
                drawerEl.style.width = `${widthPx}px`;
                drawerEl.style.minWidth = `${widthPx}px`;
                console.log(`[DockViewInterop] Set drawer width for key '${panelKey}' to ${widthPx}px via element style.`);
                return true;
            }
        }
        
        console.warn(`[DockViewInterop] Could not find element to set width for panel key '${panelKey}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting drawer width for panel key '${panelKey}':`, error);
        return false;
    }
}
