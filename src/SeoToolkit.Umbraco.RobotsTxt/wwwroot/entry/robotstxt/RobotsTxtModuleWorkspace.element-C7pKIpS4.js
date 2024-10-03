import { UmbElementMixin as i } from "@umbraco-cms/backoffice/element-api";
import { LitElement as p, html as u, customElement as b } from "@umbraco-cms/backoffice/external/lit";
var f = Object.defineProperty, a = Object.getOwnPropertyDescriptor, c = (n, o, r, t) => {
  for (var e = t > 1 ? void 0 : t ? a(o, r) : o, l = n.length - 1, s; l >= 0; l--)
    (s = n[l]) && (e = (t ? s(o, r, e) : s(e)) || e);
  return t && e && f(o, r, e), e;
};
let m = class extends i(p) {
  render() {
    return u`
            <umb-workspace-editor 
                headline="Robots.Txt">
                
            </umb-workspace-editor>
        `;
  }
};
m = c([
  b("seotoolkit-module-robotstxt")
], m);
const v = m;
export {
  m as SeoToolkitRobotsTxtModuleElement,
  v as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-C7pKIpS4.js.map
