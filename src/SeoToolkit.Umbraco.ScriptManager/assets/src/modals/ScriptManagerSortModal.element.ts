import {
    css,
    customElement,
    html,
    repeat,
    state,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { UmbSorterController } from "@umbraco-cms/backoffice/sorter";

export interface ScriptSortItem {
    id: string;
    name: string;
}

export interface ScriptSortModalData {
    scripts: Array<ScriptSortItem>;
}

@customElement("st-script-sort-modal")
export default class ScriptManagerSortModal extends UmbModalBaseElement<
    ScriptSortModalData,
    Array<ScriptSortItem>
> {
    #sorter: UmbSorterController<ScriptSortItem>;

    @state()
    private _scripts: Array<ScriptSortItem> = [];

    constructor() {
        super();

        this.#sorter = new UmbSorterController<ScriptSortItem>(this, {
            itemSelector: ".script-item",
            containerSelector: ".script-list",
            getUniqueOfElement: (element) => element.dataset.sortId,
            getUniqueOfModel: (model) => model.id,
            onChange: ({ model }) => {
                this._scripts = model;
            },
        });
    }

    override connectedCallback() {
        super.connectedCallback();
        this._scripts = [...(this.data?.scripts ?? [])];
        this.#sorter.setModel(this._scripts);
    }

    #handleSubmit() {
        this.value = this._scripts;
        this.modalContext?.submit();
    }

    #handleCancel() {
        this.modalContext?.reject();
    }

    override render() {
        return html`
            <umb-body-layout headline="Sort scripts">
                <uui-box>
                    <p>Drag and drop the scripts to set the order they will be rendered on the page.</p>
                    <div class="script-list">
                        ${repeat(
                            this._scripts,
                            (item) => item.id,
                            (item) => html`
                                <div data-sort-id=${item.id} class="script-item">
                                    <uui-icon name="icon-navigation"></uui-icon>
                                    <span>${item.name}</span>
                                </div>
                            `
                        )}
                    </div>
                </uui-box>
                <div slot="actions">
                    <uui-button
                        label="Cancel"
                        @click=${this.#handleCancel}>
                    </uui-button>
                    <uui-button
                        label="Save"
                        look="primary"
                        color="positive"
                        @click=${this.#handleSubmit}>
                    </uui-button>
                </div>
            </umb-body-layout>
        `;
    }

    static styles = [
        css`
            p {
                margin-top: 0;
            }

            .script-list {
                display: flex;
                flex-direction: column;
                gap: 4px;
            }

            .script-item {
                display: flex;
                align-items: center;
                gap: 8px;
                padding: 8px 12px;
                border: 1px solid var(--uui-color-border);
                border-radius: var(--uui-border-radius);
                background: var(--uui-color-surface);
                cursor: grab;
                user-select: none;
            }

            .script-item:hover {
                background: var(--uui-color-surface-emphasis);
            }

            uui-icon {
                flex-shrink: 0;
                color: var(--uui-color-text-alt);
            }
        `,
    ];
}
