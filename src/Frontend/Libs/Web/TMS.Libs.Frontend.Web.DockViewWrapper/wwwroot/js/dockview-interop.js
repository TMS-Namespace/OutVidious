/**
 * DockView Interop Module for FrontTube
 * 
 * This is the main entry point that re-exports all functions from the modular components.
 * The actual implementation is split across multiple files for better organization:
 * 
 * - dockview-interop-core.js - Core utilities (getDockview, find functions)
 * - dockview-interop-pin.js - Pin/unpin operations
 * - dockview-interop-float.js - Float/dock operations
 * - dockview-interop-panel.js - Panel management (activate, state, add/remove)
 * - dockview-interop-drawer.js - Drawer operations (show, hide, width)
 * - dockview-interop-group.js - Group visibility operations
 * - dockview-interop-static-titles.js - Static title operations
 */

// Core utilities
export { getDockview, findPanelByTitle, findGroupByPanelTitle, getGroupByIndex, loadDataModule } from './dockview-interop-core.js';

// Pin/unpin operations
export { unpinGroup, unpinPanel, pinPanel, unpinGroupByPanelTitle } from './dockview-interop-pin.js';

// Float/dock operations
export { floatPanel, dockPanel } from './dockview-interop-float.js';

// Panel management
export { collapsePanel, expandPanel, activatePanel, getPanelState, panelExists, addPanelToNewGroup, removePanel } from './dockview-interop-panel.js';

// Drawer operations
export { showDrawer, hideDrawer, hideDrawerTab, showDrawerTab, showAndExpandDrawer, setDrawerWidth } from './dockview-interop-drawer.js';

// Group visibility
export { showGroup, hideGroup, isGroupVisible } from './dockview-interop-group.js';

// Static titles
export { setGroupStaticTitle, clearGroupStaticTitle } from './dockview-interop-static-titles.js';

