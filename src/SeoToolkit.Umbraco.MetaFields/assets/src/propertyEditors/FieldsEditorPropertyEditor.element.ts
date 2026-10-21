import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement, nothing } from "lit";
import { MetaFieldsSettingsRepository } from "../dataAccess/MetaFieldsSettingsRepository";
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document-type";
import { UmbDataTypeDetailRepository } from "@umbraco-cms/backoffice/data-type";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import { ItemGroupPickerConfig } from "../popups/ItemGroupPicker.element";
import { UmbSorterController } from "@umbraco-cms/backoffice/sorter";
import {
  UmbPropertyEditorConfigCollection,
  UmbPropertyEditorUiElement,
  UmbPropertyValueChangeEvent,
} from "@umbraco-cms/backoffice/property-editor";

interface FieldData {
  name: string;
  value: string;
  onlyShowIfInherited: boolean;
  source: number;
  group: string;
}

// Field sources as stored in the value: properties of the document type, or fields provided by SEO Toolkit.
const SOURCE_SEO_TOOLKIT = 2;

interface FieldItem {
  name: string;
  value: string;
  source: number;
}

@customElement("st-fieldseditor-propertyeditor")
export default class FieldsEditorPropertyEditor
  extends UmbElementMixin(LitElement)
  implements UmbPropertyEditorUiElement
{
  #repository: MetaFieldsSettingsRepository;
  #dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);
  #sorter: UmbSorterController<string>;

  @property({ type: Array })
  public set value(items: FieldItem[] | undefined) {
    this._value = items ?? [];
    this.requestUpdate();

    if (this.#sorter) {
      this.#sorter.setModel(this._value.map((item) => item.value));
    }
  }
  public get value() {
    return this._value;
  }
  private _value: FieldItem[] = [];

  @state()
  additionalFields: FieldData[] = [];

  @state()
  contentTypeFields: FieldData[] = [];

  @state()
  dataTypesCache: { [key: string]: string } = {};

  @state()
  dataTypesToRender: string[] = [];

  constructor() {
    super();

    this.#sorter = new UmbSorterController<string>(this, {
      itemSelector: ".field-item",
      containerSelector: ".field-container",
      getUniqueOfElement: (element) => element.dataset.sortId,
      getUniqueOfModel: (model) => model,
      onChange: (model) => {
        const fields = this.getFields();
        this.value = model.model.map(
          (value) => fields.find((field) => field.value === value)!,
        );
        this.dispatchEvent(new UmbPropertyValueChangeEvent());
      },
    });

    this.#sorter.setModel(this._value.map((item) => item.value));

    this.#repository = new MetaFieldsSettingsRepository(this);

    this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
      this.observe(instance?.structure.contentTypes, (value) => {
        const contentTypeFields: FieldData[] = [];

        value?.forEach((field) => {
          field.properties.forEach((prop) => {
            if (this.dataTypesCache[prop.dataType.unique]) {
              if (
                !this.dataTypesToRender.includes(
                  this.dataTypesCache[prop.dataType.unique],
                )
              ) {
                return;
              } else {
                contentTypeFields.push({
                  name: prop.name,
                  value: prop.alias,
                  source: 1,
                  group:
                    field.containers.find(
                      (con) => con.id === prop.container!.id,
                    )?.name ?? "",
                  onlyShowIfInherited: false,
                });
              }
            } else {
              this.#dataTypeDetailRepository
                .requestByUnique(prop.dataType.unique)
                .then((dataType) => {
                  this.dataTypesCache[prop.dataType.unique] =
                    dataType.data!.editorAlias!;
                  if (
                    this.dataTypesToRender.includes(dataType.data!.editorAlias!)
                  ) {
                    contentTypeFields.push({
                      name: prop.name,
                      value: prop.alias,
                      source: 1,
                      group:
                        field.containers.find(
                          (con) => con.id === prop.container!.id,
                        )?.name ?? "",
                      onlyShowIfInherited: false,
                    });
                  }
                });
            }
          });
        });
        this.contentTypeFields = contentTypeFields;
      });
    });

    this.#repository.getAdditionalFields().then((resp) => {
      this.additionalFields = resp.data.map<FieldData>((item) => ({
        name: item.name!,
        value: item.value!,
        onlyShowIfInherited: item.onlyShowIfInherited,
        source: SOURCE_SEO_TOOLKIT,
        group: "SEO Toolkit",
      }));
    });
  }

  public set config(config: UmbPropertyEditorConfigCollection | undefined) {
    if (!config) {
      return;
    }

    this.dataTypesToRender =
      config.getValueByAlias<string[]>("dataTypes") ?? [];
  }

  getFields() {
    return [...this.additionalFields, ...this.contentTypeFields];
  }

  #getFieldData(value: string) {
    return this.getFields().find((field) => field.value === value);
  }

  #getIcon(source: number) {
    return source === SOURCE_SEO_TOOLKIT ? "icon-wand" : "icon-document";
  }

  async onItemsAddHandler() {
    const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
    if (!modalManager) {
      return;
    }

    const modal = modalManager.open<ItemGroupPickerConfig, string[]>(
      this,
      "seoToolkit.modal.itemGroupPicker",
      {
        modal: { type: "sidebar", size: "small" },
        data: {
          headline: "Choose fields",
          items: this.getFields().map((field) => ({
            name: field.name,
            value: field.value,
            group: field.group,
            icon: this.#getIcon(field.source),
          })),
        },
        value: this._value.map((item) => item.value),
      },
    );

    try {
      await modal.onSubmit();
    } catch {
      // Closed without choosing
      return;
    }

    const fields = this.getFields();
    this.value = modal
      .getValue()
      .map((value) => fields.find((field) => field.value === value)!)
      .filter((value) => value !== undefined);
    this.#sorter.setModel(this._value.map((item) => item.value));
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  #removeItem(item: FieldItem) {
    this.value = this._value.filter((field) => field.value !== item.value);
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  render() {
    return html`
      <div class="field-container">
        ${repeat(
          this._value,
          (item) => item.value,
          (item, index) => this.#renderItem(item, index),
        )}
      </div>
      ${this._value.length === 0
        ? html`<p class="empty">No fields chosen yet.</p>`
        : nothing}
      <uui-button
        class="add-button"
        look="placeholder"
        label="Add field"
        @click="${this.onItemsAddHandler}"
      >
        <uui-icon name="icon-add"></uui-icon>
        Add field
      </uui-button>
      <small class="control-description">
        Checked from top to bottom. The first field with a value is used.
      </small>
    `;
  }

  #renderItem(item: FieldItem, index: number) {
    const field = this.#getFieldData(item.value);

    return html`
      <div data-sort-id=${item.value} class="field-item">
        <uui-icon class="grip" name="icon-grip"></uui-icon>
        <span class="position">${index + 1}</span>
        <uui-icon name=${this.#getIcon(item.source)}></uui-icon>
        <span class="name">${field?.name ?? item.name}</span>
        ${field
          ? html`<span class="source">${field.group}</span>`
          : html`<uui-tag color="warning" look="secondary">Not found</uui-tag>`}
        <uui-button
          compact
          look="default"
          label="Remove ${item.name}"
          @click=${() => this.#removeItem(item)}
        >
          <uui-icon name="icon-wrong"></uui-icon>
        </uui-button>
      </div>
    `;
  }

  static styles = [
    css`
      .field-container {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-2);
        margin-bottom: var(--uui-size-space-2);
      }

      .field-item {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-3);
        padding: var(--uui-size-space-1) var(--uui-size-space-1) var(--uui-size-space-1) var(--uui-size-space-3);
        background: var(--uui-color-surface);
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-border-radius);
      }

      .grip {
        cursor: grab;
        color: var(--uui-color-text-alt);
      }

      .position {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: none;
        width: 20px;
        height: 20px;
        border-radius: 50%;
        font-size: 11px;
        font-weight: 700;
        background: var(--uui-color-surface-alt);
        border: 1px solid var(--uui-color-border);
      }

      .name {
        flex: 1;
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      .source {
        font-size: var(--uui-type-small-size);
        color: var(--uui-color-text-alt);
      }

      .empty {
        margin: 0 0 var(--uui-size-space-2);
        color: var(--uui-color-text-alt);
        font-style: italic;
      }

      .add-button {
        width: 100%;
      }

      .control-description {
        display: block;
        margin-top: var(--uui-size-space-2);
        color: var(--uui-color-text-alt);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-fieldseditor-propertyeditor": FieldsEditorPropertyEditor;
  }
}
