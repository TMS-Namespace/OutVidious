/**
 * DockView Interop - Static Title Operations
 * Handles setting and clearing static titles on group sidebar buttons.
 */

import { getDockview, findGroupByPanelId } from './dockview-interop-core.js';

const setSidebarButtonContent = (button, title, iconClass) => {
    button.textContent = '';
    if (iconClass) {
        const icon = document.createElement('i');
        icon.className = `bb-dockview-aside-button-icon ${iconClass}`;
        icon.setAttribute('aria-hidden', 'true');
        button.append(icon);
    }
    if (title) {
        const titleSpan = document.createElement('span');
        titleSpan.className = 'bb-dockview-aside-button-title';
        titleSpan.textContent = title;
        button.append(titleSpan);
    }
};

const findGroupButton = (dockview, group) => {
    const dockviewEl = document.getElementById(dockview.id) ?? dockview.element;
    if (!dockviewEl) {
        return null;
    }

    const groupId = `${dockview.id}_${group.id}`;
    return dockviewEl.querySelector(`.bb-dockview-aside>[groupId="${groupId}"]`);
};

/**
 * Sets a static title for a group's sidebar button that persists across panel changes.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The ID of any panel in the group.
 * @param {string} staticTitle - The static title to display on the sidebar button.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function setGroupStaticTitle(dockViewId, panelId, staticTitle) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;

        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Group not found for panel '${panelId}'.`);
            return false;
        }

        const button = findGroupButton(dockview, group);
        if (!button) {
            console.warn(`[DockViewInterop] Sidebar button not found for panel '${panelId}'.`);
            return false;
        }

        setSidebarButtonContent(button, staticTitle, button.dataset.staticIcon);
        button.dataset.staticTitle = staticTitle;

        const observer = new MutationObserver(() => {
            const expectedTitle = button.dataset.staticTitle;
            const currentText = button.textContent?.trim();
            if (expectedTitle && currentText !== expectedTitle) {
                observer.disconnect();
                setSidebarButtonContent(button, expectedTitle, button.dataset.staticIcon);
                observer.observe(button, { characterData: true, childList: true, subtree: true });
            }
        });

        button._staticTitleObserver = observer;
        observer.observe(button, { characterData: true, childList: true, subtree: true });

        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error setting static title for panel '${panelId}':`, error);
        return false;
    }
}

/**
 * Removes the static title from a group's sidebar button, reverting to default DockView behavior.
 * @param {string} dockViewId - The DockViewV2 element ID.
 * @param {string} panelId - The ID of any panel in the group.
 * @returns {Promise<boolean>} True if the operation succeeded.
 */
export async function clearGroupStaticTitle(dockViewId, panelId) {
    try {
        const dockview = await getDockview(dockViewId);
        if (!dockview) return false;

        const group = findGroupByPanelId(dockview, panelId);
        if (!group) {
            console.warn(`[DockViewInterop] Group not found for panel '${panelId}'.`);
            return false;
        }

        const button = findGroupButton(dockview, group);
        if (!button) {
            console.warn(`[DockViewInterop] Sidebar button not found for panel '${panelId}'.`);
            return false;
        }

        if (button._staticTitleObserver) {
            button._staticTitleObserver.disconnect();
            delete button._staticTitleObserver;
        }

        delete button.dataset.staticTitle;

        const fallbackTitle = group.activePanel?.title ?? group.panels?.[0]?.title;
        if (fallbackTitle) {
            setSidebarButtonContent(button, fallbackTitle, button.dataset.staticIcon);
        }

        return true;
    } catch (error) {
        console.error(`[DockViewInterop] Error clearing static title for panel '${panelId}':`, error);
        return false;
    }
}
