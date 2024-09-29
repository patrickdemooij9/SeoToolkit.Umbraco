import { LitElement, html, customElement } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { ManifestWorkspaceView } from "@umbraco-cms/backoffice/extension-registry";

@customElement('robotstxt-module')
export class RobotsTxtModule extends UmbElementMixin(LitElement) {

  render() {
    return html`
      <div class="welcomeDashboard">
        <h1>Welcome to RobotsTxt!</h1>
      </div>
    `;
  }
}

export default RobotsTxtModule;

export const robotsTxtWorkspaceView: ManifestWorkspaceView =
{
    type: 'workspaceView',
    alias: 'seotoolkit.workspace.robotstxt',
    name: 'default view',
    js: () => import('./RobotsTxtModuleContentApp.element'),
    weight: 300,
    meta: {
        icon: 'icon-alarm-clock',
        pathname: 'overview',
        label: 'RobotsTxt'
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.module.workspace'
        },
    ],
};