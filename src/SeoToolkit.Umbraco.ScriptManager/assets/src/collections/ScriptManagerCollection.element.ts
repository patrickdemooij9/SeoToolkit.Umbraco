import { UmbTableColumn, UmbTableConfig, UmbTableDeselectedEvent, UmbTableElement, UmbTableItem, UmbTableSelectedEvent } from "@umbraco-cms/backoffice/components";
import { customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import ScriptManagerModuleContext, { ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT } from "../workspaces/ScriptManagerModuleContext";

import './../workspaces/ScriptManagerNameLayout.element';

@customElement("script-manager-collection")
export default class ScriptManagerCollection extends UmbLitElement {
    
    #context?: ScriptManagerModuleContext;

    @state()
    private _tableConfig: UmbTableConfig = {
        allowSelection: true,
    };

    @state()
    private _tableColumns: Array<UmbTableColumn> = [
        {
            name: this.localize.term('general_name'),
            alias: 'name',
            elementName: 'st-name-column-layout'
        },
        {
            name: 'Definition',
            alias: 'definition',
        }
    ];

    @state()
    private _tableItems: Array<UmbTableItem> = [];

    @state()
	private _selection: Array<string> = [];

    constructor() {
        super();

        this.consumeContext(ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT, (instance) => {
            this.#context = instance;

            this.observe(this.#context.selection.selection, (selection) => this._selection = selection.filter(it => it) as string[]);
            this.observe(this.#context.items, (items) => {
                this._tableItems = items.map<UmbTableItem>((item) => {
                    return {
                        id: item.id.toString(),
                        icon: 'icon-script',
                        data: [
                        {
                            columnAlias: 'name',
                            value: {
                                name: item.name, unique: item.id
                            }
                        }, {
                            columnAlias: 'definition',
                            value: item.definitionName
                        }]
                    }
                })
            })
        })
    }

    #onSelected(event: UmbTableSelectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#context?.selection.setSelection(selection);
	}

	#onDeselected(event: UmbTableDeselectedEvent) {
		event.stopPropagation();
		const table = event.target as UmbTableElement;
		const selection = table.selection;
		this.#context?.selection.setSelection(selection);
	}

    render() {
        return html`
            <umb-table 
                .config=${this._tableConfig} 
                .columns=${this._tableColumns} 
                .items=${this._tableItems}
                .selection=${this._selection}
				@selected="${this.#onSelected}"
				@deselected="${this.#onDeselected}">
            </umb-table>
        `
    }
}