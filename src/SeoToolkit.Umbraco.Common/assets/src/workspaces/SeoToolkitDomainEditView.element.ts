import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbWorkspaceViewElement } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitDomainContext, { ST_DOMAIN_DETAIL_TOKEN_CONTEXT } from "./SeoToolkitDomainContext";
import { html } from "lit";
import { customElement } from "@umbraco-cms/backoffice/external/lit";

@customElement("seotoolkit-domain-edit-view")
export class SeoToolkitDomainEditViewElement extends UmbLitElement implements UmbWorkspaceViewElement {

    #context?: SeoToolkitDomainContext;

    constructor(){
        super();

        this.consumeContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT, (instance) => {
            if (!instance) {
                return;
            }
            this.#context = instance;

            console.log(this.#context);
        });
    }

    override render() {
        return html`
            <div>
                <uui-box>
                    Domain Edit View
                </uui-box>
            </div>
        `
    }
}