import { UmbElementMixin } 
    from "@umbraco-cms/backoffice/element-api";
import { LitElement, html, customElement } 
    from "@umbraco-cms/backoffice/external/lit";
import SeoToolkitModuleContext from "./seoToolkitModule.context";

import "../dashboards/welcome/welcomeDashboard.element";

@customElement('seotoolkit-module-root')
export class SeoToolkitModuleElement extends
    UmbElementMixin(LitElement) {

        _workspaceContext: SeoToolkitModuleContext;

        constructor(){
            super();

            this._workspaceContext = new SeoToolkitModuleContext(this);
        }

    render() {
        return html`
            <umb-workspace-editor 
                headline="Editor"
                .enforceNoFooter=${true}>
            </umb-workspace-editor>
        `
    }
}

export default SeoToolkitModuleElement;