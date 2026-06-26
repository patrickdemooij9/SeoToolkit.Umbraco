import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import {
  UmbPropertyEditorConfigCollection,
  UmbPropertyEditorUiElement
} from "@umbraco-cms/backoffice/property-editor";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";

interface CheckboxItem {
  label: string;
  value: string | undefined;
  checked: boolean;
}

type UmbInputCheckboxListElement = {
  selection: string[];
};

@customElement("st-selectheckbox-propertyeditor")
export default class SelectCheckboxList
  extends UmbElementMixin(LitElement)
  implements UmbPropertyEditorUiElement
{
  @property({ type: Array })
  public value: string[];

  @state()
  public list: CheckboxItem[] = [];

  constructor() {
    super();

    this.value = [];
  }

  public set config(config: UmbPropertyEditorConfigCollection | undefined) {
    if (!config) {
      return;
    }

    const items = config.getValueByAlias<CheckboxItem[]>("items");
    if (!items) {
      return;
    }

    this.list = [...items];

    if (config.getValueByAlias<boolean>("includeNone")) {
      this.list.push({
        label: "None",
        value: 'none',
        checked: false,
      });
    }
  }

  getList() {
    if (!this.list) {
      return [];
    }
    return this.list.map((item) => ({
      ...item,
      checked: this.value?.includes(item.value!),
    }));
  }

  #onChange(event: CustomEvent & { target: UmbInputCheckboxListElement }) {
    let newValue = event.target.selection;
    if (this.value && this.value.length < newValue.length && newValue.includes('none')) {
      const newestValueAdded = newValue.find((item) => !this.value.includes(item));
      if (newestValueAdded === 'none'){
        newValue = ['none'];
      }else{
        newValue = newValue.filter((item) => item !== 'none');
      }
    }

    this.value = newValue;
    this.dispatchEvent(new UmbChangeEvent());
  }

  override render() {
    return html`
      <umb-input-checkbox-list
        .list=${this.getList()}
        .selection=${this.value ?? []}
        @change=${this.#onChange}
      ></umb-input-checkbox-list>
    `;
  }
}
