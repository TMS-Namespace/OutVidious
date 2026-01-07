/**
 * DockView Interop - Core Utilities
 * Provides core helper functions for accessing DockView instances and finding panels/groups.
 * 
 * DockViewV2 stores its instance in: Data.get(dockViewId).dockview
 * We access the internal dockview instance and manipulate panels/groups directly.
 */

// Reference to BootstrapBlazor's Data module (loaded dynamically)
let bbData = null;

/**
 * Loads the BootstrapBlazor Data module if not already loaded.
 * @returns {Promise<object|null>} The Data module or null if failed.
 */
export async function loadDataModule() {
    if (bbData) return bbData;
    
    try {
        // Import the Data module from BootstrapBlazor
        const module = await import('/_content/BootstrapBlazor/modules/data.js');
        bbData = module.default;
        return bbData;
    } catch (e) {
        console.error('[DockViewInterop] Failed to load BootstrapBlazor Data module:', e);
        return null;
    }
}

/**
 * Gets the dockview instance from the BootstrapBlazor Data store.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @returns {Promise<object|null>} The dockview instance or null if not found.
 */
export async function getDockview(dockViewId) {
    // First check if element exists
    const el = document.getElementById(dockViewId);
    if (!el) {
        console.warn(`[DockViewInterop] Element with id '${dockViewId}' not found.`);
        return null;
    }
    
    // Try to get from BootstrapBlazor Data store
    const Data = await loadDataModule();
    if (Data) {
        const dock = Data.get(dockViewId);
        if (dock && dock.dockview) {
            return dock.dockview;
        }
    }
    
    // Fallback: check window.dockview
    if (window.dockview) {
        return window.dockview;
    }
    
    console.warn(`[DockViewInterop] Dockview instance not found for '${dockViewId}'.`);
    return null;
}

/**
 * Finds a panel by its internal ID.
 * @param {object} dockview - The dockview instance.
 * @param {string} panelId - The internal panel ID.
 * @returns {object|null} The panel or null if not found.
 */
export function findPanelById(dockview, panelId) {
    if (!dockview || !dockview.panels) {
        return null;
    }
    
    return dockview.panels.find(p => p.id === panelId) || null;
}

/**
 * Finds a group containing a panel with the given ID.
 * @param {object} dockview - The dockview instance.
 * @param {string} panelId - The panel ID.
 * @returns {object|null} The group or null if not found.
 */
export function findGroupByPanelId(dockview, panelId) {
    const panel = findPanelById(dockview, panelId);
    return panel?.group || null;
}

/**
 * Gets a group by its internal ID.
 * @param {object} dockview - The dockview instance.
 * @param {string} groupId - The group ID.
 * @returns {object|null} The group or null if not found.
 */
export function getGroupById(dockview, groupId) {
    if (!dockview || !dockview.groups) {
        return null;
    }
    
    return dockview.api?.getGroup?.(groupId) ?? dockview.groups.find(g => g.id === groupId) ?? null;
}
