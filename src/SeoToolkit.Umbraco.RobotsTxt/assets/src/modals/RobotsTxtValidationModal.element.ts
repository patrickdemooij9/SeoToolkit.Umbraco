import { customElement, html } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import RobotsTxtModuleContext, { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT } from "../contexts/RobotsTxtModuleContext";

@customElement('robotstxt-validation-modal')
export default class RobotsTxtValidationModal extends UmbModalBaseElement {
    
    #context?: RobotsTxtModuleContext; 

    constructor(){
        super();

        this.consumeContext(ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT, (instance) => {
            this.#context = instance;
        })
    }
    
    #handleClose() {
        this.modalContext?.reject();
    }

    #handleSave(){
        this.#context?.submit(true);
        this.#handleClose();
    }

    render() {
        return html`
                <uui-dialog-layout class="uui-text"
                    headline="Robots.txt validation didn\'t pass">
                    <p>The input submitted didn\'t pass the validation.</p>
                    <p>Are you sure you want to save the robots.txt? - ignoring the validation errors could result in potentially using an invalid robots.txt, which has a negative impact on SEO.</p>
    
                    <uui-button slot="actions" id="close" label="Close"
                            look='primary' color='danger'
                            @click="${this.#handleClose}">Close</uui-button>
                        <uui-button slot="actions" id="save" label="Save"
                            look='primary' color='positive'
                            @click="${this.#handleSave}">Save</uui-button>
    
                </uui-dialog-layout>
            `;
    }
}