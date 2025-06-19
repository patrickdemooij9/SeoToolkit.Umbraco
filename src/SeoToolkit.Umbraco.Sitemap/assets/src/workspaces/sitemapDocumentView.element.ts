import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import { UmbPropertyDatasetElement, UmbPropertyValueData } from "@umbraco-cms/backoffice/property";
import ChangeFrequence from "../models/changeFrequency";
import Priority from "../models/priority";
import SitemapDocumentViewContext, { ST_SITEMAP_DOCUMENT_TOKEN_CONTEXT } from "./sitemapDocumentViewContext";

@customElement("st-sitemap-document-view")
export default class SitemapDocumentViewElement extends UmbElementMixin(LitElement) {
    #context?: SitemapDocumentViewContext;

    #changeFrequences: ChangeFrequence[] = [
        { name: 'None', value: undefined },
        { name: "Always", value: "always" },
        { name: "Hourly", value: "hourly" },
        { name: "Daily", value: "daily" },
        { name: "Weekly", value: "weekly" },
        { name: "Monthly", value: "monthly" },
        { name: "Yearly", value: "yearly" },
        { name: "Never", value: "never" }
    ]

    #priorities: Priority[] = [
        { name: 'None', value: undefined },
        { name: '0.1', value: 0.1 },
        { name: '0.2', value: 0.2 },
        { name: '0.3', value: 0.3 },
        { name: '0.4', value: 0.4 },
        { name: '0.5', value: 0.5 },
        { name: '0.6', value: 0.6 },
        { name: '0.7', value: 0.7 },
        { name: '0.8', value: 0.8 },
        { name: '0.9', value: 0.9 },
        { name: '1', value: 1 },
    ]

    @state()
    _content?: UmbPropertyValueData[] = [];

    constructor() {
        super();

        this.consumeContext(ST_SITEMAP_DOCUMENT_TOKEN_CONTEXT, (instance) => {
            if (!instance) {
                return;
            }
            this.#context = instance;
            instance.model.subscribe((item) => {
                const changeFrequence = this.#changeFrequences.find((f) => f.value == item.changeFrequency)?.name;
                const priority = this.#priorities.find((p) => p.value == item.priority)?.name;
                this._content = [
                    {
                        alias: 'hideFromSitemap',
                        value: item.hideFromSitemap
                    },
                    {
                        alias: 'changeFrequency',
                        value: [changeFrequence]
                    },
                    {
                        alias: 'priority',
                        value: [priority]
                    }
                ]
            });
        });
    }

    #onPropertyDataChange(e: Event) {
        const value = (e.target as UmbPropertyDatasetElement).value;

        const newValue = {} as any;
        value.forEach((item) => {
            const itemValue = item.value as any;
            switch(item.alias){
                case 'hideFromSitemap':
                    newValue[item.alias] = itemValue;
                    break;
                case 'changeFrequency':
                    newValue[item.alias] = itemValue ? this.#changeFrequences.find((f) => f.name == itemValue[0])?.value : undefined;
                    break;
                case 'priority':
                    newValue[item.alias] = itemValue ? this.#priorities.find((f) => f.name == itemValue[0])?.value : undefined;
                    break;
            }
        });
        this.#context?.update(newValue);
    }

    render() {
        return html`
            <uui-box>
                <umb-property-dataset
                    .value=${this._content!}
                    @change=${this.#onPropertyDataChange}>
                    <umb-property 
                        alias='hideFromSitemap'
                        label='Hide from sitemap'
                        description='Check this box if you want to hide all pages with this type from the sitemap'
                        property-editor-ui-alias='Umb.PropertyEditorUi.Toggle'
                        val>
                    </umb-property>
                    <umb-property 
                        alias='changeFrequency'
                        label='Change frequence'
                        description='The change frequency showed in the sitemap for all pages of this type'
                        property-editor-ui-alias='Umb.PropertyEditorUi.Dropdown'
                        val
                        .config=${[{
                                alias: 'items',
                                value: this.#changeFrequences.map((def) => def.name)
                            }]}>
                    </umb-property>
                    <umb-property 
                        alias='priority'
                        label='Priority'
                        description='The priority showed in the sitemap for all pages of this type'
                        property-editor-ui-alias='Umb.PropertyEditorUi.Dropdown'
                        val
                        .config=${[{
                                alias: 'items',
                                value: this.#priorities.map((def) => def.name)
                            }]}>
                    </umb-property>
                </umb-property-dataset>
            </uui-box>
        `
    }
}