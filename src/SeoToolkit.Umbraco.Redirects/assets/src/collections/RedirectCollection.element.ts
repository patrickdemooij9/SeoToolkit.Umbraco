import { UmbTableColumn, UmbTableConfig, UmbTableDeselectedEvent, UmbTableElement, UmbTableItem, UmbTableSelectedEvent } from "@umbraco-cms/backoffice/components";
import { customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

import './RedirectNameLayout.element';
import RedirectModuleContext, { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

@customElement("st-redirects-collection")
export default class RedirectCollection extends UmbLitElement {
    
    #context?: RedirectModuleContext;

    @state()
    private _tableConfig: UmbTableConfig = {
        allowSelection: true,
    };

    @state()
    private _tableColumns: Array<UmbTableColumn> = [
        {
            name: 'From',
            alias: 'from',
            elementName: 'st-redirect-name-column-layout'
        },
        {
            name: 'To',
            alias: 'to',
        },
        {
            name: 'Domain',
            alias: 'domain'
        },
        {
            name: 'Status code',
            alias: 'redirectCode'
        },
        {
            name: 'Last updated',
            alias: 'lastUpdated'
        },
        {
            name: 'Enabled',
            alias: 'enabled'
        }
    ];

    @state()
    private _tableItems: Array<UmbTableItem> = [];

    @state()
	private _selection: Array<string> = [];

    constructor() {
        super();

        this.loadItems();
    }

    async loadItems(){
        this.consumeContext(ST_REDIRECT_MODULE_TOKEN_CONTEXT, (instance) => {
            if (!instance) {
                return;
            }
            this.#context = instance;

            this.observe(this.#context.selection.selection, (selection) => this._selection = selection.filter(it => it) as string[]);
            this.observe(this.#context.items, (items) => {
                this._tableItems = items.map<UmbTableItem>((item) => {
                    return {
                        id: item.unique,
                        icon: 'icon-trafic',
                        data: [
                        {
                            columnAlias: 'from',
                            value: {
                                name: item.oldUrl,
                                unique: item.unique,
                                url: item.oldUrl,
                            }
                        }, {
                            columnAlias: 'to',
                            value: item.newUrl
                        },{
                            columnAlias: 'domain',
                            value: item.domain
                        },{
                            columnAlias: 'redirectCode',
                            value: item.statusCode
                        },{
                            columnAlias: 'enabled',
                            value: item.isEnabled
                        },{
                            columnAlias: 'lastUpdated',
                            value: item.lastUpdated
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