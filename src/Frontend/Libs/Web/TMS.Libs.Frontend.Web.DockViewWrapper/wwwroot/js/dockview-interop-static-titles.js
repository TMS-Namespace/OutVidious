/**
 * DockView Interop - Static Title Operations
 * Handles setting and clearing static titles on group sidebar buttons.
 */

import { getDockview, findPanelByTitle } from './dockview-interop-core.js';

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
            
            console.log(`[DockViewInterop]   Checking button: text='${btnText}', spanText='${spanText}'`);
            
            if (btnText === panelTitle || spanText === panelTitle || btnText?.includes(panelTitle)) {
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
