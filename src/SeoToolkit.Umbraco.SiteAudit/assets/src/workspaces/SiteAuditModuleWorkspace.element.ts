import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement } from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";

@customElement('seotoolkit-module-site-audit')
export class SiteAuditModuleWorkspace extends UmbElementMixin(LitElement) {

    render() {
        return html`
            <umb-body-layout main-no-padding headline='Site audits'>
                <umb-collection alias='seoToolkit.collections.siteAudits'></umb-collection>;
            </umb-body-layout>
        `
    }
}

export default SiteAuditModuleWorkspace;