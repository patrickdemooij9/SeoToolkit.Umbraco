import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";
import { SchemaPropertyViewModel, SchemaTypeViewModel } from "../api";

export interface EditableSchema {
  schemaAlias: string;
  properties: { [key: string]: PropertyValue };
}

export interface PropertyValue {
  value: string;
  isReference: boolean;
  referenceKey: string;
}

const REFERENCE_OPTIONS = [
  { key: "[PageName]", label: "Page Name" },
  { key: "[PageUrl]", label: "Page URL" },
  { key: "[SiteName]", label: "Site Name" },
  { key: "[SiteUrl]", label: "Site URL" },
];

@customElement("st-schema-property-modal")
export default class SchemaPropertyModal extends UmbModalBaseElement<
  { availableSchemas: SchemaTypeViewModel[]; editSchema?: EditableSchema },
  EditableSchema
> {
  @state()
  private _selectedSchemaAlias: string = "";

  @state()
  private _propertyValues: { [key: string]: PropertyValue } = {};

  override connectedCallback() {
    super.connectedCallback();

    if (this.data?.editSchema) {
      this._selectedSchemaAlias = this.data.editSchema.schemaAlias;
      this._propertyValues = { ...this.data.editSchema.properties };
    } else {
      this._selectedSchemaAlias = this.value?.schemaAlias ?? "";
      this._propertyValues = this.value?.properties ? { ...this.value.properties } : {};
    }
  }

  #onSchemaSelect(alias: string) {
    this._selectedSchemaAlias = alias;
    this._propertyValues = {};
    this.requestUpdate();
  }

  #onPropertyValueChange(propertyAlias: string, propertyValue: PropertyValue) {
    this._propertyValues = {
      ...this._propertyValues,
      [propertyAlias]: propertyValue,
    };
  }

  #handleClose() {
    this.modalContext?.reject();
  }

  #handleSubmit() {
    if (!this._selectedSchemaAlias) {
      return;
    }

    this.value = {
      schemaAlias: this._selectedSchemaAlias,
      properties: this._propertyValues,
    };
    this.modalContext?.submit();
  }

  #renderPropertyInput(property: SchemaPropertyViewModel, propertyValue: PropertyValue) {
    const value = propertyValue ?? { value: "", isReference: false, referenceKey: "" };
    const alias = property.alias ?? "";
    const displayName = property.displayName ?? "";
    const propEditor = property.propertyEditor ?? "Umb.PropertyEditorUi.TextBox";

    return html`
      <div class="property-row">
        <div class="property-header">
          <strong>${displayName}</strong>
        </div>

        <div class="property-toggle">
          <uui-toggle
            .checked=${value.isReference}
            @change="${(e: Event) => {
              const checked = (e.target as HTMLInputElement).checked;
              this.#onPropertyValueChange(alias, {
                ...value,
                isReference: checked,
                value: checked ? "" : value.value,
                referenceKey: checked ? REFERENCE_OPTIONS[0].key : "",
              });
            }}"
          >
            Use reference from context
          </uui-toggle>
        </div>

        ${value.isReference
          ? html`
              <div class="property-input">
                <uui-select
                  .value=${value.referenceKey}
                  @change="${(e: Event) => {
                    const referenceKey = (e.target as HTMLSelectElement).value;
                    this.#onPropertyValueChange(alias, {
                      ...value,
                      referenceKey,
                    });
                  }}"
                >
                  ${REFERENCE_OPTIONS.map(
                    (opt) => html`
                      <uui-option .value=${opt.key}>${opt.label}</uui-option>
                    `
                  )}
                </uui-select>
              </div>
            `
          : html`
              <div class="property-input">
                <umb-property
                  .alias=${alias}
                  .label=${displayName}
                  .value=${value.value}
                  .propertyEditorUiAlias=${propEditor}
                  @change="${(e: Event) => {
                    const val = (e as any).target.value;
                    this.#onPropertyValueChange(alias, {
                      ...value,
                      value: val ?? "",
                    });
                  }}"
                ></umb-property>
              </div>
            `}
      </div>
    `;
  }

  override render() {
    const selectedSchema = this.data?.availableSchemas.find(
      (s) => s.alias === this._selectedSchemaAlias
    );

    const properties = selectedSchema?.properties ?? [];

    return html`
      <umb-body-layout headline="Edit Schema">
        <div class="schema-selector">
          <uui-select
            label="Select Schema Type"
            .value=${this._selectedSchemaAlias}
            @change="${(e: Event) => {
              this.#onSchemaSelect((e.target as HTMLSelectElement).value);
            }}"
          >
            <uui-option value="">Select a schema type...</uui-option>
            ${this.data?.availableSchemas.map(
              (schema) => html`
                <uui-option .value=${schema.alias}>${schema.name}</uui-option>
              `
            )}
          </uui-select>
        </div>

        ${properties.length > 0
          ? html`
              <div class="schema-properties">
                ${properties.map(
                  (prop) =>
                    this.#renderPropertyInput(prop, this._propertyValues[prop.alias!])
                )}
              </div>
            `
          : this._selectedSchemaAlias
          ? html`<p>No properties available for this schema.</p>`
          : ""}

        <umb-footer-layout slot="footer">
          <uui-button
            id="close"
            label="Cancel"
            look="primary"
            color="danger"
            slot="actions"
            @click="${this.#handleClose}"
            >Cancel</uui-button
          >
          <uui-button
            id="save"
            label="Save"
            look="primary"
            color="positive"
            slot="actions"
            @click="${this.#handleSubmit}"
            >Save</uui-button
          >
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  static styles = [
    css`
      .schema-selector {
        margin-bottom: 16px;
      }

      .schema-selector uui-select {
        width: 100%;
      }

      .schema-properties {
        display: flex;
        flex-direction: column;
        gap: 12px;
      }

      .property-row {
        padding: 12px;
        border: 1px solid var(--uui-palette-gravel);
        border-radius: 4px;
        background-color: var(--uui-palette-surface);
      }

      .property-header {
        margin-bottom: 8px;
      }

      .property-toggle {
        margin-bottom: 12px;
      }

      .property-input {
        margin-top: 8px;
      }

      .property-input,
      .property-input uui-select,
      .property-input umb-property {
        width: 100%;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-schema-property-modal": SchemaPropertyModal;
  }
}
