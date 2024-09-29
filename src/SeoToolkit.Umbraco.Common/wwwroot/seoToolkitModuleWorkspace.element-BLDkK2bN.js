import { UmbElementMixin as n } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as a } from "@umbraco-cms/backoffice/external/lit";
var d = Object.defineProperty, f = Object.getOwnPropertyDescriptor, c = (i, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? f(o, r) : o, l = i.length - 1, m; l >= 0; l--)
    (m = i[l]) && (e = (t ? m(o, r, e) : m(e)) || e);
  return t && e && d(o, r, e), e;
};
let s = class extends n(u) {
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
s = c([
  a("seotoolkit-module-root")
], s);
const _ = s;
export {
  s as SeoToolkitModuleElement,
  _ as default
};
//# sourceMappingURL=seoToolkitModuleWorkspace.element-BLDkK2bN.js.map
