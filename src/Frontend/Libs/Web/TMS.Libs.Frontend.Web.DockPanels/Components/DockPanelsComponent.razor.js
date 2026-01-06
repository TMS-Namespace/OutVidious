import { addLink, getTheme } from '../../BootstrapBlazor/modules/utility.js'
import { cerateDockview } from '../js/dockview-utils.js'
import Data from '../../BootstrapBlazor/modules/data.js'
import EventHandler from "../../BootstrapBlazor/modules/event-handler.js"

export async function init(id, invoke, options) {
    await addLink("./_content/TMS.Libs.Frontend.Web.DockPanels/css/dockview-bb.css")
    const el = document.getElementById(id);
    if (!el) {
        return;
    }

    if (options.theme === 'dockview-theme-light') {
        let theme = getTheme();
        if (theme === 'dark') {
            options.theme = `dockview-theme-dark`;
        }
    }
    const dockview = cerateDockview(el, options);
    const updateTheme = e => dockview.switchTheme(e.theme);
    Data.set(id, { el, dockview, updateTheme });

    const notifyActivePanels = () => {
        if (!dockview._panelActiveChanged) {
            return;
        }

        (dockview.groups || []).forEach(group => {
            const activePanel = group.activePanel;
            if (!activePanel) {
                return;
            }

            const params = group.getParams?.() || {};
            if (params.floatType === 'drawer') {
                const wrapper = group.element?.parentElement;
                if (!wrapper || !wrapper.classList.contains('active')) {
                    return;
                }
            }

            const panelKey = activePanel.params?.key || activePanel.params?.Key || activePanel.id || activePanel.title;
            dockview._panelActiveChanged.fire({ title: activePanel.title, key: panelKey, isActive: true });
        });
    };

    dockview.on('initialized', () => {
        invoke.invokeMethodAsync(options.initializedCallback);
        dockview._fronttubeActiveTrackingReady = true;
        notifyActivePanels();
        setTimeout(notifyActivePanels, 100);
    });
    dockview.on('lockChanged', ({ title, isLock }) => {
        invoke.invokeMethodAsync(options.lockChangedCallback, title, isLock);
    });
    dockview.on('panelVisibleChanged', ({ title, status }) => {
        invoke.invokeMethodAsync(options.panelVisibleChangedCallback, title, status);
    });
    dockview.on('panelActiveChanged', ({ title, key, isActive }) => {
        invoke.invokeMethodAsync(options.panelActiveChangedCallback, title, key, isActive);
    });
    dockview.on('groupSizeChanged', () => {
        invoke.invokeMethodAsync(options.splitterCallback);
    });

    EventHandler.on(document, 'changed.bb.theme', updateTheme);
}

export function update(id, options) {
    const dock = Data.get(id)
    if (dock) {
        const { dockview } = dock;
        dockview.update(options);
    }
}

export function reset(id, options) {
    const dock = Data.get(id)
    if (dock) {
        const { dockview } = dock;
        dockview.reset(options);
    }
}

export function save(id) {
    let ret = '';
    const dock = Data.get(id)
    if (dock) {
        const { dockview } = dock;
        ret = JSON.stringify(dockview.toJSON());
    }
    return ret;
}

export function dispose(id) {
    const dock = Data.get(id)
    Data.remove(id);

    if (dock) {
        EventHandler.off(document, 'changed.bb.theme', dock.updateTheme);

        const { dockview } = dock;
        if (dockview) {
            dockview.dispose();
        }
    }
}
