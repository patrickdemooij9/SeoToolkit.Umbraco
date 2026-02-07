import {
  customElement,
  property,
  css,
  html,
  nothing,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

@customElement("st-redirect-name-column-layout")
export class RedirectNameLayout extends UmbLitElement {
  @property({ attribute: false })
  value?: {
    unique: string;
    name: string;
	url: string;
  };

  #clickItem(event: Event) {
    event.stopPropagation();
    this.consumeContext(ST_REDIRECT_MODULE_TOKEN_CONTEXT, (instance) => {
      instance?.openCreateModal(this.value?.unique);
    });
  }

  override render() {
    if (!this.value) return nothing;

    return html`<div class="redirect-name-layout">
      <a @click=${this.#clickItem}>${this.value.name}</a>
      <a href="${this.value.url}" target="_blank" class="quick-link">
        <uui-icon name="icon-log-out"></uui-icon>
      </a>
    </div>`;
  }

  static override styles = [
    css`
      :host {
        white-space: nowrap;
      }

      a {
        color: var(--uui-color-interactive);
      }

      a:hover {
        color: var(--uui-color-interactive-emphasis);
      }

	  .redirect-name-layout {
		&:hover {
			.quick-link {
				visibility: visible;
			}
		}
	  }

      .quick-link {
		visibility: hidden;

        margin-left: 8px;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-redirect-name-column-layout": RedirectNameLayout;
  }
}
