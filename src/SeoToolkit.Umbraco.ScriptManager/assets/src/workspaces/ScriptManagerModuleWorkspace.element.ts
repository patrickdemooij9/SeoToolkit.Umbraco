import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { css, html, LitElement } from "lit";
import { customElement, state } from "lit/decorators.js";
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import ScriptManagerModuleContext, { ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT } from "./ScriptManagerModuleContext";

@customElement('seotoolkit-module-scriptmanager')
export class ScriptManagerModuleWorkspace extends UmbElementMixin(LitElement) {

    #context?: ScriptManagerModuleContext;

    @state()
    private _tableConfig: UmbTableConfig = {
        allowSelection: false,
    };

    @state()
    private _tableColumns: Array<UmbTableColumn> = [
        {
            name: this.localize.term('general_name'),
            alias: 'name',
        },
        {
            name: 'Definition',
            alias: 'definition',
        }
    ];

    @state()
    private _tableItems: Array<UmbTableItem> = [];

    constructor() {
        super();

        this.consumeContext(ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT, (instance) => {
            this.#context = instance;
            this.observe(this.#context.scripts, (items) => {
                this._tableItems = items.map<UmbTableItem>((item) => {
                    return {
                        id: item.id.toString(),
                        icon: 'icon-book',
                        data: [{
                            columnAlias: 'name',
                            value: item.name
                        }, {
                            columnAlias: 'definition',
                            value: item.definitionName
                        }]
                    }
                })
            })
        })
    }

    render() {
        return html`
            <div class="scriptManagerWorkspace">
                <div class="button-bar">
                    <uui-button look="outline" href="/umbraco/section/SeoToolkit/workspace/script/create" label="Create"></uui-button>
                </div>
                <umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
            </div>
        `
    }

    static styles = [
        css`
            .scriptManagerWorkspace {
                padding: var(--uui-size-layout-1);
            }

            .button-bar {
                margin-bottom: 20px;
            }
        `
    ]
}

export default ScriptManagerModuleWorkspace;