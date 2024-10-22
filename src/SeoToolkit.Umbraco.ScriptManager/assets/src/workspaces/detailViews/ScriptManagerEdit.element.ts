import { UmbWorkspaceViewElement } from "@umbraco-cms/backoffice/extension-registry";
import { customElement, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbPropertyDatasetElement, UmbPropertyValueData } from "@umbraco-cms/backoffice/property";
import { css, html } from "lit";
import { ScriptDefinitionViewModel, ScriptField } from "../../api";
import ScriptManagerDetailContext, { ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT } from "../ScriptManagerDetailContext";

@customElement("script-manager-edit")
export class ScriptManagerEdit extends UmbLitElement implements UmbWorkspaceViewElement {

    #context?: ScriptManagerDetailContext;

    @state()
    _definitions: Array<ScriptDefinitionViewModel> = [];

    @state() 
    _values: Array<UmbPropertyValueData> = [];

    @state()
    _fields: Array<ScriptField> = [];

    constructor(){
        super();

        this.consumeContext(ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT, (instance) => {
            this.#context = instance;

            this.observe(this.#context.definitions, (value) => {
                this._definitions = value;
            });
            this.observe(this.#context.script, (value) => {
                this._values = [];
                this._fields = [];
                if (value.definitionAlias) {
                    const definition = this._definitions.find((def) => def.alias == value.definitionAlias);
                    if (definition){
                        this._values.push({
                            alias: 'scriptType',
                            value: definition.name
                        });

                        definition.fields?.forEach((field) => {
                            this._values.push({
                                alias: field.key!,
                                value: value.config && value.config.hasOwnProperty(field.key!) ? value.config[field.key!] : ''
                            });
                            this._fields.push(field);
                        })
                    }
                }
            });
        });
    }

    #onPropertyDataChange(e: Event) {
        const value = (e.target as UmbPropertyDatasetElement).value;

        const newScriptTypeName = (value.find((item) => item.alias === 'scriptType')?.value as string[])[0];
        if (newScriptTypeName) {
            const newScriptTypeAlias = this._definitions.find((item) => item.name == newScriptTypeName)?.alias;
            if (newScriptTypeAlias){
                this.#context?.updateScript({
                    definitionAlias: newScriptTypeAlias
                });
            }
        }

        const config: {[key: string]: string} = {}
        this._fields.forEach((field) => {
            var fieldValue = value.find((item) => item.alias === field.key);
            if (fieldValue){
                config[fieldValue.alias] = fieldValue.value!.toString()
            }
        });
        this.#context?.updateScript({
            config: config
        });
    }

    override render(){
        return html`
            <div id="scriptmanager-edit">
                <uui-box>
                    <umb-property-dataset
                    .value=${this._values}
                    @change=${this.#onPropertyDataChange}>
                        <umb-property 
                            alias='scriptType'
                            label='Script type'
                            description='Select a script type.'
                            property-editor-ui-alias='Umb.PropertyEditorUi.Dropdown'
                            val
                            .config=${[{
                                alias: 'items',
                                value: this._definitions.map((def) => def.name)
                            }]}>
                        </umb-property>

                        ${repeat(
                            this._fields,
                            (itm) => itm.key,
                            (itm) => html`<umb-property
                                alias=${itm.key!}
                                label=${itm.name!}
                                description=${itm.description!}
                                property-editor-ui-alias=${itm.propertyAlias!}>
                            </umb-property>`)}
                    </umb-property-dataset>
                </uui-box>
            </div>
        `
    }

    static styles = [
        css`
            #scriptmanager-edit {
                padding: var(--uui-size-layout-1);
            }
        `
    ]
}

export default ScriptManagerEdit;