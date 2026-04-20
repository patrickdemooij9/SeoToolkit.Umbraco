import { classMap, customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";

export interface SchemaPickerConfig {
  availableSchemas: SchemaPickerItem[];
}

export interface SchemaPickerItem {
  alias: string;
  name: string;
}

@customElement("st-schema-picker-modal")
export default class SchemaPickerModal extends UmbModalBaseElement<
  SchemaPickerConfig,
  string
> {
  @state()
  selectedAlias: string = "";

  override connectedCallback() {
    super.connectedCallback();
    this.value = this.value ?? "";
    this.selectedAlias = this.value;
  }

  clickItem(item: SchemaPickerItem) {
    this.selectedAlias = item.alias;
    this.value = item.alias;
    this.requestUpdate();
  }

  #handleClose() {
    this.modalContext?.reject();
  }

  #handleSubmit() {
    if (!this.selectedAlias) {
      return;
    }
    this.modalContext?.submit();
  }

  override render() {
    return html`
      <umb-body-layout headline="Select Schema Type">
        ${(this.data?.availableSchemas ?? []).map(
          (item) => html`
            <div
              class=${classMap({
                "select-item": true,
                selected: this.selectedAlias === item.alias,
              })}
              @click="${() => this.clickItem(item)}"
            >
              ${item.name}
            </div>
          `
        )}

        <umb-footer-layout slot="footer">
          <uui-button
            id="close"
            label="Close"
            look="primary"
            color="danger"
            slot="actions"
            @click="${this.#handleClose}"
            >Close</uui-button
          >
          <uui-button
            id="save"
            label="Select"
            look="primary"
            color="positive"
            slot="actions"
            @click="${this.#handleSubmit}"
            >Select</uui-button
          >
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  static styles = [
    css`
      .select-item {
        padding: 12px 20px;
        cursor: pointer;
        border-radius: 8px;
        margin-bottom: 8px;
        border: 1px solid var(--uui-palette-gravel);
      }      

      .select-item.selected,
      .select-item:hover {
        background-color: var(--uui-palette-gravel-light);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-schema-picker-modal": SchemaPickerModal;
  }
}
