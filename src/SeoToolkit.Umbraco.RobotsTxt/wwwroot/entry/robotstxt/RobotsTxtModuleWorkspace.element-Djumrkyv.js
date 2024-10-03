import { UmbElementMixin as p } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as _, state as T, customElement as b } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as c } from "./RobotsTxtModuleContext-BGEUb6Ff.js";
var f = Object.defineProperty, O = Object.getOwnPropertyDescriptor, n = (o, e, l, r) => {
  for (var t = r > 1 ? void 0 : r ? O(e, l) : e, m = o.length - 1, i; m >= 0; m--)
    (i = o[m]) && (t = (r ? i(e, l, t) : i(t)) || t);
  return r && t && f(e, l, t), t;
};
let s = class extends p(u) {
  constructor() {
    super(), this.consumeContext(c, (o) => {
      this.observe(o.test, (e) => {
        this._test = e;
      });
    });
  }
  render() {
    return _`
            <umb-workspace-editor 
                headline="Robots.Txt">
                ${this._test}
            </umb-workspace-editor>
        `;
  }
};
n([
  T()
], s.prototype, "_test", 2);
s = n([
  b("seotoolkit-module-robotstxt")
], s);
const d = s;
export {
  s as SeoToolkitRobotsTxtModuleElement,
  d as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-Djumrkyv.js.map
