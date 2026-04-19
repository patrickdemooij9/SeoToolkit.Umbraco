import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import ChangeFrequence from "../models/changeFrequency";
import Priority from "../models/priority";
import SitemapContentViewContext, {
  ST_SITEMAP_CONTENT_TOKEN_CONTEXT,
} from "./sitemapContentViewContext";

// Label for the "no override / inherit from document type" option
const INHERITED_LABEL = "Inherited";

@customElement("st-sitemap-content-view")
export default class SitemapContentViewElement extends UmbElementMixin(
  LitElement,
) {
  #context?: SitemapContentViewContext;

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
  _inheritedChangeFrequency?: string | null;

  @state()
  _inheritedPriority?: number | null;

  constructor() {
    super();

    this.consumeContext(ST_SITEMAP_CONTENT_TOKEN_CONTEXT, (instance) => {
      if (!instance) return;
      this.#context = instance;

      this.observe(instance.model, (item) => {
        if (!item) return;

        this._inheritedChangeFrequency = item.inheritedChangeFrequency;
        this._inheritedPriority = item.inheritedPriority;

        const changeFrequence =
          item.changeFrequency == null
            ? INHERITED_LABEL
            : (this.#changeFrequences.find(
                (f) => f.value === item.changeFrequency,
              )?.name ?? INHERITED_LABEL);

        const priority =
          item.priority == null
            ? INHERITED_LABEL
            : (this.#priorities.find((p) => p.value === item.priority)?.name ??
              INHERITED_LABEL);

        this._content = [
          {
            alias: "excludeFromSitemap",
            value: item.excludeFromSitemap ?? false,
          },
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
      const itemValue = item.value as any;

      switch (item.alias) {
        case "excludeFromSitemap": {
          newValue[item.alias] = itemValue;
          break;
        }
        case "changeFrequency": {
          const selected = (itemValue as string[] | undefined)?.[0];
          const opt = this.#changeFrequences.find((f) => f.name === selected);
          newValue[item.alias] =
            opt?.name === INHERITED_LABEL ? null : opt?.value;
          break;
        }
        case "priority": {
          const selected = (itemValue as string[] | undefined)?.[0];
          const opt = this.#priorities.find((p) => p.name === selected);
          newValue[item.alias] =
            opt?.name === INHERITED_LABEL ? null : opt?.value;
          break;
        }
      }
    });

    this.#context?.update(newValue as any);
  }

  #renderInheritedNote(
    label: string,
    value: string | number | null | undefined,
  ) {
    const displayValue = value != null ? String(value) : "None";
    return html`
      <div class="inherited-value">
        <uui-icon name="icon-info"></uui-icon>
        Inherited ${label}:
        <span class="inherited-value-label">${displayValue}</span>
      </div>
    `;
  }

  render() {
    return html`
      <uui-box>
        <umb-property-dataset
          .value=${this._content!}
          @change=${this.#onPropertyDataChange}
        >
          <umb-property
            alias="excludeFromSitemap"
            label="Exclude from sitemap"
            description="When checked, this page is excluded from the sitemap regardless of all other settings."
            property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"
          >
          </umb-property>
          <umb-property
            alias="changeFrequency"
            label="Change frequency"
            description="Override the change frequency for this page. Select 'Inherited' to use the document type default."
            property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
            .validation=${{
              mandatory: true,
              mandatoryMessage: "This field is required",
            }}
            .config=${[
              {
                alias: "items",
                value: this.#changeFrequences.map((f) => f.name),
              },
            ]}
          >
          </umb-property>
          ${this.#renderInheritedNote(
            "change frequency",
            this._inheritedChangeFrequency,
          )}
          <umb-property
            alias="priority"
            label="Priority"
            description="Override the priority for this page. Select 'Inherited' to use the document type default."
            property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
            .validation=${{
              mandatory: true,
              mandatoryMessage: "This field is required",
            }}
            .config=${[
              {
                alias: "items",
                value: this.#priorities.map((p) => p.name),
              },
            ]}
          >
          </umb-property>
          ${this.#renderInheritedNote("priority", this._inheritedPriority)}
        </umb-property-dataset>
      </uui-box>
    `;
  }

  static styles = css`
    .inherited-value {
      font-size: 0.8125rem;
      color: var(--uui-color-text-alt, #666);
      margin: -6px 0 12px 0;
      padding: 4px 8px;
      background: var(--uui-color-surface-alt, #f5f5f5);
      border-radius: var(--uui-border-radius, 3px);
      border-left: 3px solid var(--uui-color-border, #ccc);
      display: flex;
      align-items: center;
      gap: 6px;
    }
    .inherited-value uui-icon {
      flex-shrink: 0;
      color: var(--uui-color-interactive, #006df4);
    }
    .inherited-value-label {
      font-weight: 500;
      color: var(--uui-color-text, #333);
    }
  `;
}
