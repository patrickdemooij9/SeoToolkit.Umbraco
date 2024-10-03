import { UmbElementMixin as p } from "@umbraco-cms/backoffice/element-api";
import { LitElement as u, html as _, state as T, customElement as c } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as b } from "./RobotsTxtModuleContext-FsZsf3ha.js";
var f = Object.defineProperty, O = Object.getOwnPropertyDescriptor, i = (o, e, l, s) => {
  for (var t = s > 1 ? void 0 : s ? O(e, l) : e, m = o.length - 1, n; m >= 0; m--)
    (n = o[m]) && (t = (s ? n(e, l, t) : n(t)) || t);
  return s && t && f(e, l, t), t;
};
let r = class extends p(u) {
  constructor() {
    super(), this.consumeContext(b, (o) => {
      console.log("Hello?"), this.observe(o.test, (e) => {
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
i([
  T()
], r.prototype, "_test", 2);
r = i([
  c("seotoolkit-module-robotstxt")
], r);
const d = r;
export {
  r as SeoToolkitRobotsTxtModuleElement,
  d as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-BXZMJXfZ.js.map
