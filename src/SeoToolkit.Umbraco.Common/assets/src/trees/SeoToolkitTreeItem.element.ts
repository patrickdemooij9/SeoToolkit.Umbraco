import { UmbTreeItemContext, UmbTreeItemElementBase } from "@umbraco-cms/backoffice/tree";
import { SeoToolkitTreeItemModel } from "./types";
import { css, html } from "lit";
import { customElement, property } from "lit/decorators.js";

@customElement("seotoolkit-tree-item")
export default class SeoToolkitTreeItemElement extends UmbTreeItemElementBase<SeoToolkitTreeItemModel> {

    #api: UmbTreeItemContext<SeoToolkitTreeItemModel> | undefined;

    @property({ type: Object, attribute: false })
    public override get api(): UmbTreeItemContext<SeoToolkitTreeItemModel> | undefined {
        return this.#api;
    }
    public override set api(value: UmbTreeItemContext<SeoToolkitTreeItemModel> | undefined) {
        this.#api = value;

        if (this.#api) {
            this.observe(this.#api.treeItem, (isDraft) => (this._isDraft = isDraft?.isDraft || false));
        }

        super.api = value;
    }

    @property({ type: Boolean, reflect: true, attribute: 'draft' })
    protected _isDraft = this.item?.isDraft;

    override renderLabel() {
        return html`<span id="label" slot="label">${this.item?.name}</span> `;
    }

    static override styles = [
        css`
            :host([draft]) #label {
				opacity: 0.6;
			}
			:host([draft]) umb-icon {
				opacity: 0.6;
			}
        `
    ]
}