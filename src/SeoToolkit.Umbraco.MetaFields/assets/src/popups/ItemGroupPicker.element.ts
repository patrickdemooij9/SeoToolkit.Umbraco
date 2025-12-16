import { classMap, customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";
import { repeat } from "lit/directives/repeat.js";

export interface ItemGroupPickerConfig {
  items: ItemGroupPickerItem[];
}

interface ItemGroupPickerGroup {
  group: string;
  items: ItemGroupPickerItem[];
}

export interface ItemGroupPickerItem {
  name: string;
  value: string;
  group: string;
}

@customElement("st-item-group-picker-modal")
export default class ItemGroupPicker extends UmbModalBaseElement<
  ItemGroupPickerConfig,
  string[]
> {
  @state()
  groups: ItemGroupPickerGroup[] = [];

  override async connectedCallback() {
    super.connectedCallback();

    this.data?.items.forEach((item) => {
      let group = this.groups.find((group) => group.group === item.group);
      if (!group) {
        group = {
          group: item.group,
          items: [],
        };
        this.groups.push(group);
      }
      group.items.push(item);
    });
  }

  isSelected(item: ItemGroupPickerItem) {
    return this.value.includes(item.value);
  }

  clickItem(item: ItemGroupPickerItem) {
    if (this.value.includes(item.value)) {
      const copyValue = [...this.value];
      copyValue.splice(this.value.indexOf(item.value), 1);
      this.value = copyValue;
    } else {
      this.value = [...this.value, item.value];
    }
    this.requestUpdate();
  }

  #handleClose() {
    this.modalContext?.reject();
  }

  #handleSubmit() {
    this.modalContext?.submit();
  }

  override render() {
    return html`
      <umb-body-layout headline="Select items">
        ${repeat(
          this.groups,
          (group) => group.group,
          (group) => html`
            <uui-box class="select-group" headline=${group.group}>
              ${repeat(
                group.items,
                (item) => item.name,
                (item) =>
                  html`
                    <div
                      class=${classMap({
                        "select-item": true,
                        selected: this.isSelected(item),
                      })}
                      @click="${() => this.clickItem(item)}"
                    >
                      ${item.name}
                    </div>
                  `
              )}
            </uui-box>
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
            label="Submit"
            look="primary"
            color="positive"
            slot="actions"
            @click="${this.#handleSubmit}"
            >Submit</uui-button
          >
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  static styles = [
    css`
      .select-group {
        margin-top: 12px;
      }

      .select-item {
        padding: 12px 20px;
        cursor: pointer;
        border-radius: 8px;
        margin-bottom: 4px;

        &.selected,
        &:hover {
          background-color: var(--uui-palette-gravel);
        }
      }
    `,
  ];
}
