import { UmbElementMixin as n } from "@umbraco-cms/backoffice/element-api";
import { LitElement as p, html as u, customElement as a } from "@umbraco-cms/backoffice/external/lit";
import { UMB_WORKSPACE_CONTEXT as c } from "@umbraco-cms/backoffice/workspace";
import { S as d } from "./index-DUgqX8AQ.js";
import { UmbControllerBase as f } from "@umbraco-cms/backoffice/class-api";
class E extends f {
  constructor(o) {
    super(o), this.workspaceAlias = "seoToolkit.module.workspace", this.provideContext(c, this);
  }
  getEntityType() {
    return d;
  }
}
var T = Object.defineProperty, _ = Object.getOwnPropertyDescriptor, O = (r, o, s, t) => {
  for (var e = t > 1 ? void 0 : t ? _(o, s) : o, l = r.length - 1, i; l >= 0; l--)
    (i = r[l]) && (e = (t ? i(o, s, e) : i(e)) || e);
  return t && e && T(o, s, e), e;
};
let m = class extends n(p) {
  constructor() {
    super(...arguments), this._workspaceContext = new E(this);
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
m = O([
  a("seotoolkit-module-root")
], m);
const b = m;
export {
  m as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-CAU_pF-u.js.map
