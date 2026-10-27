import { customElement, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { UUIInputEvent } from "@umbraco-cms/backoffice/external/uui";
import { css, html, nothing } from "lit";

export interface ItemGroupPickerConfig {
  headline?: string;
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
  icon?: string;
}

@customElement("st-item-group-picker-modal")
export default class ItemGroupPicker extends UmbModalBaseElement<
  ItemGroupPickerConfig,
  string[]
> {
  @state()
  _search = "";

  get #groups(): ItemGroupPickerGroup[] {
    const search = this._search.trim().toLowerCase();
    const groups: ItemGroupPickerGroup[] = [];

    this.data?.items.forEach((item) => {
      if (
        search &&
        !item.name.toLowerCase().includes(search) &&
        !item.group.toLowerCase().includes(search)
      ) {
        return;
      }

      let group = groups.find((group) => group.group === item.group);
      if (!group) {
        group = { group: item.group, items: [] };
        groups.push(group);
      }
      group.items.push(item);
    });

    return groups;
  }

  clickItem(item: ItemGroupPickerItem) {
    // The order of the value is the order the fields are checked in, so new picks go last.
    this.value = this.value.includes(item.value)
      ? this.value.filter((value) => value !== item.value)
      : [...this.value, item.value];
  }

  #onSearch(e: UUIInputEvent) {
    this._search = e.target.value as string;
  }

  #handleClose() {
    this.modalContext?.reject();
  }

  #handleSubmit() {
    this.modalContext?.submit();
  }

  override render() {
    const groups = this.#groups;

    return html`
      <umb-body-layout headline=${this.data?.headline ?? "Choose fields"}>
        <uui-input
          class="search"
          label="Search fields"
          placeholder="Search fields"
          .value=${this._search}
          @input=${this.#onSearch}
        >
          <uui-icon name="icon-search" slot="prepend"></uui-icon>
        </uui-input>
        <p class="hint">
          Fields are checked in the order you pick them. You can reorder them
          afterwards.
        </p>

        ${groups.length === 0
          ? html`<p class="empty">No fields match "${this._search}".</p>`
          : nothing}
        ${repeat(
          groups,
          (group) => group.group,
          (group) => html`
            <section class="group">
              <h4 class="group-name">${group.group || "Other"}</h4>
              ${repeat(
                group.items,
                (item) => item.value,
                (item) => this.#renderItem(item),
              )}
            </section>
          `,
        )}

        <umb-footer-layout slot="footer">
          <uui-button
            label="Cancel"
            slot="actions"
            @click="${this.#handleClose}"
          ></uui-button>
          <uui-button
            label="Choose${this.value.length ? ` (${this.value.length})` : ""}"
            look="primary"
            color="positive"
            slot="actions"
            @click="${this.#handleSubmit}"
          ></uui-button>
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  #renderItem(item: ItemGroupPickerItem) {
    const position = this.value.indexOf(item.value);
    const selected = position !== -1;

    return html`
      <button
        class="item ${selected ? "selected" : ""}"
        role="checkbox"
        aria-checked=${selected ? "true" : "false"}
        @click=${() => this.clickItem(item)}
      >
        <span class="check">${selected ? position + 1 : nothing}</span>
        ${item.icon ? html`<uui-icon name=${item.icon}></uui-icon>` : nothing}
        <span class="name">${item.name}</span>
      </button>
    `;
  }

  static styles = [
    css`
      .search {
        width: 100%;
      }

      .search uui-icon {
        margin-left: var(--uui-size-space-3);
        color: var(--uui-color-text-alt);
      }

      .hint,
      .empty {
        margin: var(--uui-size-space-3) 0 0;
        color: var(--uui-color-text-alt);
        font-size: var(--uui-type-small-size);
      }

      .group {
        margin-top: var(--uui-size-space-5);
      }

      .group-name {
        margin: 0 0 var(--uui-size-space-2);
        font-size: var(--uui-type-small-size);
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--uui-color-text-alt);
      }

      .item {
        all: unset;
        box-sizing: border-box;
        cursor: pointer;
        width: 100%;
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-3);
        padding: var(--uui-size-space-3) var(--uui-size-space-4);
        border-radius: var(--uui-border-radius);
      }

      .item:hover {
        background: var(--uui-color-surface-emphasis);
      }

      .item:focus-visible {
        outline: 2px solid var(--uui-color-focus);
      }

      .item.selected {
        background: var(--uui-color-selected);
        color: var(--uui-color-selected-contrast);
      }

      .check {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: none;
        width: 18px;
        height: 18px;
        border-radius: 4px;
        border: 1px solid var(--uui-color-border-emphasis);
        background: var(--uui-color-surface);
        color: var(--uui-color-text);
        font-size: 11px;
        font-weight: 700;
      }

      .item.selected .check {
        border-color: var(--uui-color-selected-contrast);
      }

      .name {
        flex: 1;
      }
    `,
  ];
}
