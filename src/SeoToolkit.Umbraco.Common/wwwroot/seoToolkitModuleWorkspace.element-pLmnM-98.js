import { UmbElementMixin as m } from "@umbraco-cms/backoffice/element-api";
import { LitElement as p, html as u, customElement as T } from "@umbraco-cms/backoffice/external/lit";
import { UMB_WORKSPACE_CONTEXT as c } from "@umbraco-cms/backoffice/workspace";
import { S as a } from "./index-BYeEgeqj.js";
import { UmbControllerBase as E } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken as O } from "@umbraco-cms/backoffice/context-api";
class d extends E {
  constructor(e) {
    super(e), this.workspaceAlias = "seoToolkit.module.workspace", this.provideContext(_, this), this.provideContext(c, this);
  }
  getEntityType() {
    return a;
  }
}
const _ = new O(
  "SeoToolkitModuleContext"
);
var f = Object.defineProperty, C = Object.getOwnPropertyDescriptor, h = (r, e, s, t) => {
  for (var o = t > 1 ? void 0 : t ? C(e, s) : e, i = r.length - 1, l; i >= 0; i--)
    (l = r[i]) && (o = (t ? l(e, s, o) : l(o)) || o);
  return t && o && f(e, s, o), o;
};
let n = class extends m(p) {
  constructor() {
    super(), this._workspaceContext = new d(this);
  }
  render() {
    return u`
            <umb-workspace-editor 
                headline="Editor"
                alias="seoToolkit.module.root"
                .enforceNoFooter=${!0}>
            </umb-workspace-editor>
        `;
  }
};
n = h([
  T("seotoolkit-module-root")
], n);
const b = n;
export {
  n as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-pLmnM-98.js.map
