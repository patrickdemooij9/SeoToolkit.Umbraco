var T = (t) => {
  throw TypeError(t);
};
var i = (t, e, o) => e.has(t) || T("Cannot " + o);
var r = (t, e, o) => (i(t, e, "read from private field"), o ? o.call(t) : e.get(t)), a = (t, e, o) => e.has(t) ? T("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, o), l = (t, e, o, n) => (i(t, e, "write to private field"), n ? n.call(t, o) : e.set(t, o), o);
import { UmbControllerBase as m } from "@umbraco-cms/backoffice/class-api";
import { S as O } from "./index-CfbrWeco.js";
import { UmbStringState as x } from "@umbraco-cms/backoffice/observable-api";
import { UmbContextToken as p } from "@umbraco-cms/backoffice/context-api";
var s;
class d extends m {
  constructor(o) {
    super(o);
    a(this, s);
    this.workspaceAlias = "seoToolkit.context.robotsTxtModule", l(this, s, new x("")), this.content = r(this, s).asObservable(), this.provideContext(b, this);
  }
  getEntityType() {
    return O;
  }
  setContent(o) {
    r(this, s).setValue(o);
  }
}
s = new WeakMap();
const b = new p(
  "robotsTxtModuleContext"
);
export {
  b as ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT,
  d as default
};
//# sourceMappingURL=RobotsTxtModuleContext-Ddn3Hhrr.js.map
