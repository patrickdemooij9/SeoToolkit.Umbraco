import { UmbWorkspaceViewElement } from "@umbraco-cms/backoffice/extension-registry";
import { customElement } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { html } from "lit";

@customElement("script-manager-edit")
export class ScriptManagerEdit extends UmbLitElement implements UmbWorkspaceViewElement {
    override render(){
        return html`
            <h1>Hello world</h1>
        `
    }
}

export default ScriptManagerEdit;