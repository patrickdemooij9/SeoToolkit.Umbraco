import { OpenAPI as t } from "@umbraco-cms/backoffice/external/backend-api";
import { UMB_AUTH_CONTEXT as i } from "@umbraco-cms/backoffice/auth";
const e = "seoToolkit-robotstxt", r = {
  type: "treeItem",
  kind: "default",
  alias: "seoToolkit.module.robotsTxt",
  name: "SeoToolkit RobotsTxt",
  forEntityTypes: [
    e
  ]
}, a = {
  type: "workspace",
  alias: "seoToolkit.module.workspace.robotsTxt",
  name: "SeoToolkit RobotsTxt Workspace",
  element: () => import("./RobotsTxtModuleWorkspace.element-BBRbfPUL.js"),
  meta: {
    entityType: e
  }
}, p = [r, a], l = (s, T) => {
  s.consumeContext(i, (n) => {
    const o = n.getOpenApiConfiguration();
    t.BASE = o.base, t.WITH_CREDENTIALS = o.withCredentials, t.CREDENTIALS = o.credentials, t.TOKEN = o.token;
  }), T.registerMany(p);
};
export {
  l as onInit
};
//# sourceMappingURL=robotstxt.js.map
