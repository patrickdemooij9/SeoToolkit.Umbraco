import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { html, LitElement } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('seotoolkit-module-scriptmanager')
export class ScriptManagerModuleWorkspace extends UmbElementMixin(LitElement) {

    render() {
        return html`
            <umb-body-layout main-no-padding headline='Scripts'>
                <umb-collection alias='seoToolkit.collections.scripts'></umb-collection>;
            </umb-body-layout>
        `
    }
}

export default ScriptManagerModuleWorkspace;