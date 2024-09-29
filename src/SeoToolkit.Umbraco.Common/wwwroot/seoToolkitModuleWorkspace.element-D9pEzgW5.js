import { UmbElementMixin as i } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as c } from "@umbraco-cms/backoffice/external/lit";
import { S as f } from "./index-BcdD1qei.js";
var a = Object.defineProperty, d = Object.getOwnPropertyDescriptor, _ = (m, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? d(o, r) : o, l = m.length - 1, s; l >= 0; l--)
    (s = m[l]) && (e = (t ? s(o, r, e) : s(e)) || e);
  return t && e && a(o, r, e), e;
};
let n = class extends i(u) {
  constructor() {
    super(), this._workspaceContext = new f(this);
  }
  render() {
    return p`
            <umb-workspace-editor 
                headline="Editor"
                .enforceNoFooter=${!0}>
            </umb-workspace-editor>
        `;
  }
};
n = _([
  c("seotoolkit-module-root")
], n);
const b = n;
export {
  n as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-D9pEzgW5.js.map
