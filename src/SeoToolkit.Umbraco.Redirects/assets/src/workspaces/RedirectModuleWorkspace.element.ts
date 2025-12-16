import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement } from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";

@customElement('seotoolkit-module-redirect')
export class RedirectModuleWorkspace extends UmbElementMixin(LitElement) {

    render() {
        return html`
            <umb-body-layout main-no-padding headline='Redirects'>
                <umb-collection alias='seoToolkit.collections.redirects'></umb-collection>
            </umb-body-layout>
        `
    }
}

export default RedirectModuleWorkspace;