import { OpenAPI as t } from "@umbraco-cms/backoffice/external/backend-api";
import { UMB_AUTH_CONTEXT as n } from "@umbraco-cms/backoffice/auth";
import { UmbSubmitWorkspaceAction as r } from "@umbraco-cms/backoffice/workspace";
const e = "seoToolkit-robotstxt", T = {
  type: "treeItem",
  kind: "default",
  alias: "seoToolkit.module.robotsTxt",
  name: "SeoToolkit RobotsTxt",
  forEntityTypes: [
    e
  ]
}, p = {
  type: "workspace",
  alias: "seoToolkit.module.workspace.robotsTxt",
  name: "SeoToolkit RobotsTxt Workspace",
  element: () => import("./RobotsTxtModuleWorkspace.element-dhHeKB5A.js"),
  api: () => import("./RobotsTxtModuleContext--tRAVjlk.js"),
  meta: {
    entityType: e
  }
}, c = {
  type: "workspaceAction",
  kind: "default",
  alias: "seoToolkit.module.workspace.actions.save",
  name: "SeoToolkit RobotsTxt Workspace Save",
  api: r,
  meta: {
    look: "primary",
    color: "positive",
    label: "#buttons_save"
  },
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "seoToolkit.module.workspace.robotsTxt"
    }
  ]
}, l = [T, p, c], d = (s, i) => {
  s.consumeContext(n, (a) => {
    const o = a.getOpenApiConfiguration();
    t.BASE = o.base, t.WITH_CREDENTIALS = o.withCredentials, t.CREDENTIALS = o.credentials, t.TOKEN = o.token;
  }), i.registerMany(l);
};
export {
  e as S,
  d as o
};
//# sourceMappingURL=index-e0j2uCwy.js.map
