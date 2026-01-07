export class DockviewPanelContent {
    constructor(option) {
        this.option = option
    }

    init(parameter) {
        const { params, api: { panel, accessor: { params: { template } } } } = parameter;
        const { titleClass, titleWidth, class: panelClass, id: panelId } = params;
        const { tab, content } = panel.view

        if (template) {
            const resolvedId = panelId ?? this.option.id;
            this._element = template.querySelector(`[data-bb-id="${resolvedId}"]`);
        }

        if (titleClass) {
            tab._content.classList.add(titleClass);
        }
        if (titleWidth) {
            tab._content.style.width = `${titleWidth}px`;
        }
        if (panelClass) {
            panelClass.split(' ').forEach(className => {
                content.element.classList.add(className);
            });
        }
    }

    get element() {
        return this._element;
    }
}
