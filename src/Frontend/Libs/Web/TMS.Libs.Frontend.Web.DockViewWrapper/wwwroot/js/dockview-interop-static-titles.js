/**
 * DockView Interop - Static Title Operations
 * Handles setting and clearing static titles on group sidebar buttons.
 */

import { getDockview, findPanelByTitle, findPanelByKey } from './dockview-interop-core.js';

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
 * Sets a static title for a group's sidebar button that persists across panel changes.
 * When set, the button text will not change when switching between panels in the group.
 * This is achieved by:
 * 1. Finding the sidebar button for the group using a panel title that belongs to that group
 * 2. Setting the button text to the static title
 * 3. Marking the button with a data attribute to prevent DockView from updating it
 * 4. Setting up a MutationObserver to re-apply the static title if DockView tries to change it
 * 
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of any panel in the group (used to find the sidebar button).
 * @param {string} staticTitle - The static title to display on the sidebar button.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setGroupStaticTitle(dockViewId, panelTitle, staticTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        // Find the sidebar button for this group using panel title
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        const asideButtons = dockviewEl.querySelectorAll('.bb-dockview-aside-button');
        let matchedBtn = null;
        
        console.log(`[DockViewInterop] Looking for sidebar button with panel title '${panelTitle}' among ${asideButtons.length} buttons.`);
        
        // Try to find button by panel title
        for (const btn of asideButtons) {
            const btnText = btn.textContent?.trim();
            const spanText = btn.querySelector('span')?.textContent?.trim();
            // Also check data-panel-title for already-set static titles and data-hidden-panel-title
            const dataPanelTitle = btn.dataset.panelTitle;
            const hiddenPanelTitle = btn.dataset.hiddenPanelTitle;
            
            console.log(`[DockViewInterop]   Checking button: text='${btnText}', spanText='${spanText}', dataPanelTitle='${dataPanelTitle}'`);
            
            if (btnText === panelTitle || spanText === panelTitle || btnText?.includes(panelTitle) || dataPanelTitle === panelTitle || hiddenPanelTitle === panelTitle) {
                matchedBtn = btn;
                console.log(`[DockViewInterop]   Matched!`);
                break;
            }
        }
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Sidebar button not found for panel '${panelTitle}'.`);
            return false;
        }
        
        // Set the static title
        matchedBtn.textContent = staticTitle;
        matchedBtn.dataset.staticTitle = staticTitle;
        matchedBtn.dataset.panelTitle = panelTitle;
        
        // Set up MutationObserver to re-apply static title if DockView tries to change it
        const observer = new MutationObserver((mutations) => {
            for (const mutation of mutations) {
                if (mutation.type === 'characterData' || mutation.type === 'childList') {
                    const currentText = matchedBtn.textContent?.trim();
                    const expectedTitle = matchedBtn.dataset.staticTitle;
                    
                    if (expectedTitle && currentText !== expectedTitle) {
                        // Disconnect observer temporarily to avoid infinite loop
                        observer.disconnect();
                        matchedBtn.textContent = expectedTitle;
                        // Reconnect observer
                        observer.observe(matchedBtn, { characterData: true, childList: true, subtree: true });
                        console.log(`[DockViewInterop] Re-applied static title '${expectedTitle}' to sidebar button.`);
                    }
                }
            }
        });
        
        // Store observer reference for cleanup
        matchedBtn._staticTitleObserver = observer;
        
        // Start observing
        observer.observe(matchedBtn, { characterData: true, childList: true, subtree: true });
        
        console.log(`[DockViewInterop] Set static title '${staticTitle}' for panel '${panelTitle}' sidebar button.`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting static title for panel '${panelTitle}':`, error);
        return false;
    }
}

/**
 * Removes the static title from a group's sidebar button, reverting to default DockView behavior.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelTitle - The title of any panel in the group.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function clearGroupStaticTitle(dockViewId, panelTitle) {
    try {
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        // Find button with our panelTitle data attribute
        const matchedBtn = dockviewEl.querySelector(`.bb-dockview-aside-button[data-panel-title="${panelTitle}"]`);
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Sidebar button with static title not found for panel '${panelTitle}'.`);
            return false;
        }
        
        // Disconnect the observer if it exists
        if (matchedBtn._staticTitleObserver) {
            matchedBtn._staticTitleObserver.disconnect();
            delete matchedBtn._staticTitleObserver;
        }
        
        // Remove data attributes
        delete matchedBtn.dataset.staticTitle;
        delete matchedBtn.dataset.panelTitle;
        
        console.log(`[DockViewInterop] Cleared static title for panel '${panelTitle}'.`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error clearing static title for panel '${panelTitle}':`, error);
        return false;
    }
}

// ============================================================================
// Key-based functions (preferred over title-based)
// ============================================================================

/**
 * Sets a static title for a group's sidebar button by panel key.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelKey - The unique key of any panel in the group.
 * @param {string} staticTitle - The static title to display on the sidebar button.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setGroupStaticTitleByKey(dockViewId, panelKey, staticTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;
        
        const panel = findPanelByKey(dockview, panelKey);
        if (!panel) {
            console.warn(`[DockViewInterop] Panel with key '${panelKey}' not found.`);
            return false;
        }
        
        const dockviewEl = document.getElementById(dockViewId);
        if (!dockviewEl) {
            console.warn(`[DockViewInterop] DockView element '${dockViewId}' not found.`);
            return false;
        }
        
        // Find sidebar button by key
        const matchedBtn = findSidebarButtonByKey(dockviewEl, panelKey, panel);
        
        if (!matchedBtn) {
            console.warn(`[DockViewInterop] Sidebar button not found for panel key '${panelKey}'.`);
            return false;
        }
        
        // Set the static title
        matchedBtn.textContent = staticTitle;
        matchedBtn.dataset.staticTitle = staticTitle;
        matchedBtn.dataset.panelKey = panelKey;
        matchedBtn.dataset.panelTitle = panel.title; // Keep for legacy compatibility
        
        // Set up MutationObserver to re-apply static title if DockView tries to change it
        const observer = new MutationObserver((mutations) => {
            for (const mutation of mutations) {
                if (mutation.type === 'characterData' || mutation.type === 'childList') {
                    const currentText = matchedBtn.textContent?.trim();
                    const expectedTitle = matchedBtn.dataset.staticTitle;
                    
                    if (expectedTitle && currentText !== expectedTitle) {
                        observer.disconnect();
                        matchedBtn.textContent = expectedTitle;
                        observer.observe(matchedBtn, { characterData: true, childList: true, subtree: true });
                        console.log(`[DockViewInterop] Re-applied static title '${expectedTitle}' to sidebar button.`);
                    }
                }
            }
        });
        
        // Store observer reference for cleanup
        matchedBtn._staticTitleObserver = observer;
        
        // Start observing
        observer.observe(matchedBtn, { characterData: true, childList: true, subtree: true });
        
        console.log(`[DockViewInterop] Set static title '${staticTitle}' for panel key '${panelKey}'.`);
        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting static title for panel key '${panelKey}':`, error);
        return false;
    }
}
