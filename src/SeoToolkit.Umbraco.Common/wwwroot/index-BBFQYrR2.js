import { UMB_AUTH_CONTEXT as a } from "@umbraco-cms/backoffice/auth";
const l = {
  type: "section",
  alias: "SeoToolkit",
  name: "SeoToolkit",
  weight: 10,
  meta: {
    label: "SeoToolkit",
    pathname: "SeoToolkit"
  }
}, c = {
  type: "dashboard",
  alias: "seoToolkitWelcomeDashboard",
  name: "Welcome",
  meta: {
    pathname: "welcome"
  },
  element: () => import("./welcomeDashboard.element-CebMxF81.js"),
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
  eject(o) {
    const e = this._fns.indexOf(o);
    e !== -1 && (this._fns = [...this._fns.slice(0, e), ...this._fns.slice(e + 1)]);
  }
  use(o) {
    this._fns = [...this._fns, o];
  }
}
const i = {
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
}, r = {
  type: "sectionSidebarApp",
  kind: "menuWithEntityActions",
  alias: "seoToolkit.sidebar",
  name: "SeoToolkit",
  meta: {
    label: "SeoToolkit"
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
}, T = (s, o) => {
  s.consumeContext(a, (e) => {
    const t = e.getOpenApiConfiguration();
    i.BASE = t.base, i.WITH_CREDENTIALS = t.withCredentials, i.CREDENTIALS = t.credentials, i.TOKEN = t.token;
  }), o.register(l), o.register(c), o.register(r);
};
export {
  i as O,
  T as o
};
//# sourceMappingURL=index-BBFQYrR2.js.map
