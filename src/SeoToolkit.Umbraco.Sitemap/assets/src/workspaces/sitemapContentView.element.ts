import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import { UmbPropertyDatasetElement, UmbPropertyValueData } from "@umbraco-cms/backoffice/property";
import ChangeFrequence from "../models/changeFrequency";
import Priority from "../models/priority";
import SitemapContentViewContext, { ST_SITEMAP_CONTENT_TOKEN_CONTEXT } from "./sitemapContentViewContext";

// Label for the "no override / inherit from document type" option
const INHERITED_LABEL = "Inherited";

type HideFromSitemapOption = { name: string; value: boolean | undefined };

@customElement("st-sitemap-content-view")
export default class SitemapContentViewElement extends UmbElementMixin(LitElement) {
    #context?: SitemapContentViewContext;

    #hideOptions: HideFromSitemapOption[] = [
        { name: INHERITED_LABEL, value: undefined },
        { name: "Hide from sitemap", value: true },
        { name: "Show in sitemap", value: false },
    ];

    #changeFrequences: ChangeFrequence[] = [
        { name: INHERITED_LABEL, value: undefined },
        { name: "Always", value: "always" },
        { name: "Hourly", value: "hourly" },
        { name: "Daily", value: "daily" },
        { name: "Weekly", value: "weekly" },
        { name: "Monthly", value: "monthly" },
        { name: "Yearly", value: "yearly" },
        { name: "Never", value: "never" },
    ];

    #priorities: Priority[] = [
        { name: INHERITED_LABEL, value: undefined },
        { name: "0.1", value: 0.1 },
        { name: "0.2", value: 0.2 },
        { name: "0.3", value: 0.3 },
        { name: "0.4", value: 0.4 },
        { name: "0.5", value: 0.5 },
        { name: "0.6", value: 0.6 },
        { name: "0.7", value: 0.7 },
        { name: "0.8", value: 0.8 },
        { name: "0.9", value: 0.9 },
        { name: "1", value: 1 },
    ];

    @state()
    _content?: UmbPropertyValueData[] = [];

    @state()
    _effectiveHideFromSitemap?: boolean;

    @state()
    _effectiveChangeFrequency?: string | null;

    @state()
    _effectivePriority?: number | null;

    constructor() {
        super();

        this.consumeContext(ST_SITEMAP_CONTENT_TOKEN_CONTEXT, (instance) => {
            if (!instance) return;
            this.#context = instance;

            this.observe(instance.model, (item) => {
                if (!item) return;

                this._effectiveHideFromSitemap = item.effectiveHideFromSitemap;
                this._effectiveChangeFrequency = item.effectiveChangeFrequency;
                this._effectivePriority = item.effectivePriority;

                const hideOption = item.hideFromSitemap == null
                    ? this.#hideOptions[0]
                    : this.#hideOptions.find((o) => o.value === item.hideFromSitemap) ?? this.#hideOptions[0];

                const changeFrequence = item.changeFrequency == null
                    ? INHERITED_LABEL
                    : (this.#changeFrequences.find((f) => f.value === item.changeFrequency)?.name ?? INHERITED_LABEL);

                const priority = item.priority == null
                    ? INHERITED_LABEL
                    : (this.#priorities.find((p) => p.value === item.priority)?.name ?? INHERITED_LABEL);

                this._content = [
                    { alias: "hideFromSitemap", value: [hideOption.name] },
                    { alias: "changeFrequency", value: [changeFrequence] },
                    { alias: "priority", value: [priority] },
                ];
            });
        });
    }

    #onPropertyDataChange(e: Event) {
        const value = (e.target as UmbPropertyDatasetElement).value;

        const newValue: Record<string, unknown> = {};
        value.forEach((item) => {
            const itemValue = item.value as string[] | undefined;
            const selected = itemValue?.[0];

            switch (item.alias) {
                case "hideFromSitemap": {
                    const option = this.#hideOptions.find((o) => o.name === selected);
                    newValue[item.alias] = option?.name === INHERITED_LABEL ? null : option?.value;
                    break;
                }
                case "changeFrequency": {
                    const opt = this.#changeFrequences.find((f) => f.name === selected);
                    newValue[item.alias] = opt?.name === INHERITED_LABEL ? null : opt?.value;
                    break;
                }
                case "priority": {
                    const opt = this.#priorities.find((p) => p.name === selected);
                    newValue[item.alias] = opt?.name === INHERITED_LABEL ? null : opt?.value;
                    break;
                }
            }
        });

        this.#context?.update(newValue as any);
    }

    #effectiveHideLabel() {
        if (this._effectiveHideFromSitemap === true) return "Hide from sitemap";
        if (this._effectiveHideFromSitemap === false) return "Show in sitemap";
        return "Show in sitemap";
    }

    render() {
        return html`
            <uui-box>
                <umb-property-dataset
                    .value=${this._content!}
                    @change=${this.#onPropertyDataChange}>
                    <umb-property
                        alias="hideFromSitemap"
                        label="Hide from sitemap"
                        description="Override whether this page is hidden from the sitemap. 'Inherited' uses the document type setting (currently: ${this.#effectiveHideLabel()})."
                        property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
                        .config=${[{
                            alias: "items",
                            value: this.#hideOptions.map((o) => o.name),
                        }]}>
                    </umb-property>
                    <umb-property
                        alias="changeFrequency"
                        label="Change frequency"
                        description="Override the change frequency for this page. 'Inherited' uses the document type setting${this._effectiveChangeFrequency ? ` (currently: ${this._effectiveChangeFrequency})` : ''}."
                        property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
                        .config=${[{
                            alias: "items",
                            value: this.#changeFrequences.map((f) => f.name),
                        }]}>
                    </umb-property>
                    <umb-property
                        alias="priority"
                        label="Priority"
                        description="Override the priority for this page. 'Inherited' uses the document type setting${this._effectivePriority != null ? ` (currently: ${this._effectivePriority})` : ''}."
                        property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
                        .config=${[{
                            alias: "items",
                            value: this.#priorities.map((p) => p.name),
                        }]}>
                    </umb-property>
                </umb-property-dataset>
            </uui-box>
        `;
    }
}
