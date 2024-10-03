import { UmbElementMixin as u } from "@umbraco-cms/backoffice/element-api";
import { LitElement as i, html as p, customElement as f } from "@umbraco-cms/backoffice/external/lit";
var b = Object.defineProperty, c = Object.getOwnPropertyDescriptor, a = (n, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? c(o, r) : o, l = n.length - 1, s; l >= 0; l--)
    (s = n[l]) && (e = (t ? s(o, r, e) : s(e)) || e);
  return t && e && b(o, r, e), e;
};
let m = class extends u(i) {
  render() {
    return p`
            <umb-workspace-editor
                .enforceNoFooter=${!0}>
                
            </umb-workspace-editor>
        `;
  }
};
m = a([
  f("seotoolkit-module-robotstxt")
], m);
const x = m;
export {
  m as SeoToolkitRobotsTxtModuleElement,
  x as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-ByLdD3zx.js.map
