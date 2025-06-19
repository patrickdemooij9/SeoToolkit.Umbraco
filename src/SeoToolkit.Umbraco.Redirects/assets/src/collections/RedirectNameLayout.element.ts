import { customElement, property, css, html, nothing } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

@customElement("st-redirect-name-column-layout")
export class RedirectNameLayout extends UmbLitElement {
	@property({ attribute: false })
	value?: { unique: string; name: string, callback: (event: Event, unique: string) => void };

	#clickItem(event: Event) {
		event.stopPropagation();
		this.consumeContext(ST_REDIRECT_MODULE_TOKEN_CONTEXT, (instance) => {
			instance?.openCreateModal(this.value?.unique);
		})
	}

	override render() {
		if (!this.value) return nothing;

		return html`<a @click=${this.#clickItem}>${this.value.name}</a>`;
	}

	static override styles = [
		css`
			:host {
				white-space: nowrap;
			}

            a {
                color: var(--uui-color-interactive);
            }

            a:hover{
                color: var(--uui-color-interactive-emphasis);
            }
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'st-redirect-name-column-layout': RedirectNameLayout;
	}
}