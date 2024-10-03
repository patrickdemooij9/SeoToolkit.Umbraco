import { UmbElementMixin as n } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as a, state as b, customElement as T } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as _ } from "./RobotsTxtModuleContext-BxyRXlOG.js";
var c = Object.defineProperty, x = Object.getOwnPropertyDescriptor, p = (o, e, l, r) => {
  for (var t = r > 1 ? void 0 : r ? x(e, l) : e, m = o.length - 1, i; m >= 0; m--)
    (i = o[m]) && (t = (r ? i(e, l, t) : i(t)) || t);
  return r && t && c(e, l, t), t;
};
let s = class extends n(u) {
  constructor() {
    super(), this.consumeContext(_, (o) => {
      console.log("Hello?"), this.observe(o.test, (e) => {
        this._test = e;
      });
    });
  }
  render() {
    return a`
            <umb-workspace-editor 
                headline="Robots.Txt">
                <umb-property-editor-ui-textarea></umb-property-editor-ui-textarea>
            </umb-workspace-editor>
        `;
  }
};
p([
  b()
], s.prototype, "_test", 2);
s = p([
  T("seotoolkit-module-robotstxt")
], s);
const E = s;
export {
  s as SeoToolkitRobotsTxtModuleElement,
  E as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-B902xmC2.js.map
