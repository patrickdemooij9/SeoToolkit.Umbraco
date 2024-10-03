import { UmbElementMixin as u } from "@umbraco-cms/backoffice/element-api";
import { LitElement as a, html as p, state as b, customElement as T } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as _ } from "./RobotsTxtModuleContext--tRAVjlk.js";
var c = Object.defineProperty, x = Object.getOwnPropertyDescriptor, n = (o, t, l, r) => {
  for (var e = r > 1 ? void 0 : r ? x(t, l) : t, m = o.length - 1, i; m >= 0; m--)
    (i = o[m]) && (e = (r ? i(t, l, e) : i(e)) || e);
  return r && e && c(t, l, e), e;
};
let s = class extends u(a) {
  constructor() {
    super(), this.consumeContext(_, (o) => {
      console.log("Hello?"), this.observe(o.test, (t) => {
        this._test = t;
      });
    });
  }
  render() {
    return p`
            <umb-workspace-editor 
                headline="Robots.Txt">
                <uui-textarea label="Label"></uui-textarea>
            </umb-workspace-editor>
        `;
  }
};
n([
  b()
], s.prototype, "_test", 2);
s = n([
  T("seotoolkit-module-robotstxt")
], s);
const d = s;
export {
  s as SeoToolkitRobotsTxtModuleElement,
  d as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-dhHeKB5A.js.map
