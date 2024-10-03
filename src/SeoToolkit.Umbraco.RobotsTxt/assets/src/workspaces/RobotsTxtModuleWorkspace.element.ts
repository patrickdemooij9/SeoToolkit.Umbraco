import { UmbElementMixin }
    from "@umbraco-cms/backoffice/element-api";
import { LitElement, html, customElement, state, when, repeat, css }
    from "@umbraco-cms/backoffice/external/lit";
import RobotsTxtModuleContext, { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT } from "../contexts/RobotsTxtModuleContext";
import { UmbPropertyTypeAppearanceModel } from "@umbraco-cms/backoffice/content-type";
import { ValidationError } from "../types/ValidationError";
import { UmbPropertyDatasetElement, UmbPropertyValueData } from "@umbraco-cms/backoffice/property";


@customElement('seotoolkit-module-robotstxt')
export class SeoToolkitRobotsTxtModuleElement extends
    UmbElementMixin(LitElement) {

    #context?: RobotsTxtModuleContext;

    @state()
    _content?: UmbPropertyValueData;

    @state()
    _validationErrors: ValidationError[] = [];

    propertyAppearance: UmbPropertyTypeAppearanceModel = {
        labelOnTop: true
    };

    constructor() {
        super();

        this.consumeContext(ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT, (instance) => {
            this.#context = instance;

            this.observe(instance.content, (val) => {
                this._content = {
                    alias: 'robotsTxt',
                    value: val
                };
            })
        });
    }

    #onPropertyDataChange(e: Event) {
        const value = (e.target as UmbPropertyDatasetElement).value;

        const newValue = value.find((item) => item.alias === 'robotsTxt')?.value as string;
        if (newValue) {
            this.#context?.setContent(newValue);
        }
    }

    render() {
        return html`
            <umb-workspace-editor>
                <div class="robotsTxtWorkspace">
                <uui-box headline="Robots.txt" headline-variant="h5">
                    <umb-property-dataset
                        .value=${[this._content!]}
                        @change=${this.#onPropertyDataChange}>
                        <umb-property 
                            alias='robotsTxt'
                            label='Robots.txt'
                            description='Robots.txt is used to let bots know what they are able to access. If you are looking for more information about how to configure robots.txt, then <a class="btn-link -underline" href="https://developers.google.com/search/docs/advanced/robots/intro">this guide</a> will be able to help you out.'
                            property-editor-ui-alias='Umb.PropertyEditorUi.TextArea'
                            val
                            .appearance=${this.propertyAppearance}
                            .config=${[{
                alias: 'rows',
                value: 10
            }]}>
                        </umb-property>
                    </umb-property-dataset>

                    ${when(this._validationErrors.length > 0, () => html`
                        <h5>Validation Errors</h5>
                            ${repeat(this._validationErrors, (item) => item.error, (item) => html`
                            <p class="error">
                                <umb-icon icon="icon-delete" class="red"></umb-icon>
                                Line ${item.lineNumber} - ${item.error}
                            </p>
                            `)}
                        </div>
                            `)}
                </uui-box>
                </div>
            </umb-workspace-editor>
        `
    }

    static styles = [
        css`
            .robotsTxtWorkspace{
                padding: var(--uui-size-layout-1);
            }
        `
    ]
}

export default SeoToolkitRobotsTxtModuleElement;