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
 * Finds a panel by its title.
 * @param {object} dockview - The dockview instance.
 * @param {string} panelTitle - The title of the panel to find.
 * @returns {object|null} The panel or null if not found.
 */
export function findPanelByTitle(dockview, panelTitle) {
    if (!dockview || !dockview.panels) {
        return null;
    }
    
    return dockview.panels.find(p => p.title === panelTitle) || null;
}

/**
 * Finds a group containing a panel with the given title.
 * @param {object} dockview - The dockview instance.
 * @param {string} panelTitle - The title of the panel.
 * @returns {object|null} The group or null if not found.
 */
export function findGroupByPanelTitle(dockview, panelTitle) {
    const panel = findPanelByTitle(dockview, panelTitle);
    return panel?.group || null;
}

/**
 * Finds a panel by its key (unique identifier).
 * The key is set via the Key property on DockViewComponent.
 * If no Key is specified, the Title is used as the key.
 * @param {object} dockview - The dockview instance.
 * @param {string} panelKey - The key/id of the panel to find.
 * @returns {object|null} The panel or null if not found.
 */
export function findPanelByKey(dockview, panelKey) {
    if (!dockview || !dockview.panels) {
        return null;
    }
    
    // Try multiple locations where the key might be stored:
    // 1. panel.id - the internal dockview id (BootstrapBlazor uses Key as id)
    // 2. panel.params?.key or panel.params?.Key - the Key property from DockViewComponent
    // 3. panel.title - fall back to title match
    const found = dockview.panels.find(p => 
        p.id === panelKey || 
        p.params?.key === panelKey ||
        p.params?.Key === panelKey ||
        p.title === panelKey
    );
    
    if (!found) {
        console.log(`[DockViewInterop] findPanelByKey: Panel with key '${panelKey}' not found. Available panels:`, 
            dockview.panels.map(p => ({ id: p.id, title: p.title, paramsKey: p.params?.key || p.params?.Key })));
    }
    
    return found || null;
}

/**
 * Finds a group containing a panel with the given key.
 * @param {object} dockview - The dockview instance.
 * @param {string} panelKey - The key of the panel.
 * @returns {object|null} The group or null if not found.
 */
export function findGroupByPanelKey(dockview, panelKey) {
    const panel = findPanelByKey(dockview, panelKey);
    return panel?.group || null;
}

/**
 * Gets a group by its index.
 * @param {object} dockview - The dockview instance.
 * @param {number} groupIndex - The 0-based index of the group.
 * @returns {object|null} The group or null if not found.
 */
export function getGroupByIndex(dockview, groupIndex) {
    if (!dockview || !dockview.groups) {
        return null;
    }
    
    // Filter to only grid groups (not floating)
    const gridGroups = dockview.groups.filter(g => g.model?.location?.type === 'grid');
    
    if (groupIndex < 0 || groupIndex >= gridGroups.length) {
        console.warn(`[DockViewInterop] Group index ${groupIndex} out of range (0-${gridGroups.length - 1}).`);
        return null;
    }
    
    return gridGroups[groupIndex];
}
