var T = (t) => {
  throw TypeError(t);
};
var n = (t, o, e) => o.has(t) || T("Cannot " + e);
var i = (t, o, e) => (n(t, o, "read from private field"), e ? e.call(t) : o.get(t)), l = (t, o, e) => o.has(t) ? T("Cannot add the same private member more than once") : o instanceof WeakSet ? o.add(t) : o.set(t, e), m = (t, o, e, r) => (n(t, o, "write to private field"), r ? r.call(t, e) : o.set(t, e), e);
import { UmbControllerBase as O } from "@umbraco-cms/backoffice/class-api";
import { S as a } from "./index-6IFdIq3d.js";
import { UmbStringState as x } from "@umbraco-cms/backoffice/observable-api";
import { UmbContextToken as p } from "@umbraco-cms/backoffice/context-api";
var s;
class C extends O {
  constructor(e) {
    super(e);
    l(this, s);
    this.workspaceAlias = "seoToolkit.context.robotsTxtModule", m(this, s, new x("Hello world")), this.test = i(this, s).asObservable(), this.provideContext(b, this), console.log("Test");
  }
  getEntityType() {
    return a;
  }
}
s = new WeakMap();
const b = new p(
  "robotsTxtModuleContext"
);
export {
  b as ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT,
  C as default
};
//# sourceMappingURL=RobotsTxtModuleContext-CpOUazKL.js.map
