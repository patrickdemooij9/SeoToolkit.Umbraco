var A = (t) => {
  throw TypeError(t);
};
var x = (t, e, o) => e.has(t) || A("Cannot " + o);
var g = (t, e, o) => (x(t, e, "read from private field"), o ? o.call(t) : e.get(t)), w = (t, e, o) => e.has(t) ? A("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, o), _ = (t, e, o, r) => (x(t, e, "write to private field"), r ? r.call(t, o) : e.set(t, o), o);
import { LitElement as B, html as u, when as f, repeat as L, css as W, state as F, customElement as G } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as V } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as K } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as z } from "@umbraco-cms/backoffice/resources";
import { UMB_AUTH_CONTEXT as J } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as X, UmbTreeRepositoryBase as Y, UmbUniqueTreeStore as Q } from "@umbraco-cms/backoffice/tree";
import { UmbConditionBase as Z } from "@umbraco-cms/backoffice/extension-registry";
const ee = {
  type: "section",
  alias: "SeoToolkit",
  name: "SeoToolkit",
  weight: 10,
  meta: {
    label: "SeoToolkit",
    pathname: "SeoToolkit"
  }
};
class j extends Error {
  constructor(e, o, r) {
    super(r), this.name = "ApiError", this.url = o.url, this.status = o.status, this.statusText = o.statusText, this.body = o.body, this.request = e;
  }
}
class te extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class oe {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((o, r) => {
      this._resolve = o, this._reject = r;
      const s = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isResolved = !0, this._resolve && this._resolve(a));
      }, n = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isRejected = !0, this._reject && this._reject(a));
      }, i = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || this.cancelHandlers.push(a);
      };
      return Object.defineProperty(i, "isResolved", {
        get: () => this._isResolved
      }), Object.defineProperty(i, "isRejected", {
        get: () => this._isRejected
      }), Object.defineProperty(i, "isCancelled", {
        get: () => this._isCancelled
      }), e(s, n, i);
    });
  }
  get [Symbol.toStringTag]() {
    return "Cancellable Promise";
  }
  then(e, o) {
    return this.promise.then(e, o);
  }
  catch(e) {
    return this.promise.catch(e);
  }
  finally(e) {
    return this.promise.finally(e);
  }
  cancel() {
    if (!(this._isResolved || this._isRejected || this._isCancelled)) {
      if (this._isCancelled = !0, this.cancelHandlers.length)
        try {
          for (const e of this.cancelHandlers)
            e();
        } catch (e) {
          console.warn("Cancellation threw an error", e);
          return;
        }
      this.cancelHandlers.length = 0, this._reject && this._reject(new te("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
class v {
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
const d = {
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
    request: new v(),
    response: new v()
  }
}, T = (t) => typeof t == "string", R = (t) => T(t) && t !== "", C = (t) => t instanceof Blob, U = (t) => t instanceof FormData, re = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, se = (t) => {
  const e = [], o = (s, n) => {
    e.push(`${encodeURIComponent(s)}=${encodeURIComponent(String(n))}`);
  }, r = (s, n) => {
    n != null && (n instanceof Date ? o(s, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => r(s, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => r(`${s}[${i}]`, a)) : o(s, n));
  };
  return Object.entries(t).forEach(([s, n]) => r(s, n)), e.length ? `?${e.join("&")}` : "";
}, ne = (t, e) => {
  const o = encodeURI, r = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? o(String(e.path[i])) : n;
  }), s = t.BASE + r;
  return e.query ? s + se(e.query) : s;
}, ie = (t) => {
  if (t.formData) {
    const e = new FormData(), o = (r, s) => {
      T(s) || C(s) ? e.append(r, s) : e.append(r, JSON.stringify(s));
    };
    return Object.entries(t.formData).filter(([, r]) => r != null).forEach(([r, s]) => {
      Array.isArray(s) ? s.forEach((n) => o(r, n)) : o(r, s);
    }), e;
  }
}, y = async (t, e) => typeof e == "function" ? e(t) : e, ae = async (t, e) => {
  const [o, r, s, n] = await Promise.all([
    // @ts-ignore
    y(e, t.TOKEN),
    // @ts-ignore
    y(e, t.USERNAME),
    // @ts-ignore
    y(e, t.PASSWORD),
    // @ts-ignore
    y(e, t.HEADERS)
  ]), i = Object.entries({
    Accept: "application/json",
    ...n,
    ...e.headers
  }).filter(([, a]) => a != null).reduce((a, [c, l]) => ({
    ...a,
    [c]: String(l)
  }), {});
  if (R(o) && (i.Authorization = `Bearer ${o}`), R(r) && R(s)) {
    const a = re(`${r}:${s}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : C(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : T(e.body) ? i["Content-Type"] = "text/plain" : U(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, le = (t) => {
  var e, o;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (o = t.mediaType) != null && o.includes("+json") ? JSON.stringify(t.body) : T(t.body) || C(t.body) || U(t.body) ? t.body : JSON.stringify(t.body);
}, ce = async (t, e, o, r, s, n, i) => {
  const a = new AbortController();
  let c = {
    headers: n,
    body: r ?? s,
    method: e.method,
    signal: a.signal
  };
  t.WITH_CREDENTIALS && (c.credentials = t.CREDENTIALS);
  for (const l of t.interceptors.request._fns)
    c = await l(c);
  return i(() => a.abort()), await fetch(o, c);
}, de = (t, e) => {
  if (e) {
    const o = t.headers.get(e);
    if (T(o))
      return o;
  }
}, ue = async (t) => {
  if (t.status !== 204)
    try {
      const e = t.headers.get("Content-Type");
      if (e) {
        const o = ["application/octet-stream", "application/pdf", "application/zip", "audio/", "image/", "video/"];
        if (e.includes("application/json") || e.includes("+json"))
          return await t.json();
        if (o.some((r) => e.includes(r)))
          return await t.blob();
        if (e.includes("multipart/form-data"))
          return await t.formData();
        if (e.includes("text/"))
          return await t.text();
      }
    } catch (e) {
      console.error(e);
    }
}, me = (t, e) => {
  const r = {
    400: "Bad Request",
    401: "Unauthorized",
    402: "Payment Required",
    403: "Forbidden",
    404: "Not Found",
    405: "Method Not Allowed",
    406: "Not Acceptable",
    407: "Proxy Authentication Required",
    408: "Request Timeout",
    409: "Conflict",
    410: "Gone",
    411: "Length Required",
    412: "Precondition Failed",
    413: "Payload Too Large",
    414: "URI Too Long",
    415: "Unsupported Media Type",
    416: "Range Not Satisfiable",
    417: "Expectation Failed",
    418: "Im a teapot",
    421: "Misdirected Request",
    422: "Unprocessable Content",
    423: "Locked",
    424: "Failed Dependency",
    425: "Too Early",
    426: "Upgrade Required",
    428: "Precondition Required",
    429: "Too Many Requests",
    431: "Request Header Fields Too Large",
    451: "Unavailable For Legal Reasons",
    500: "Internal Server Error",
    501: "Not Implemented",
    502: "Bad Gateway",
    503: "Service Unavailable",
    504: "Gateway Timeout",
    505: "HTTP Version Not Supported",
    506: "Variant Also Negotiates",
    507: "Insufficient Storage",
    508: "Loop Detected",
    510: "Not Extended",
    511: "Network Authentication Required",
    ...t.errors
  }[e.status];
  if (r)
    throw new j(t, e, r);
  if (!e.ok) {
    const s = e.status ?? "unknown", n = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new j(
      t,
      e,
      `Generic Error: status: ${s}; status text: ${n}; body: ${i}`
    );
  }
}, b = (t, e) => new oe(async (o, r, s) => {
  try {
    const n = ne(t, e), i = ie(e), a = le(e), c = await ae(t, e);
    if (!s.isCancelled) {
      let l = await ce(t, e, n, a, i, c, s);
      for (const $ of t.interceptors.response._fns)
        l = await $(l);
      const I = await ue(l), H = de(l, e.responseHeader);
      let O = I;
      e.responseTransformer && l.ok && (O = await e.responseTransformer(I));
      const D = {
        url: n,
        ok: l.ok,
        status: l.status,
        statusText: l.statusText,
        body: H ?? O
      };
      me(e, D), o(D.body);
    }
  } catch (n) {
    r(n);
  }
});
class k {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return b(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/modules"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.descendantId
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitTreeInfoAncestors(e = {}) {
    return b(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/tree/info/ancestors",
      query: {
        descendantId: e.descendantId
      }
    });
  }
  /**
   * @param data The data for the request.
   * @param data.parentId
   * @param data.skip
   * @param data.take
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitTreeInfoChildren(e = {}) {
    return b(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/tree/info/children",
      query: {
        parentId: e.parentId,
        skip: e.skip,
        take: e.take
      }
    });
  }
  /**
   * @param data The data for the request.
   * @param data.skip
   * @param data.take
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitTreeInfoRoot(e = {}) {
    return b(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/tree/info/root",
      query: {
        skip: e.skip,
        take: e.take
      }
    });
  }
}
var h;
class he {
  constructor(e) {
    w(this, h);
    _(this, h, e);
  }
  async getModules() {
    return await z(g(this, h), k.getUmbracoSeoToolkitModules());
  }
}
h = new WeakMap();
var p;
class pe extends K {
  constructor(o) {
    super(o);
    w(this, p);
    _(this, p, new he(this));
  }
  async getModules() {
    return g(this, p).getModules();
  }
}
p = new WeakMap();
var Te = Object.defineProperty, fe = Object.getOwnPropertyDescriptor, q = (t, e, o, r) => {
  for (var s = r > 1 ? void 0 : r ? fe(e, o) : e, n = t.length - 1, i; n >= 0; n--)
    (i = t[n]) && (s = (r ? i(e, o, s) : i(s)) || s);
  return r && s && Te(e, o, s), s;
};
let m = class extends V(B) {
  connectedCallback() {
    super.connectedCallback(), new pe(this).getModules().then((t) => {
      this.modules = t.data, console.log(this.modules);
    });
  }
  render() {
    return u`
      <div class="welcomeDashboard">
        <h1>Welcome!</h1>
        <div class="intro">
            <p>This is the dashboard for SeoToolkit. The SEO package for Umbraco.</p>
            <p>Here you can see what modules are installed. Each functionality is shipped in its own package. So you can mix and match to your liking!</p>
        </div>
        <div class="modules">
          ${f(
      this.modules,
      () => u`
      
        ${L(
        this.modules,
        (t) => t.alias,
        (t) => u`
                <a href="#" href="${t.link}" class="module" target="_blank">
                    <div class="module-icon">
                        <umb-icon name="${t.icon}"></umb-icon>
                    </div>
                    <p class="module-title">${t.title}</p>
            ${f(t.status === "Disabled", () => u`<p class="module-status module-status-disabled">Disabled</p>`)}
            ${f(t.status === "Installed", () => u`<p class="module-status module-status-installed">Installed</p>`)}
            ${f(t.status === "NotInstalled", () => u`<p class="module-status">Not installed</p>`)}
                </a>`
      )}
      `
    )}
            
        </div>
      </div>
    `;
  }
};
m.styles = [
  W`
      .welcomeDashboard h1 {
    text-align: center;
}

.welcomeDashboard .intro {
    text-align: center;
}

.welcomeDashboard .modules {
    display: flex;
}

    .welcomeDashboard .modules .module {
        padding: 10px;
        margin: 10px;
        background-color: white;
        border-radius: 3px;
        width: 20%;
        color: black;
        text-decoration: none;
    }

.welcomeDashboard .module-icon {
    text-align: center;
    padding-bottom: 10px;
    padding-top: 10px;
}

.welcomeDashboard .module-title {
    text-align: center;
    font-weight: 600;
}

.welcomeDashboard .module-status {
    text-align: center;
    color: red;
}

.welcomeDashboard .module-status-installed {
    color: green;
}

.welcomeDashboard .module-status-disabled {
    color: #f0ac00;
}
    `
];
q([
  F()
], m.prototype, "modules", 2);
m = q([
  G("welcome-dashboard")
], m);
const N = m, ye = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  get MyWelcomeDashboardElement() {
    return m;
  },
  default: N
}, Symbol.toStringTag, { value: "Module" })), be = {
  type: "dashboard",
  alias: "seoToolkitWelcomeDashboard",
  name: "Welcome",
  meta: {
    pathname: "welcome"
  },
  element: N,
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
}, Se = {
  type: "sectionSidebarApp",
  kind: "menuWithEntityActions",
  alias: "seoToolkit.sidebar",
  name: "SeoToolkit",
  meta: {
    label: "SeoToolkit",
    menu: "SeoToolkitMenu"
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
}, E = "seoToolkit_tree_root", S = "seoToolkit-module", M = "seoToolkit_tree_store_context";
class ke extends X {
  constructor(e) {
    super(e, {
      getRootItems: P,
      getChildrenOf: Ee,
      getAncestorsOf: ge,
      mapper: we
    });
  }
}
const P = () => k.getUmbracoSeoToolkitTreeInfoRoot(), Ee = (t) => t.parent.unique === null ? P() : k.getUmbracoSeoToolkitTreeInfoChildren({
  parentId: t.parent.unique,
  skip: t.skip,
  take: t.take
}), ge = (t) => k.getUmbracoSeoToolkitTreeInfoAncestors({
  descendantId: t.treeItem.unique
}), we = (t) => {
  var e;
  return {
    unique: t.id,
    parent: {
      unique: ((e = t.parent) == null ? void 0 : e.id) || null,
      entityType: t.parent ? S : E
    },
    name: t.name,
    entityType: S,
    hasChildren: t.hasChildren,
    isFolder: !1,
    icon: "icon-book-alt"
  };
};
class _e extends Y {
  constructor(e) {
    super(e, ke, M);
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: E,
      name: "SeoToolkit",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
class Re extends Q {
  constructor(e) {
    super(e, M);
  }
}
const Ce = {
  type: "repository",
  alias: "SeoToolkitTreeRepository",
  name: "SeoToolkit Tree repository",
  api: _e
}, Ie = {
  type: "treeStore",
  alias: "SeoToolkitTreeStore",
  name: "SeoToolkit tree Store",
  api: Re
}, Oe = {
  type: "tree",
  kind: "default",
  alias: "SeoToolkitTree",
  name: "SeoToolkit tree",
  meta: {
    repositoryAlias: "SeoToolkitTreeRepository"
  }
}, De = {
  type: "treeItem",
  kind: "default",
  alias: "SeoToolkitTreeItem",
  name: "SeoToolkit Tree Item",
  forEntityTypes: [
    S,
    E
  ]
}, Ae = {
  type: "menu",
  alias: "SeoToolkitMenu",
  name: "SeoToolkit Menu"
}, xe = {
  type: "menuItem",
  kind: "tree",
  alias: "SeoToolkitMenuItem",
  name: "SeoToolkit Menu Item",
  weight: 400,
  meta: {
    label: "Times",
    icon: "icon-alarm-clock",
    entityType: E,
    menus: ["SeoToolkitMenu"],
    treeAlias: "SeoToolkitTree",
    hideTreeRoot: !0
  }
}, je = {
  type: "workspace",
  alias: "seoToolkit.module.workspace",
  name: "SeoToolkit Module Workspace",
  element: () => import("./seoToolkitModuleWorkspace.element-CAU_pF-u.js"),
  meta: {
    entityType: S
  }
}, ve = {
  type: "workspaceView",
  alias: "seotoolkit.workspace.info",
  name: "default view",
  js: () => Promise.resolve().then(() => ye),
  weight: 300,
  meta: {
    icon: "icon-alarm-clock",
    pathname: "content",
    label: "time"
  },
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "seoToolkit.module.workspace"
    }
  ]
};
class Ue extends Z {
  constructor(e, o) {
    super(e, o), console.log(o), o.config.match === "Yes" && (this.permitted = !0, o.onChange());
  }
}
const qe = {
  type: "condition",
  name: "Workspace Entity Id Condition",
  alias: "SeoToolkit.WorkspaceEntityIdCondition",
  api: Ue
}, Fe = (t, e) => {
  t.consumeContext(J, (o) => {
    const r = o.getOpenApiConfiguration();
    d.BASE = r.base, d.WITH_CREDENTIALS = r.withCredentials, d.CREDENTIALS = r.credentials, d.TOKEN = r.token;
  }), e.register(ee), e.register(be), e.register(Se), e.register(qe), e.registerMany([Ce, Oe, Ie, De, Ae, xe, je, ve]);
};
export {
  S,
  Fe as o
};
//# sourceMappingURL=index-DUgqX8AQ.js.map
