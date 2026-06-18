import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";
import { SchemaPropertyViewModel, SchemaTypeViewModel } from "../api";
import { UmbPropertyDatasetElement } from "@umbraco-cms/backoffice/property";
import { UmbPropertyTypeAppearanceModel } from "@umbraco-cms/backoffice/content-type";

export interface EditableSchema {
  schemaAlias: string;
  displayName?: string;
  renderAutomatically?: boolean;
  properties: { [key: string]: PropertyValue };
}

export interface PropertyValue {
  value: unknown | undefined;
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
  {
    availableSchemas: SchemaTypeViewModel[];
    editSchema?: EditableSchema;
    entryId?: string;
    showRenderToggle?: boolean;
    documentTypeKey?: string;
  },
  EditableSchema
> {
  @state()
  private _selectedSchemaAlias: string = "";

  @state()
  private _displayName: string = "";

  @state()
  private _renderAutomatically: boolean = true;

  @state()
  private _propertyValues: { [key: string]: PropertyValue } = {};

  propertyAppearance: UmbPropertyTypeAppearanceModel = {
    labelOnTop: true,
  };

  override connectedCallback() {
    super.connectedCallback();

    if (this.data?.editSchema) {
      this._selectedSchemaAlias = this.data.editSchema.schemaAlias;
      this._displayName = this.data.editSchema.displayName ?? "";
      this._renderAutomatically = this.data.editSchema.renderAutomatically ?? true;
      this._propertyValues = { ...this.data.editSchema.properties };
    } else {
      this._selectedSchemaAlias = this.value?.schemaAlias ?? "";
      this._displayName = this.value?.displayName ?? "";
      this._renderAutomatically = this.value?.renderAutomatically ?? true;
      this._propertyValues = this.value?.properties
        ? { ...this.value.properties }
        : {};
    }
  }

  #onPropertyValueChange(propertyAlias: string, propertyValue: PropertyValue) {
    this._propertyValues = {
      ...this._propertyValues,
      [propertyAlias]: propertyValue,
    };
  }

  #handlePropertyInputChange(
    propertyAlias: string,
    propertyValue: PropertyValue,
    event: Event,
  ) {
    const target = event.target as UmbPropertyDatasetElement;
    if (!target) {
      return;
    }
    const value = target.value.find((item) => item.alias === propertyAlias)?.value;
    this._propertyValues = {
      ...this._propertyValues,
      [propertyAlias]: {
        ...propertyValue,
        value,
      },
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
      renderAutomatically: this._renderAutomatically,
      properties: this._propertyValues,
    };
    this.modalContext?.submit();
  }

  #buildPropertyConfig(property: SchemaPropertyViewModel): Array<{ alias: string; value: unknown }> {
    const entryId = this.data?.entryId;
    // Convert static config from the property definition into the Umbraco config array format
    const staticConfig = Object.entries(property.config ?? {}).map(([alias, value]) => ({ alias, value }));

    // For nested schema editors, inject the current entry's ID as ownerKey so they can save sub-entries
    if (property.propertyEditor === "SeoToolkit.SchemaEditor") {
      if (entryId) {
        staticConfig.push({ alias: "nodeGuid", value: entryId });
      }
      // Pass the document type down so nested editors can offer document type entries for reuse.
      const documentTypeKey = this.data?.documentTypeKey;
      if (documentTypeKey) {
        staticConfig.push({ alias: "documentTypeKey", value: documentTypeKey });
      }
    }

    return staticConfig;
  }

  #renderPropertyInput(
    property: SchemaPropertyViewModel,
    propertyValue: PropertyValue,
  ) {
    const value = propertyValue ?? {
      value: undefined,
      isReference: false,
      referenceKey: undefined,
    };
    const alias = property.alias ?? "";
    const displayName = property.displayName ?? "";
    const propEditor =
      property.propertyEditor ?? "Umb.PropertyEditorUi.TextBox";
    const allowReference = property.allowReference !== false;
    const config = this.#buildPropertyConfig(property);

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
                      >
                        ${opt.label}
                      </option>
                    `,
                  )}
                </select>
              </div>
            `
          : html`
              <div class="property-input">
                <umb-property-dataset
                  .value=${[{ alias, value: value.value }]}
                  @change=${(e: Event) => {this.#handlePropertyInputChange(alias, value, e)}}
                >
                  <umb-property
                    .alias=${alias}
                    property-editor-ui-alias=${propEditor}
                    .appearance=${this.propertyAppearance}
                    .config=${config}
                  ></umb-property>
                </umb-property-dataset>
              </div>
            `}
      </div>
    `;
  }

  override render() {
    const selectedSchema = this.data?.availableSchemas.find(
      (s) => s.alias === this._selectedSchemaAlias,
    );

    const properties = selectedSchema?.properties ?? [];

    return html`
      <umb-body-layout headline="Edit Schema">
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

        ${this.data?.showRenderToggle
          ? html`
              <div class="render-toggle-row">
                <uui-toggle
                  .checked=${this._renderAutomatically}
                  @change="${(e: Event) => {
                    this._renderAutomatically = (
                      e.target as HTMLInputElement
                    ).checked;
                  }}"
                >
                  Render on all pages of this document type
                </uui-toggle>
                <small class="render-toggle-hint">
                  When disabled, this schema is not rendered automatically but
                  can still be referenced from individual pages.
                </small>
              </div>
            `
          : ""}

        ${properties.length > 0
          ? html`
              <div class="schema-properties">
                ${properties.map((prop) =>
                  this.#renderPropertyInput(
                    prop,
                    this._propertyValues[prop.alias!],
                  ),
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

      .render-toggle-row {
        margin-bottom: 16px;
      }

      .render-toggle-hint {
        display: block;
        margin-top: 4px;
        color: var(--uui-palette-grey-4);
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

        --uui-size-layout-1: 0;
      }

      .native-select {
        padding: 8px;
        border: 1px solid var(--uui-palette-gravel);
        border-radius: 4px;
        font-size: 14px;
        cursor: pointer;
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
