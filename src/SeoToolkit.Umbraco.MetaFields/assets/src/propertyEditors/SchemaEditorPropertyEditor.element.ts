import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import {
  UmbPropertyEditorConfigCollection,
  UmbPropertyEditorUiElement,
  UmbPropertyValueChangeEvent,
} from "@umbraco-cms/backoffice/property-editor";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import {
  SchemaEntryViewModel,
  SchemaPropertyValue,
  SchemaTypeViewModel,
} from "../api";
import { MetaFieldsSchemaSource } from "../dataAccess/MetaFieldsSchemaSource";
import { SchemaEntrySource } from "../dataAccess/SchemaEntrySource";
import { SchemaPickerItem } from "../popups/SchemaPickerModal.element";
import type {
  SchemaSourceModalData,
  SchemaSourceModalResult,
} from "../popups/SchemaSourceModal.element";
import type {
  EditableSchema,
  PropertyValue,
} from "../popups/SchemaPropertyModal.element";

interface SchemaEditorValue {
  schemas: string[];
}

@customElement("st-schema-editor-propertyeditor")
export default class SchemaEditorPropertyEditor
  extends UmbElementMixin(LitElement)
  implements UmbPropertyEditorUiElement
{
  @property({ type: Object })
  public set value(value: SchemaEditorValue | undefined) {
    this._value = value ?? { schemas: [] };
    this.requestUpdate();
  }

  public get value(): SchemaEditorValue {
    return this._value;
  }

  private _value: SchemaEditorValue = { schemas: [] };

  @property({ attribute: false })
  public config?: UmbPropertyEditorConfigCollection;

  @state()
  private _schemaTypes: SchemaTypeViewModel[] = [];

  @state()
  private _loadedEntries: Map<string, SchemaEntryViewModel> = new Map();

  @state()
  private _docTypeEntries: SchemaEntryViewModel[] = [];

  #schemaSource?: MetaFieldsSchemaSource;
  #entrySource?: SchemaEntrySource;

  constructor() {
    super();
    this.#schemaSource = new MetaFieldsSchemaSource(this);
    this.#entrySource = new SchemaEntrySource(this);
    this.#loadSchemaTypes();
  }

  override updated(changedProperties: Map<string, unknown>) {
    if (changedProperties.has("value")) {
      this.#loadEntryDetails();
    }
    if (changedProperties.has("config")) {
      this.#loadDocTypeEntries();
    }
  }

  async #loadSchemaTypes() {
    this._schemaTypes = (await this.#schemaSource!.getSchemas()).data ?? [];
  }

  async #loadEntryDetails() {
    const guids = this._value.schemas.filter(
      (id) => !this._loadedEntries.has(id),
    );
    if (guids.length === 0) return;

    await Promise.all(
      guids.map(async (id) => {
        const result = await this.#entrySource!.getEntry(id);
        if (result.data) {
          this._loadedEntries = new Map(this._loadedEntries).set(
            id,
            result.data,
          );
        }
      }),
    );
    this.requestUpdate();
  }

  async #loadDocTypeEntries() {
    const docTypeKey = this.#getDocumentTypeKey();
    if (!docTypeKey || this.#getOwnerType() !== "content") {
      this._docTypeEntries = [];
      return;
    }
    const result = await this.#entrySource!.getEntries(
      "documentType",
      docTypeKey,
    );
    this._docTypeEntries = result.data ?? [];
  }

  #getNodeGuid(): string {
    const nodeGuid = this.config?.getValueByAlias<string>("nodeGuid") ?? "";
    if (!nodeGuid && this.#getOwnerType() === "documentType") {
      return this.config?.getValueByAlias<string>("documentTypeKey") ?? "";
    }
    return nodeGuid;
  }

  #getOwnerType(): string {
    return this.config?.getValueByAlias<string>("ownerType") ?? "content";
  }

  #getDocumentTypeKey(): string {
    return this.config?.getValueByAlias<string>("documentTypeKey") ?? "";
  }

  /** Returns the list of schema aliases that are allowed, or all schemas if not configured. */
  #getAllowedSchemaAliases(): string[] | null {
    const allowed = this.config?.getValueByAlias<string[]>("allowedSchemas");
    if (Array.isArray(allowed) && allowed.length > 0) return allowed;
    return null;
  }

  /** Filter the full schema type list down to only those permitted by config. */
  #getAvailableSchemas(): SchemaTypeViewModel[] {
    const allowed = this.#getAllowedSchemaAliases();
    if (!allowed) return this._schemaTypes;
    return this._schemaTypes.filter(
      (s) => s.alias != null && allowed.includes(s.alias),
    );
  }

  #getSchemaName(alias: string): string {
    const schema = this._schemaTypes.find((s) => s.alias === alias);
    return schema?.name ?? alias;
  }

  #getEntryDisplayName(id: string): string {
    const entry = this._loadedEntries.get(id);
    if (!entry) return id;
    return entry.displayName || this.#getSchemaName(entry.schemaAlias!);
  }

  async #openAddSchemaFlow() {
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (modalManager) => {
      if (!modalManager) return;

      const availableSchemas = this.#getAvailableSchemas();

      // Step 1: Pick schema type
      const availableSchemaItems: SchemaPickerItem[] = availableSchemas.map(
        (s) => ({
          alias: s.alias ?? "",
          name: s.name ?? s.alias ?? "",
        }),
      );

      const pickerModal = modalManager.open<
        { availableSchemas: SchemaPickerItem[] },
        string
      >(this, "seoToolkit.modal.schemaPicker", {
        modal: { type: "sidebar", size: "small" },
        data: { availableSchemas: availableSchemaItems },
        value: "",
      });

      const selectedAlias = await pickerModal.onSubmit().catch(() => null);
      if (!selectedAlias) return;

      // Step 2: Pick source (new vs existing)
      const sourceData: SchemaSourceModalData = {
        schemaAlias: selectedAlias,
        schemaTypeName: this.#getSchemaName(selectedAlias),
        ownerType: this.#getOwnerType(),
        ownerKey: this.#getNodeGuid(),
        documentTypeKey: this.#getDocumentTypeKey(),
      };

      const sourceModal = modalManager.open<
        SchemaSourceModalData,
        SchemaSourceModalResult
      >(this, "seoToolkit.modal.schemaSource", {
        modal: { type: "sidebar", size: "small" },
        data: sourceData,
        value: { mode: "new" },
      });

      const sourceResult = await sourceModal.onSubmit().catch(() => null);
      if (!sourceResult) return;

      if (sourceResult.mode === "existing") {
        // Add existing entry ID directly
        this._value = {
          schemas: [...this._value.schemas, sourceResult.entryId],
        };
        this.dispatchEvent(new UmbPropertyValueChangeEvent());
        return;
      }

      // Step 3: Configure new schema properties
      const schemaType = availableSchemas.find(
        (s) => s.alias === selectedAlias,
      );
      if (!schemaType) return;

      // Pre-generate the entry ID so nested schema editors (e.g. address in organization)
      // can use it as their ownerKey before the parent entry is persisted.
      const preGeneratedId = crypto.randomUUID();

      const propertyModal = modalManager.open<
        {
          availableSchemas: SchemaTypeViewModel[];
          editSchema?: EditableSchema;
          entryId?: string;
        },
        EditableSchema
      >(this, "seoToolkit.modal.schemaProperty", {
        modal: { type: "sidebar", size: "medium" },
        data: {
          availableSchemas: this._schemaTypes,
          editSchema: { schemaAlias: selectedAlias, properties: {} },
          entryId: preGeneratedId,
        },
        value: { schemaAlias: selectedAlias, properties: {} },
      });

      const schemaData = await propertyModal.onSubmit().catch(() => null);
      if (!schemaData) return;

      // POST to create the entry, using the pre-generated ID
      const result = await this.#entrySource!.createEntry({
        id: preGeneratedId,
        ownerType: this.#getOwnerType(),
        ownerKey: this.#getNodeGuid(),
        schemaAlias: schemaData.schemaAlias,
        displayName: schemaData.displayName || undefined,
        properties: schemaData.properties,
      });

      if (result.data) {
        this._loadedEntries = new Map(this._loadedEntries).set(
          result.data.id,
          result.data,
        );
        this._value = { schemas: [...this._value.schemas, result.data.id] };
        this.dispatchEvent(new UmbPropertyValueChangeEvent());
      }
    });
  }

  #toPropertyValues(
    apiProps:
      | { [key: string]: SchemaPropertyValue | null | undefined }
      | null
      | undefined,
  ): { [key: string]: PropertyValue } {
    if (!apiProps) return {};
    const result: { [key: string]: PropertyValue } = {};
    for (const [key, val] of Object.entries(apiProps)) {
      result[key] = {
        value: val?.value,
        isReference: val?.isReference ?? false,
        referenceKey: val?.referenceKey ?? "",
      };
    }
    return result;
  }

  async #openEditSchemaModal(entryId: string) {
    let entry = this._loadedEntries.get(entryId);
    if (!entry) {
      const result = await this.#entrySource!.getEntry(entryId);
      if (result.data) {
        this._loadedEntries = new Map(this._loadedEntries).set(
          entryId,
          result.data,
        );
        entry = result.data;
      }
      if (!entry) return;
    }

    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (modalManager) => {
      if (!modalManager) return;

      const editableProps = this.#toPropertyValues(entry.properties);

      const modal = modalManager.open<
        {
          availableSchemas: SchemaTypeViewModel[];
          editSchema?: EditableSchema;
          entryId?: string;
        },
        EditableSchema
      >(this, "seoToolkit.modal.schemaProperty", {
        modal: { type: "sidebar", size: "medium" },
        data: {
          availableSchemas: this._schemaTypes,
          editSchema: {
            schemaAlias: entry.schemaAlias!,
            displayName: entry.displayName ?? undefined,
            properties: editableProps,
          },
          entryId,
        },
        value: {
          schemaAlias: entry.schemaAlias!,
          displayName: entry.displayName ?? undefined,
          properties: editableProps,
        },
      });

      const schemaData = await modal.onSubmit().catch(() => null);
      if (!schemaData) return;

      // PUT to update the entry
      const result = await this.#entrySource!.updateEntry(entryId, {
        schemaAlias: schemaData.schemaAlias!,
        displayName: schemaData.displayName || undefined,
        properties: schemaData.properties!,
        ownerKey: this.#getNodeGuid(),
      });

      if (result.data) {
        this._loadedEntries = new Map(this._loadedEntries).set(
          entryId,
          result.data,
        );
        this.requestUpdate();
      }
    });
  }

  #removeSchema(id: string, e: Event) {
    e.stopPropagation();
    const schemas = this._value.schemas.filter((s) => s !== id);
    this._value = { schemas };
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  #renderSchemaList() {
    const hasInherited = this._docTypeEntries.length > 0;
    const hasOwn = this._value.schemas.length > 0;

    return html`
      ${when(
        hasInherited || hasOwn,
        () => html`
          <div class="schema-list">
            ${hasInherited
              ? html`
                  <span class="schema-info"
                    >Number of inherited schemas:
                    ${this._docTypeEntries.length}</span
                  >
                `
              : ""}
            ${hasOwn
              ? html`
                  ${hasInherited
                    ? html`<div class="schema-section-header">This item</div>`
                    : ""}
                  ${this._value.schemas.map(
                    (id) => html`
                      <div
                        class="schema-item"
                        @click="${() => this.#openEditSchemaModal(id)}"
                      >
                        <div class="schema-item-info">
                          <span class="schema-name"
                            >${this.#getEntryDisplayName(id)}</span
                          >
                        </div>
                        <div class="schema-item-actions">
                          <uui-button
                            color="danger"
                            @click="${(e: Event) => this.#removeSchema(id, e)}"
                          >
                            Remove
                          </uui-button>
                        </div>
                      </div>
                    `,
                  )}
                `
              : !hasInherited
                ? ""
                : ""}
          </div>
        `,
      )}
      <div class="add-button">
        <uui-button look="primary" @click="${() => this.#openAddSchemaFlow()}">
          Add Schema
        </uui-button>
      </div>
    `;
  }

  override render() {
    return this.#renderSchemaList();
  }

  static styles = [
    css`
      .schema-section-header {
        font-size: 0.8em;
        font-weight: 600;
        text-transform: uppercase;
        color: var(--uui-palette-grey-4);
        margin: 8px 0 4px;
        letter-spacing: 0.05em;
      }

      .schema-list {
        display: flex;
        flex-direction: column;
        gap: 8px;
        margin-bottom: 16px;
      }

      .schema-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 6px 12px;
        border: 1px solid var(--uui-palette-gravel);
        border-radius: 4px;
        cursor: pointer;
        background-color: var(--uui-palette-surface);
      }

      .schema-item:hover {
        background-color: var(--uui-palette-gravel-light);
      }

      .schema-item--inherited {
        cursor: default;
        opacity: 0.8;
        border-style: dashed;
      }

      .schema-item--inherited:hover {
        background-color: var(--uui-palette-surface);
      }

      .schema-item-info {
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .schema-name {
        font-weight: 500;
      }

      .schema-info {
        width: fit-content;
        font-size: 0.75em;
        padding: 4px 6px;
        border-radius: 10px;
        background-color: var(--uui-palette-gravel-light);
        color: var(--uui-palette-grey-4);
        border: 1px solid var(--uui-palette-gravel);
      }

      .add-button {
        margin-top: 8px;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-schema-editor-propertyeditor": SchemaEditorPropertyEditor;
  }
}
