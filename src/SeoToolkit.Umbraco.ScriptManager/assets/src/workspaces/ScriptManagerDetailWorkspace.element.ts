import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { umbFocus, UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { css, html } from "lit";

@customElement("seotoolkit-scriptmanager-detail")
export class ScriptManagerDetailWorkspace extends UmbLitElement {
    @state()
	private _name?: string = '';

    #onNameInput(_event: Event) {
		//const target = event.target as UUIInputElement;
		//const value = target.value as string;
		//this.#context?.setName(value);
	}

    override render(){
        return html`
            <umb-workspace-editor
				alias="seoToolkit.scriptManager.detail"
				back-path="section/SeoToolkit/workspace/seoToolkit-scriptManager/edit/94e95f4a-2ecb-4038-bcfd-8357b7c41f1a">
                <div id="workspace-header" slot="header">
					<uui-input
						placeholder=${this.localize.term('placeholders_entername')}
						.value=${this._name}
						@input=${this.#onNameInput}
						label=${this.localize.term('placeholders_entername')}
						${umbFocus()}>
					</uui-input>
				</div>
            </umb-workspace-editor>
        `
    }

    static styles = [
        css`
            #workspace-header {
                width: 100%;
            }

            uui-input {
				width: 100%;
			}
        `
    ]
}

export default ScriptManagerDetailWorkspace;