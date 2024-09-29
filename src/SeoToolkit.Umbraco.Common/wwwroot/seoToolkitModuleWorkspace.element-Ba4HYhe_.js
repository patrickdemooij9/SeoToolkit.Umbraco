import { UmbElementMixin as m } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as c } from "@umbraco-cms/backoffice/external/lit";
import { UmbDefaultWorkspaceContext as a, UMB_WORKSPACE_CONTEXT as d } from "@umbraco-cms/backoffice/workspace";
import { S as f } from "./index-By48L2l4.js";
class E extends a {
  constructor(o) {
    super(o), this.provideContext(d, this), this.provideContext("seoToolkit.modules.context", this);
  }
  getEntityType() {
    return f;
  }
  getUnique() {
  }
}
var T = Object.defineProperty, _ = Object.getOwnPropertyDescriptor, x = (r, o, s, t) => {
  for (var e = t > 1 ? void 0 : t ? _(o, s) : o, i = r.length - 1, n; i >= 0; i--)
    (n = r[i]) && (e = (t ? n(o, s, e) : n(e)) || e);
  return t && e && T(o, s, e), e;
};
let l = class extends m(u) {
  constructor() {
    super(...arguments), this._workspaceContext = new E(this);
  }
  render() {
    return p`
            <umb-workspace-editor 
                headline="Editor"
                alias="seoToolkit.module.root"
                .enforceNoFooter=${!0}>
            </umb-workspace-editor>
        `;
  }
};
l = x([
  c("seotoolkit-module-root")
], l);
const b = l;
export {
  l as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-Ba4HYhe_.js.map
