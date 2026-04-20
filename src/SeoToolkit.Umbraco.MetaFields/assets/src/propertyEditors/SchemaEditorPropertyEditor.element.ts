import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import {
  UmbPropertyEditorUiElement,
  UmbPropertyValueChangeEvent,
} from "@umbraco-cms/backoffice/property-editor";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import { SchemaTypeViewModel } from "../api";
import { MetaFieldsSchemaSource } from "../dataAccess/MetaFieldsSchemaSource";
import { SchemaPickerItem } from "../popups/SchemaPickerModal.element";

interface PropertyValue {
  value: string;
  isReference: boolean;
  referenceKey: string;
}

interface SchemaItemValue {
  schemaAlias: string;
  properties: { [key: string]: PropertyValue };
}

interface SchemaEditorValue {
  schemas: SchemaItemValue[];
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

  @state()
  private _schemaTypes: SchemaTypeViewModel[] = [];

  constructor() {
    super();
    this.#loadSchemaTypes();
  }

  async #loadSchemaTypes() {
    this._schemaTypes = (await new MetaFieldsSchemaSource(this).getSchemas()).data ?? [];
  }

  #getSchemaName(alias: string): string {
    const schema = this._schemaTypes.find((s) => s.alias === alias);
    return schema?.name ?? alias;
  }

  async #openSchemaModal(editIndex?: number) {
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (modalManager) => {
      if (!modalManager) return;

      const editSchema =
        editIndex !== undefined
          ? this._value.schemas[editIndex]
          : await this.#pickSchemaType(modalManager);

      if (!editSchema) {
        return;
      }

      const modal = modalManager.open<
        {
          availableSchemas: SchemaTypeViewModel[];
          editSchema?: SchemaItemValue;
        },
        SchemaItemValue
      >(this, "seoToolkit.modal.schemaProperty", {
        modal: { type: "sidebar", size: "medium" },
        data: {
          availableSchemas: this._schemaTypes,
          editSchema: editSchema,
        },
        value: editSchema,
      });

      const result = await modal.onSubmit();
      if (result) {
        if (editIndex !== undefined) {
          const schemas = [...this._value.schemas];
          schemas[editIndex] = result;
          this._value = { schemas };
        } else {
          this._value = {
            schemas: [...this._value.schemas, result],
          };
        }
        this.dispatchEvent(new UmbPropertyValueChangeEvent());
      }
    });
  }

  async #pickSchemaType(modalManager: any) {
    const availableSchemas: SchemaPickerItem[] = this._schemaTypes.map((schema) => ({
      alias: schema.alias ?? "",
      name: schema.name ?? schema.alias ?? "",
    }));

    const modal = modalManager.open<
      { availableSchemas: SchemaPickerItem[] },
      string
    >(this, "seoToolkit.modal.schemaPicker", {
      modal: { type: "sidebar", size: "small" },
      data: {
        availableSchemas,
      },
      value: "",
    });

    const selectedAlias = await modal.onSubmit();
    if (!selectedAlias) {
      return undefined;
    }

    return {
      schemaAlias: selectedAlias,
      properties: {},
    } as SchemaItemValue;
  }

  #removeSchema(index: number, e: Event) {
    e.stopPropagation();
    const schemas = [...this._value.schemas];
    schemas.splice(index, 1);
    this._value = { schemas };
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  #renderSchemaList() {
    return html`
      <div class="schema-list">
        ${this._value.schemas.length === 0
          ? html`<p class="no-schemas">No schemas added yet. Click "Add Schema" to get started.</p>`
          : this._value.schemas.map(
              (schema, index) => html`
                <div class="schema-item" @click="${() => this.#openSchemaModal(index)}">
                  <div class="schema-item-info">
                    <span class="schema-name">${this.#getSchemaName(schema.schemaAlias)}</span>
                  </div>
                  <div class="schema-item-actions">
                    <uui-button
                      color="danger"
                      @click="${(e: Event) => this.#removeSchema(index, e)}"
                    >
                      Remove
                    </uui-button>
                  </div>
                </div>
              `
            )}
      </div>
      <div class="add-button">
        <uui-button look="primary" @click="${() => this.#openSchemaModal()}">
          Add Schema
        </uui-button>
      </div>
    `;
  }

  override render() {
    return html` <div class="schema-editor-container">${this.#renderSchemaList()}</div> `;
  }

  static styles = [
    css`
      .schema-editor-container {
        min-height: 200px;
      }

      .no-schemas {
        color: var(--uui-palette-grey-3);
        font-style: italic;
        padding: 16px;
        text-align: center;
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
        padding: 12px;
        border: 1px solid var(--uui-palette-gravel);
        border-radius: 4px;
        cursor: pointer;
        background-color: var(--uui-palette-surface);
      }

      .schema-item:hover {
        background-color: var(--uui-palette-gravel-light);
      }

      .schema-name {
        font-weight: 500;
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
