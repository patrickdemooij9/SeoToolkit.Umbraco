import { customElement, html } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

@customElement("seotoolkit-site-audit-detail")
export default class SiteAuditDetailWorkspace extends UmbLitElement {
  protected override render() {
    return html`
      <umb-workspace-editor
        alias="seoToolkit.siteAudit.detail"
        .enforceNoFooter=${true}
      >
      </umb-workspace-editor>
    `;
  }
}
