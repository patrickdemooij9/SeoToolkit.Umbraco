import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";
import { SchemaPropertyViewModel, SchemaTypeViewModel } from "../api";

export interface EditableSchema {
  schemaAlias: string;
  displayName?: string;
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
  private _displayName: string = "";

  @state()
  private _propertyValues: { [key: string]: PropertyValue } = {};

  override connectedCallback() {
    super.connectedCallback();

    if (this.data?.editSchema) {
      this._selectedSchemaAlias = this.data.editSchema.schemaAlias;
      this._displayName = this.data.editSchema.displayName ?? "";
      this._propertyValues = { ...this.data.editSchema.properties };
    } else {
      this._selectedSchemaAlias = this.value?.schemaAlias ?? "";
      this._displayName = this.value?.displayName ?? "";
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
      displayName: this._displayName || undefined,
      properties: this._propertyValues,
    };
    this.modalContext?.submit();
  }

  #renderPropertyInput(property: SchemaPropertyViewModel, propertyValue: PropertyValue) {
    const value = propertyValue ?? { value: "", isReference: false, referenceKey: "" };
    const alias = property.alias ?? "";
    const displayName = property.displayName ?? "";
    const propEditor = property.propertyEditor ?? "Umb.PropertyEditorUi.TextBox";
    const allowReference = property.allowReference !== false;

    return html`
      <div class="property-row">
        <div class="property-header">
          <strong>${displayName}</strong>
        </div>

        ${allowReference
          ? html`
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
            `
          : ""}

        ${value.isReference && allowReference
          ? html`
              <div class="property-input">
                <select
                  class="native-select"
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
                      <option
                        .value=${opt.key}
                        ?selected=${opt.key === value.referenceKey}
                      >${opt.label}</option>
                    `
                  )}
                </select>
              </div>
            `
          : html`
              <div class="property-input">
                ${propEditor === "Umb.PropertyEditorUi.MediaPicker"
                  ? html`
                      <umb-input-media
                        max="1"
                        .selection=${value.value ? [{ unique: value.value }] : []}
                        @change="${(e: Event) => {
                          const selection = (e.target as any).selection as Array<{ unique: string }>;
                          const guid = selection.length > 0 ? selection[0].unique : "";
                          this.#onPropertyValueChange(alias, {
                            ...value,
                            value: guid,
                          });
                        }}"
                      ></umb-input-media>
                    `
                  : propEditor === "Umb.PropertyEditorUi.TextArea"
                  ? html`
                      <uui-textarea
                        .value=${value.value ?? ""}
                        @input="${(e: Event) => {
                          const val = (e.target as HTMLTextAreaElement).value;
                          this.#onPropertyValueChange(alias, {
                            ...value,
                            value: val ?? "",
                          });
                        }}"
                      ></uui-textarea>
                    `
                  : html`
                      <uui-input
                        .value=${value.value ?? ""}
                        @input="${(e: Event) => {
                          const val = (e.target as HTMLInputElement).value;
                          this.#onPropertyValueChange(alias, {
                            ...value,
                            value: val ?? "",
                          });
                        }}"
                      ></uui-input>
                    `}
              </div>
            `}
      </div>
    `;
  }

  override render() {
    const isEditing = !!this.data?.editSchema;
    const selectedSchema = this.data?.availableSchemas.find(
      (s) => s.alias === this._selectedSchemaAlias
    );

    const properties = selectedSchema?.properties ?? [];

    return html`
      <umb-body-layout headline="Edit Schema">
        ${!isEditing
          ? html`
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
            `
          : ""}

        <div class="display-name-row">
          <label class="display-name-label">Display Name (optional)</label>
          <uui-input
            placeholder="Leave empty to use the schema type name"
            .value=${this._displayName}
            @input="${(e: Event) => {
              this._displayName = (e.target as HTMLInputElement).value;
            }}"
          ></uui-input>
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

      .display-name-row {
        margin-bottom: 16px;
      }

      .display-name-label {
        display: block;
        font-weight: 500;
        margin-bottom: 4px;
        font-size: 0.875rem;
      }

      .display-name-row uui-input {
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
      .property-input uui-input,
      .property-input uui-textarea,
      .property-input .native-select {
        width: 100%;
      }

      .native-select {
        padding: 8px;
        border: 1px solid var(--uui-palette-gravel);
        border-radius: 4px;
        background-color: var(--uui-palette-surface);
        font-size: 14px;
        cursor: pointer;
        box-sizing: border-box;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-schema-property-modal": SchemaPropertyModal;
  }
}
