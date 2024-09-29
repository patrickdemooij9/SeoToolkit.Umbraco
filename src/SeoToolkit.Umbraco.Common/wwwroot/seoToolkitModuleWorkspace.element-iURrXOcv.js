import { UmbElementMixin as n } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as a } from "@umbraco-cms/backoffice/external/lit";
var f = Object.defineProperty, c = Object.getOwnPropertyDescriptor, d = (i, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? c(o, r) : o, l = i.length - 1, m; l >= 0; l--)
    (m = i[l]) && (e = (t ? m(o, r, e) : m(e)) || e);
  return t && e && f(o, r, e), e;
};
let s = class extends n(u) {
  render() {
    return p`
            <umb-workspace-editor 
                headline="Time"
                alias="seoToolkit.module.root"
                .enforceNoFooter=${!0}>
            </umb-workspace-editor>
        `;
  }
};
s = d([
  a("seotoolkit-module-root")
], s);
const b = s;
export {
  s as SeoToolkitModuleElement,
  b as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-iURrXOcv.js.map
