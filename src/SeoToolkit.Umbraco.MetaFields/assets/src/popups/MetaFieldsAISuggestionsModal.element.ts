import { customElement, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { css, html } from "lit";
import type { MetaFieldsAIFieldSuggestion } from "../dataAccess/MetaFieldsAISource";

export interface MetaFieldsAISuggestionsModalConfig {
  suggestions: MetaFieldsAIFieldSuggestion[];
}

/** The value returned when the modal is submitted: the subset of suggestions the editor wants to apply */
export type MetaFieldsAISuggestionsModalValue = MetaFieldsAIFieldSuggestion[];

interface SuggestionToggle extends MetaFieldsAIFieldSuggestion {
  enabled: boolean;
}

@customElement("st-metafields-ai-suggestions-modal")
export default class MetaFieldsAISuggestionsModal extends UmbModalBaseElement<
  MetaFieldsAISuggestionsModalConfig,
  MetaFieldsAISuggestionsModalValue
> {
  @state()
  _items: SuggestionToggle[] = [];

  override connectedCallback() {
    super.connectedCallback();
    this._items = (this.data?.suggestions ?? []).map((s) => ({
      ...s,
      enabled: true,
    }));
  }

  #toggle(alias: string) {
    this._items = this._items.map((item) =>
      item.alias === alias ? { ...item, enabled: !item.enabled } : item
    );
  }

  #handleClose() {
    this.modalContext?.reject();
  }

  #handleApply() {
    this.value = this._items
      .filter((item) => item.enabled)
      .map(({ alias, value }) => ({ alias, value }));
    this.modalContext?.submit();
  }

  #labelForAlias(alias: string): string {
    const labels: Record<string, string> = {
      title: "Page Title",
      metaDescription: "Meta Description",
      openGraphTitle: "OG Title",
      openGraphDescription: "OG Description",
    };
    return labels[alias] ?? alias;
  }

  override render() {
    return html`
      <umb-body-layout headline="AI-Generated SEO Suggestions">
        <p class="intro">
          Review the AI-generated suggestions below. Toggle off any fields you
          do not want to apply, then click <strong>Apply</strong>.
        </p>

        ${repeat(
          this._items,
          (item) => item.alias,
          (item) => html`
            <uui-box class="suggestion-item">
              <div class="suggestion-header" slot="headline">
                <uui-toggle
                  .checked=${item.enabled}
                  @change=${() => this.#toggle(item.alias)}
                ></uui-toggle>
                <span class="field-label">${this.#labelForAlias(item.alias)}</span>
              </div>
              <p class="suggestion-value">${item.value}</p>
            </uui-box>
          `
        )}

        <umb-footer-layout slot="footer">
          <uui-button
            slot="actions"
            label="Cancel"
            look="secondary"
            @click=${this.#handleClose}
          >Cancel</uui-button>
          <uui-button
            slot="actions"
            label="Apply selected"
            look="primary"
            color="positive"
            @click=${this.#handleApply}
          >Apply</uui-button>
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  static styles = [
    css`
      .intro {
        margin: 0 0 16px;
        color: var(--uui-color-text, #333);
      }

      .suggestion-item {
        margin-bottom: 12px;
      }

      .suggestion-header {
        display: flex;
        align-items: center;
        gap: 10px;
      }

      .field-label {
        font-weight: bold;
      }

      .suggestion-value {
        margin: 8px 0 0;
        color: var(--uui-color-text-alt, #555);
        white-space: pre-wrap;
        word-break: break-word;
      }
    `,
  ];
}
