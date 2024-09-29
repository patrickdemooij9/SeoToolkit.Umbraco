import { UmbElementMixin as m } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as c } from "@umbraco-cms/backoffice/external/lit";
import { UmbDefaultWorkspaceContext as a } from "@umbraco-cms/backoffice/workspace";
import { S as d } from "./index-CO-uTaRr.js";
class f extends a {
  constructor(o) {
    super(o), this.provideContext("seoToolkit.modules.context", this);
  }
  getEntityType() {
    return d;
  }
  getUnique() {
  }
}
var E = Object.defineProperty, T = Object.getOwnPropertyDescriptor, x = (r, o, s, t) => {
  for (var e = t > 1 ? void 0 : t ? T(o, s) : o, l = r.length - 1, n; l >= 0; l--)
    (n = r[l]) && (e = (t ? n(o, s, e) : n(e)) || e);
  return t && e && E(o, s, e), e;
};
let i = class extends m(u) {
  constructor() {
    super(...arguments), this._workspaceContext = new f(this);
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
i = x([
  c("seotoolkit-module-root")
], i);
const b = i;
export {
  i as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-BAJEu5qn.js.map
