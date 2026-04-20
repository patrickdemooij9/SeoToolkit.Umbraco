import { classMap, customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";
import { SchemaEntryViewModel } from "../api/types.gen";
import { SchemaEntrySource } from "../dataAccess/SchemaEntrySource";

export interface SchemaSourceModalData {
  schemaAlias: string;
  ownerType: string;
  ownerKey: string;
  documentTypeKey?: string;
}

export type SchemaSourceModalResult =
  | { mode: "new" }
  | { mode: "existing"; entryId: string };

@customElement("st-schema-source-modal")
export default class SchemaSourceModal extends UmbModalBaseElement<
  SchemaSourceModalData,
  SchemaSourceModalResult
> {
  @state()
  private _activeTab: "new" | "existing" = "new";

  @state()
  private _ownEntries: SchemaEntryViewModel[] = [];

  @state()
  private _reusableEntries: SchemaEntryViewModel[] = [];

  @state()
  private _loading = false;

  #source?: SchemaEntrySource;

  override connectedCallback() {
    super.connectedCallback();
    this.#source = new SchemaEntrySource(this);
    this.#loadEntries();
  }

  async #loadEntries() {
    if (!this.data) return;
    this._loading = true;
    try {
      const { ownerType, ownerKey, documentTypeKey, schemaAlias } = this.data;

      const [ownResult, reusableResult] = await Promise.all([
        this.#source!.getEntries(ownerType, ownerKey),
        documentTypeKey
          ? this.#source!.getEntries("documentType", documentTypeKey)
          : Promise.resolve({ data: [] as SchemaEntryViewModel[] }),
      ]);

      const allOwn = (ownResult.data ?? []).filter(
        (e) => e.schemaAlias === schemaAlias
      );
      const allReusable = (reusableResult.data ?? []).filter(
        (e) => e.schemaAlias === schemaAlias
      );

      this._ownEntries = allOwn;
      this._reusableEntries = allReusable;

      if (allOwn.length === 0 && allReusable.length === 0) {
        this._activeTab = "new";
      }
    } finally {
      this._loading = false;
    }
  }

  #handleClose() {
    this.modalContext?.reject();
  }

  #handleSelectNew() {
    this.value = { mode: "new" };
    this.modalContext?.submit();
  }

  #handleSelectExisting(entryId: string) {
    this.value = { mode: "existing", entryId };
    this.modalContext?.submit();
  }

  #renderExistingList(entries: SchemaEntryViewModel[]) {
    if (entries.length === 0) {
      return html`<p class="no-items">No existing entries found.</p>`;
    }
    return html`
      <div class="entry-list">
        ${entries.map(
          (entry) => html`
            <div
              class="entry-item"
              @click="${() => this.#handleSelectExisting(entry.id)}"
            >
              <span class="entry-id">${entry.id}</span>
            </div>
          `
        )}
      </div>
    `;
  }

  override render() {
    return html`
      <umb-body-layout headline="Add Schema">
        ${this._loading
          ? html`<p>Loading...</p>`
          : html`
              <div class="tabs">
                <button
                  class=${classMap({ tab: true, active: this._activeTab === "new" })}
                  @click="${() => (this._activeTab = "new")}"
                >
                  Create New
                </button>
                <button
                  class=${classMap({ tab: true, active: this._activeTab === "existing" })}
                  @click="${() => (this._activeTab = "existing")}"
                >
                  Pick Existing (${this._ownEntries.length + this._reusableEntries.length})
                </button>
              </div>

              ${this._activeTab === "new"
                ? html`
                    <div class="tab-content">
                      <p>Configure a new schema entry for this page.</p>
                      <uui-button
                        look="primary"
                        color="positive"
                        @click="${this.#handleSelectNew}"
                      >
                        Configure New Schema
                      </uui-button>
                    </div>
                  `
                : html`
                    <div class="tab-content">
                      ${this._ownEntries.length > 0
                        ? html`
                            <h4>This Page's Entries</h4>
                            ${this.#renderExistingList(this._ownEntries)}
                          `
                        : ""}
                      ${this._reusableEntries.length > 0
                        ? html`
                            <h4>From Document Type</h4>
                            ${this.#renderExistingList(this._reusableEntries)}
                          `
                        : ""}
                      ${this._ownEntries.length === 0 && this._reusableEntries.length === 0
                        ? html`<p class="no-items">No existing entries found for this schema type.</p>`
                        : ""}
                    </div>
                  `}
            `}

        <umb-footer-layout slot="footer">
          <uui-button
            label="Cancel"
            look="primary"
            color="danger"
            slot="actions"
            @click="${this.#handleClose}"
            >Cancel</uui-button
          >
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  static styles = [
    css`
      .tabs {
        display: flex;
        gap: 4px;
        margin-bottom: 16px;
        border-bottom: 1px solid var(--uui-palette-gravel);
        padding-bottom: 4px;
      }

      .tab {
        padding: 8px 16px;
        border: none;
        background: none;
        cursor: pointer;
        border-radius: 4px 4px 0 0;
        font-size: 14px;
      }

      .tab.active {
        background-color: var(--uui-palette-gravel-light);
        font-weight: 600;
      }

      .tab-content {
        padding: 8px 0;
      }

      .entry-list {
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .entry-item {
        padding: 12px;
        border: 1px solid var(--uui-palette-gravel);
        border-radius: 4px;
        cursor: pointer;
        background-color: var(--uui-palette-surface);
      }

      .entry-item:hover {
        background-color: var(--uui-palette-gravel-light);
      }

      .entry-id {
        font-family: monospace;
        font-size: 0.85em;
        color: var(--uui-palette-grey-4);
      }

      .no-items {
        color: var(--uui-palette-grey-3);
        font-style: italic;
      }

      h4 {
        margin: 8px 0 4px;
        font-size: 0.9em;
        text-transform: uppercase;
        color: var(--uui-palette-grey-4);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-schema-source-modal": SchemaSourceModal;
  }
}
