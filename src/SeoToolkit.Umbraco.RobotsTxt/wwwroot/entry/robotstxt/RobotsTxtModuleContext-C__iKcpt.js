var T = (t) => {
  throw TypeError(t);
};
var n = (t, o, e) => o.has(t) || T("Cannot " + e);
var l = (t, o, e) => (n(t, o, "read from private field"), e ? e.call(t) : o.get(t)), i = (t, o, e) => o.has(t) ? T("Cannot add the same private member more than once") : o instanceof WeakSet ? o.add(t) : o.set(t, e), m = (t, o, e, r) => (n(t, o, "write to private field"), r ? r.call(t, e) : o.set(t, e), e);
import { UmbControllerBase as O } from "@umbraco-cms/backoffice/class-api";
import { S as a } from "./index-Dt2ptnuE.js";
import { UmbStringState as x } from "@umbraco-cms/backoffice/observable-api";
import { UmbContextToken as b } from "@umbraco-cms/backoffice/context-api";
var s;
class E extends O {
  constructor(e) {
    super(e);
    i(this, s);
    this.workspaceAlias = "seoToolkit.context.robotsTxtModule", m(this, s, new x("Hello world")), this.test = l(this, s).asObservable(), console.log("Test");
  }
  getEntityType() {
    return a;
  }
}
s = new WeakMap();
const _ = new b(
  "robotsTxtModuleContext"
);
export {
  _ as ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT,
  E as default
};
//# sourceMappingURL=RobotsTxtModuleContext-C__iKcpt.js.map
