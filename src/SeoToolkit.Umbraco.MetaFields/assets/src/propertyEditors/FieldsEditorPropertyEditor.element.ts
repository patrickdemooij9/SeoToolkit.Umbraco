import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
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
        source: 2,
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

  onItemsAddHandler() {
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (instance) => {
      if (!instance) {
        return;
      }
      const modal = instance.open<ItemGroupPickerConfig, string[]>(
        this,
        "seoToolkit.modal.itemGroupPicker",
        {
          modal: { type: "sidebar", size: "medium" },
          data: {
            items: this.getFields(),
          },
          value: this.value!.map((item) => item.value) ?? [],
        },
      );
      await modal.onSubmit();
      const fields = this.getFields();
      this.value = modal
        .getValue()
        .map((value) => fields.find((field) => field.value === value)!)
        .filter((value) => value !== undefined);
      this.#sorter.setModel(this.value.map((item) => item.value));
      this.dispatchEvent(new UmbPropertyValueChangeEvent());
    });
  }

  render() {
    return html`
      <div class="field-container">
        ${repeat(
          this.value ?? [],
          (item) => item.value,
          (item) => html`
            <div data-sort-id=${item.value} class="field-item">
              ${item.name}
            </div>
          `,
        )}
      </div>
      <div>
        <uui-button
          class="add-button"
          look="placeholder"
          label="Add fields"
          @click="${this.onItemsAddHandler}"
        >
          Manage fields
        </uui-button>
      </div>
      <small class="control-description">
        Fields to determine the meta field will be checked from top to bottom.
      </small>
    `;
  }

  static styles = [
    css`
      .add-button {
        width: 100%;
      }
      .field-item {
        border: 1px solid gray;
        border-radius: 4px;
        margin-bottom: 4px;
        padding: 8px 6px;
        text-align: center;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-fieldseditor-propertyeditor": FieldsEditorPropertyEditor;
  }
}
