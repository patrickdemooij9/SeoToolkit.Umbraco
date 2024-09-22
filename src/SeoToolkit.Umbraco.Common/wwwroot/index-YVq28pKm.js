import { UMB_AUTH_CONTEXT as a } from "@umbraco-cms/backoffice/auth";
const c = {
  type: "section",
  alias: "SeoToolkit",
  name: "SeoToolkit",
  weight: 10,
  meta: {
    label: "SeoToolkit",
    pathname: "SeoToolkit"
  }
}, l = {
  type: "dashboard",
  alias: "seoToolkitWelcomeDashboard",
  name: "Welcome",
  meta: {
    pathname: "welcome"
  },
  element: () => import("./welcomeDashboard.element-BvxL4qi2.js"),
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
};
class n {
  constructor() {
    this._fns = [];
  }
  eject(e) {
    const o = this._fns.indexOf(e);
    o !== -1 && (this._fns = [...this._fns.slice(0, o), ...this._fns.slice(o + 1)]);
  }
  use(e) {
    this._fns = [...this._fns, e];
  }
}
const s = {
  BASE: "",
  CREDENTIALS: "include",
  ENCODE_PATH: void 0,
  HEADERS: void 0,
  PASSWORD: void 0,
  TOKEN: void 0,
  USERNAME: void 0,
  VERSION: "Latest",
  WITH_CREDENTIALS: !1,
  interceptors: {
    request: new n(),
    response: new n()
  }
}, E = (i, e) => {
  i.consumeContext(a, (o) => {
    const t = o.getOpenApiConfiguration();
    s.BASE = t.base, s.WITH_CREDENTIALS = t.withCredentials, s.CREDENTIALS = t.credentials, s.TOKEN = t.token;
  }), e.register(c), e.register(l);
};
export {
  s as O,
  E as o
};
//# sourceMappingURL=index-YVq28pKm.js.map
