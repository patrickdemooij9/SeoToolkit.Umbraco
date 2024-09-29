import { UmbElementMixin as i } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as f } from "@umbraco-cms/backoffice/external/lit";
var a = Object.defineProperty, c = Object.getOwnPropertyDescriptor, d = (n, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? c(o, r) : o, l = n.length - 1, m; l >= 0; l--)
    (m = n[l]) && (e = (t ? m(o, r, e) : m(e)) || e);
  return t && e && a(o, r, e), e;
};
let s = class extends i(u) {
  render() {
    return p`
            <umb-workspace-editor 
                alias="seoToolkit.module.root"
                .enforceNoFooter=${!0}>
            </umb-workspace-editor>
        `;
  }
};
s = d([
  f("seotoolkit-module-root")
], s);
const b = s;
export {
  s as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-BJrQtRGU.js.map
