import { UmbElementMixin } 
    from "@umbraco-cms/backoffice/element-api";
import { LitElement, html, customElement } 
    from "@umbraco-cms/backoffice/external/lit";


@customElement('seotoolkit-module-robotstxt')
export class SeoToolkitRobotsTxtModuleElement extends
    UmbElementMixin(LitElement) {


    render() {
        return html`
            <umb-workspace-editor 
                headline="Robots.Txt"
                .enforceNoFooter=${true}>
            </umb-workspace-editor>
        `
    }
}

export default SeoToolkitRobotsTxtModuleElement;