import { UmbElementMixin as n } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as a } from "@umbraco-cms/backoffice/external/lit";
import { S as c } from "./index-Dj4_kxUq.js";
var d = Object.defineProperty, f = Object.getOwnPropertyDescriptor, _ = (m, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? f(o, r) : o, l = m.length - 1, s; l >= 0; l--)
    (s = m[l]) && (e = (t ? s(o, r, e) : s(e)) || e);
  return t && e && d(o, r, e), e;
};
let i = class extends n(u) {
  constructor() {
    super(), this._workspaceContext = new c(this);
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
i = _([
  a("seotoolkit-module-root")
], i);
const b = i;
export {
  i as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-COlyq3wm.js.map
