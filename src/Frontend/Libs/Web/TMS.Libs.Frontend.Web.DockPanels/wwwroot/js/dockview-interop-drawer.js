/**
 * DockView Interop - Drawer Operations
 * Handles drawer-specific operations like show, hide, width adjustment, and expansion.
 */

import { getDockview, findPanelById, findGroupByPanelId } from './dockview-interop-core.js';

/**
 * Checks if a drawer is fully ready in the DOM (button + container).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID.
 * @returns {Promise<boolean>} True if the drawer elements are ready.
 */
export async function isDrawerReady(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelById(dockview, panelId);
        if (!panel) return false;
        
        const group = panel.group;
        if (!group) return false;
        
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') return false;
        
        const drawerContainer = group.element?.parentElement;
        if (!drawerContainer || !drawerContainer.classList.contains('dv-resize-container-drawer')) {
            return false;
        }
        
        const groupId = `${dockview.id}_${group.id}`;
        const drawerButton = dockview.element?.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
        return !!drawerButton;
    } catch (error) {
        console.error(`[DockViewInterop] Error checking drawer readiness for '${panelId}':`, error);
        return false;
    }
}

/**
 * Shows a drawer panel (makes it slide in).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to show.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showDrawer(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not a drawer.`);
            return false;
        }
        
        // Find the drawer button and mark it active, or directly manipulate the drawer visibility
        const dockviewEl = dockview.element;
        const drawerContainer = group.element?.parentElement;
        const groupId = `${dockview.id}_${group.id}`;
        const drawerButton = dockviewEl?.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
        
        if (drawerContainer) {
            drawerContainer.classList.add('active');
            const contentEl = group.activePanel?.view?.content?.element?.parentElement;
            if (contentEl) {
                contentEl.classList.add('active');
            }
            if (drawerButton) {
                drawerButton.classList.add('active');
            }
            return true;
        }
        
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing drawer '${panelId}':`, error);
        return false;
    }
}

/**
 * Hides a drawer panel (makes it slide out).
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID to hide.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function hideDrawer(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Panel '${panelId}' not found.`);
            return false;
        }
        
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not a drawer.`);
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
        console.error(`[DockViewInterop] Error hiding drawer '${panelId}':`, error);
        return false;
    }
}

/**
 * Hides a drawer - first collapses it properly (so dockview knows it's collapsed),
 * then optionally hides just the sidebar tab button.
 * This approach works WITH dockview instead of against it.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID whose drawer to hide.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function hideDrawerTab(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
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
        
        // Check if it's a drawer
        const params = group.getParams?.() || {};
        if (params.floatType !== 'drawer') {
            console.warn(`[DockViewInterop] Panel '${panelId}' is not a drawer.`);
            return false;
        }
        
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        const groupId = `${dockview.id}_${group.id}`;
        const matchedBtn = dockviewEl.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Could not find sidebar button for drawer '${panelId}'.`);
            return false;
        }
        
        // 2. Check if drawer is currently expanded by checking button's active state
        const isActive = matchedBtn.classList.contains('active') || matchedBtn.getAttribute('aria-pressed') === 'true';
        console.log(`[DockViewInterop] hideDrawerTab: button isActive=${isActive}`);
        
        if (isActive) {
            console.log(`[DockViewInterop] Collapsing drawer '${panelId}' (was expanded).`);
            matchedBtn.click();
            // Wait a bit for the collapse animation/state change
            await new Promise(resolve => setTimeout(resolve, 150));
        }
        
        // 3. Now hide just the sidebar button with CSS (drawer is now properly collapsed)
        matchedBtn.style.display = 'none';
        matchedBtn.dataset.hiddenByFrontTube = 'true';
        console.log(`[DockViewInterop] Hid sidebar button for '${panelId}'.`);
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error hiding drawer for panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Shows a drawer that was previously hidden - both the sidebar tab button and the drawer panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID whose drawer to show.
 * @param {number} [widthPx] - Optional width in pixels to set for the drawer.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showDrawerTab(dockViewId, panelId, widthPx) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
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
        
        let shownCount = 0;
        
        // 1. Show the drawer panel container
        const drawerContainer = group.element?.parentElement;
        if (drawerContainer && drawerContainer.dataset.hiddenByFrontTube === 'true') {
            drawerContainer.style.display = '';
            delete drawerContainer.dataset.hiddenByFrontTube;
            shownCount++;
            console.log(`[DockViewInterop] Showed drawer panel container for '${panelId}'.`);
        }
        
        // 2. Show the sidebar aside-button
        const dockviewEl = document.getElementById(dockViewId);
        if (dockviewEl) {
            const groupId = `${dockview.id}_${group.id}`;
            const btn = dockviewEl.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
            if (btn && btn.dataset.hiddenByFrontTube === 'true') {
                btn.style.display = '';
                delete btn.dataset.hiddenByFrontTube;
                btn.dataset.justShown = 'true';
                shownCount++;
                console.log(`[DockViewInterop] Showed sidebar aside-button for '${panelId}'.`);
            }
        }
        
        // 3. Set drawer width if specified
        if (widthPx && widthPx > 0) {
            await setDrawerWidth(dockViewId, panelId, widthPx);
        }
        
        console.log(`[DockViewInterop] Showed ${shownCount} elements for drawer '${panelId}'.`);
        return shownCount > 0 || drawerContainer !== null;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing drawer for panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Shows a drawer that was previously hidden, sets its width, and expands it.
 * This function makes the sidebar button visible, sets the drawer width,
 * and clicks the button to expand the drawer.
 * Works with the new hideDrawerTab that properly collapses drawers.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID whose drawer to show and expand.
 * @param {number} [widthPx] - Optional width in pixels to set for the drawer.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function showAndExpandDrawer(dockViewId, panelId, widthPx) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
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
        
        // Log current group state for debugging
        const params = group.getParams?.() || {};
        const locationType = group.model?.location?.type;
        const isAlreadyExpanded = group.api?.isVisible === true;
        console.log(`[DockViewInterop] Panel '${panelId}' group state: floatType='${params.floatType}', locationType='${locationType}', isVisible=${isAlreadyExpanded}`);
        
        // Find the sidebar button for this drawer
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        const groupId = `${dockview.id}_${group.id}`;
        const matchedBtn = dockviewEl.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Could not find sidebar button for drawer '${panelId}'.`);
            return false;
        }
        
        // Make the button visible if it was hidden
        let wasButtonHidden = false;
        if (matchedBtn.style.display === 'none') {
            matchedBtn.style.display = '';
            delete matchedBtn.dataset.hiddenByFrontTube;
            wasButtonHidden = true;
            console.log(`[DockViewInterop] Made sidebar button visible for '${panelId}'.`);
        }
        
        // Set drawer width if specified
        if (widthPx && widthPx > 0) {
            await setDrawerWidth(dockViewId, panelId, widthPx);
        }
        
        // If drawer is already expanded, nothing more to do
        if (isAlreadyExpanded) {
            console.log(`[DockViewInterop] Drawer '${panelId}' is already expanded. Done.`);
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
        
        console.log(`[DockViewInterop] Clicking sidebar button to expand drawer for '${panelId}' (using setTimeout approach).`);
        
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
                    console.log(`[DockViewInterop] Native click() called on sidebar button for '${panelId}'.`);
                } catch (e) {
                    console.error(`[DockViewInterop] Error during click:`, e);
                }
                resolve();
            }, wasButtonHidden ? 100 : 0);
        });
        
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error showing and expanding drawer for panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Sets the width of a drawer panel.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The panel ID whose drawer width to set.
 * @param {number} widthPx - The width in pixels.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setDrawerWidth(dockViewId, panelId, widthPx) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
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
        
        // Try to set width via the group API if available
        if (group.api && typeof group.api.setSize === 'function') {
            group.api.setSize({ width: widthPx });
            console.log(`[DockViewInterop] Set drawer width for '${panelId}' to ${widthPx}px via API.`);
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
                console.log(`[DockViewInterop] Set drawer width for '${panelId}' to ${widthPx}px via element style.`);
                return true;
            }
        }
        
        // Second fallback: find the drawer wrapper element by searching
        const dockviewEl = document.getElementById(dockViewId);
        if (dockviewEl) {
            // Find all drawer containers on the right
            const drawerWrappers = dockviewEl.querySelectorAll('.bb-dockview-drawer-right, .dv-resize-container');
            for (const wrapper of drawerWrappers) {
                if (wrapper.contains(group.element)) {
                    wrapper.style.width = `${widthPx}px`;
                    wrapper.style.minWidth = `${widthPx}px`;
                    console.log(`[DockViewInterop] Set drawer wrapper width for '${panelId}' to ${widthPx}px.`);
                    return true;
                }
            }
        }
        
        console.warn(`[DockViewInterop] Could not find element to set width for drawer '${panelId}'.`);
        return false;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting drawer width for '${panelId}':`, error);
        return false;
    }
}
