import { UmbElementMixin as i } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as p, customElement as b } from "@umbraco-cms/backoffice/external/lit";
var f = Object.defineProperty, c = Object.getOwnPropertyDescriptor, a = (n, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? c(o, r) : o, l = n.length - 1, s; l >= 0; l--)
    (s = n[l]) && (e = (t ? s(o, r, e) : s(e)) || e);
  return t && e && f(o, r, e), e;
};
let m = class extends i(u) {
  render() {
    return p`
            <umb-workspace-editor 
                headline="Robots.Txt"
                .enforceNoFooter=${!0}>
            </umb-workspace-editor>
        `;
  }
};
m = a([
  b("seotoolkit-module-robotstxt")
], m);
const v = m;
export {
  m as SeoToolkitRobotsTxtModuleElement,
  v as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-BBRbfPUL.js.map
