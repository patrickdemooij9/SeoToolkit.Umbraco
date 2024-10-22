import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { UUIInputElement } from "@umbraco-cms/backoffice/external/uui";
import { umbFocus, UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { css, html } from "lit";
import ScriptManagerDetailContext, { ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT } from "./ScriptManagerDetailContext";

@customElement("seotoolkit-scriptmanager-detail")
export class ScriptManagerDetailWorkspace extends UmbLitElement {

    #context?: ScriptManagerDetailContext;

    @state()
	private _name?: string = '';

    constructor(){
        super();

        this.consumeContext(ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT, (instance) => {
            this.#context = instance;

            this.observe(this.#context.script, (value) => {
                this._name = value.name!;
            })
        })
    }

    #onNameInput(event: Event) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		this.#context?.updateScript({
            name: value
        });
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