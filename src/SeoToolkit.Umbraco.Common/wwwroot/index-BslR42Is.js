var D = (t) => {
  throw TypeError(t);
};
var v = (t, e, o) => e.has(t) || D("Cannot " + o);
var w = (t, e, o) => (v(t, e, "read from private field"), o ? o.call(t) : e.get(t)), g = (t, e, o) => e.has(t) ? D("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, o), _ = (t, e, o, r) => (v(t, e, "write to private field"), r ? r.call(t, o) : e.set(t, o), o);
import { LitElement as F, html as u, when as y, repeat as K, css as V, state as G, customElement as z } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as J } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as M } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as X } from "@umbraco-cms/backoffice/resources";
import { UMB_WORKSPACE_CONTEXT as Y } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken as Q } from "@umbraco-cms/backoffice/context-api";
import { UMB_AUTH_CONTEXT as Z } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as ee, UmbTreeRepositoryBase as te, UmbUniqueTreeStore as oe } from "@umbraco-cms/backoffice/tree";
import { UmbConditionBase as re } from "@umbraco-cms/backoffice/extension-registry";
const se = {
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
class ne extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class ie {
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new ne("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
class U {
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
    request: new U(),
    response: new U()
  }
}, f = (t) => typeof t == "string", C = (t) => f(t) && t !== "", R = (t) => t instanceof Blob, N = (t) => t instanceof FormData, ae = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, le = (t) => {
  const e = [], o = (s, n) => {
    e.push(`${encodeURIComponent(s)}=${encodeURIComponent(String(n))}`);
  }, r = (s, n) => {
    n != null && (n instanceof Date ? o(s, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => r(s, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => r(`${s}[${i}]`, a)) : o(s, n));
  };
  return Object.entries(t).forEach(([s, n]) => r(s, n)), e.length ? `?${e.join("&")}` : "";
}, ce = (t, e) => {
  const o = encodeURI, r = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? o(String(e.path[i])) : n;
  }), s = t.BASE + r;
  return e.query ? s + le(e.query) : s;
}, de = (t) => {
  if (t.formData) {
    const e = new FormData(), o = (r, s) => {
      f(s) || R(s) ? e.append(r, s) : e.append(r, JSON.stringify(s));
    };
    return Object.entries(t.formData).filter(([, r]) => r != null).forEach(([r, s]) => {
      Array.isArray(s) ? s.forEach((n) => o(r, n)) : o(r, s);
    }), e;
  }
}, b = async (t, e) => typeof e == "function" ? e(t) : e, ue = async (t, e) => {
  const [o, r, s, n] = await Promise.all([
    // @ts-ignore
    b(e, t.TOKEN),
    // @ts-ignore
    b(e, t.USERNAME),
    // @ts-ignore
    b(e, t.PASSWORD),
    // @ts-ignore
    b(e, t.HEADERS)
  ]), i = Object.entries({
    Accept: "application/json",
    ...n,
    ...e.headers
  }).filter(([, a]) => a != null).reduce((a, [c, l]) => ({
    ...a,
    [c]: String(l)
  }), {});
  if (C(o) && (i.Authorization = `Bearer ${o}`), C(r) && C(s)) {
    const a = ae(`${r}:${s}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : R(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : f(e.body) ? i["Content-Type"] = "text/plain" : N(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, me = (t) => {
  var e, o;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (o = t.mediaType) != null && o.includes("+json") ? JSON.stringify(t.body) : f(t.body) || R(t.body) || N(t.body) ? t.body : JSON.stringify(t.body);
}, pe = async (t, e, o, r, s, n, i) => {
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
}, he = (t, e) => {
  if (e) {
    const o = t.headers.get(e);
    if (f(o))
      return o;
  }
}, Te = async (t) => {
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
}, fe = (t, e) => {
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
}, k = (t, e) => new ie(async (o, r, s) => {
  try {
    const n = ce(t, e), i = de(e), a = me(e), c = await ue(t, e);
    if (!s.isCancelled) {
      let l = await pe(t, e, n, a, i, c, s);
      for (const $ of t.interceptors.response._fns)
        l = await $(l);
      const I = await Te(l), L = he(l, e.responseHeader);
      let A = I;
      e.responseTransformer && l.ok && (A = await e.responseTransformer(I));
      const x = {
        url: n,
        ok: l.ok,
        status: l.status,
        statusText: l.statusText,
        body: L ?? A
      };
      fe(e, x), o(x.body);
    }
  } catch (n) {
    r(n);
  }
});
class S {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return k(d, {
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
    return k(d, {
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
    return k(d, {
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
    return k(d, {
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
class ye {
  constructor(e) {
    g(this, h);
    _(this, h, e);
  }
  async getModules() {
    return await X(w(this, h), S.getUmbracoSeoToolkitModules());
  }
}
h = new WeakMap();
var T;
class be extends M {
  constructor(o) {
    super(o);
    g(this, T);
    _(this, T, new ye(this));
  }
  async getModules() {
    return w(this, T).getModules();
  }
}
T = new WeakMap();
const E = "seoToolkit_tree_root", p = "seoToolkit-module", q = "seoToolkit_tree_store_context";
class ke extends M {
  constructor(e) {
    super(e), this.workspaceAlias = "seoToolkit.module.workspace", this.provideContext(O, this), this.provideContext(Y, this);
  }
  getEntityType() {
    return p;
  }
}
const O = new Q(
  "SeoToolkitModuleContext"
), Se = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  SEOTOOLKIT_MODULES_CONTEXT_TOKEN: O,
  default: ke
}, Symbol.toStringTag, { value: "Module" }));
var Ee = Object.defineProperty, we = Object.getOwnPropertyDescriptor, P = (t, e, o, r) => {
  for (var s = r > 1 ? void 0 : r ? we(e, o) : e, n = t.length - 1, i; n >= 0; n--)
    (i = t[n]) && (s = (r ? i(e, o, s) : i(s)) || s);
  return r && s && Ee(e, o, s), s;
};
let m = class extends J(F) {
  constructor() {
    super(), this.consumeContext(O, () => {
      console.log("Hello?");
    });
  }
  connectedCallback() {
    super.connectedCallback(), new be(this).getModules().then((t) => {
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
          ${y(
      this.modules,
      () => u`
      
        ${K(
        this.modules,
        (t) => t.alias,
        (t) => u`
                <a href="#" href="${t.link}" class="module" target="_blank">
                    <div class="module-icon">
                        <umb-icon name="${t.icon}"></umb-icon>
                    </div>
                    <p class="module-title">${t.title}</p>
            ${y(t.status === "Disabled", () => u`<p class="module-status module-status-disabled">Disabled</p>`)}
            ${y(t.status === "Installed", () => u`<p class="module-status module-status-installed">Installed</p>`)}
            ${y(t.status === "NotInstalled", () => u`<p class="module-status">Not installed</p>`)}
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
  V`
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
P([
  G()
], m.prototype, "modules", 2);
m = P([
  z("welcome-dashboard")
], m);
const B = m, W = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  get MyWelcomeDashboardElement() {
    return m;
  },
  default: B
}, Symbol.toStringTag, { value: "Module" })), ge = {
  type: "dashboard",
  alias: "seoToolkitWelcomeDashboard",
  name: "Welcome",
  meta: {
    pathname: "welcome"
  },
  element: B,
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
}, _e = {
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
};
class Ce extends ee {
  constructor(e) {
    super(e, {
      getRootItems: H,
      getChildrenOf: Re,
      getAncestorsOf: Oe,
      mapper: Ie
    });
  }
}
const H = () => S.getUmbracoSeoToolkitTreeInfoRoot(), Re = (t) => t.parent.unique === null ? H() : S.getUmbracoSeoToolkitTreeInfoChildren({
  parentId: t.parent.unique,
  skip: t.skip,
  take: t.take
}), Oe = (t) => S.getUmbracoSeoToolkitTreeInfoAncestors({
  descendantId: t.treeItem.unique
}), Ie = (t) => {
  var e;
  return {
    unique: t.id,
    parent: {
      unique: ((e = t.parent) == null ? void 0 : e.id) || null,
      entityType: t.parent ? p : E
    },
    name: t.name,
    entityType: p,
    hasChildren: t.hasChildren,
    isFolder: !1,
    icon: "icon-book-alt"
  };
};
class Ae extends te {
  constructor(e) {
    super(e, Ce, q);
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
class xe extends oe {
  constructor(e) {
    super(e, q);
  }
}
const De = {
  type: "repository",
  alias: "SeoToolkitTreeRepository",
  name: "SeoToolkit Tree repository",
  api: Ae
}, ve = {
  type: "treeStore",
  alias: "SeoToolkitTreeStore",
  name: "SeoToolkit tree Store",
  api: xe
}, je = {
  type: "tree",
  kind: "default",
  alias: "SeoToolkitTree",
  name: "SeoToolkit tree",
  meta: {
    repositoryAlias: "SeoToolkitTreeRepository"
  }
}, Ue = {
  type: "treeItem",
  kind: "default",
  alias: "SeoToolkitTreeItem",
  name: "SeoToolkit Tree Item",
  forEntityTypes: [
    p,
    E
  ]
}, Me = {
  type: "menu",
  alias: "SeoToolkitMenu",
  name: "SeoToolkit Menu"
}, Ne = {
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
}, qe = {
  type: "workspace",
  alias: "seoToolkit.module.workspace",
  name: "SeoToolkit Module Workspace",
  element: () => import("./seoToolkitModuleWorkspace.element-MdruyYN3.js"),
  meta: {
    entityType: p
  }
}, Pe = {
  type: "workspaceContext",
  alias: "seoToolkit.module.context",
  name: "SeoTOolkit workspace context",
  js: () => Promise.resolve().then(() => Se),
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "seoToolkit.module.workspace"
    }
  ]
}, Be = {
  type: "workspaceView",
  alias: "seotoolkit.workspace.info",
  name: "default view",
  js: () => Promise.resolve().then(() => W),
  weight: 300,
  meta: {
    icon: "icon-alarm-clock",
    pathname: "default",
    label: "time"
  },
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "seoToolkit.module.workspace"
    }
  ]
}, We = {
  type: "workspaceView",
  alias: "seotoolkit.workspace.info2",
  name: "default view",
  js: () => Promise.resolve().then(() => W),
  weight: 300,
  meta: {
    icon: "icon-alarm-clock",
    pathname: "default2",
    label: "time2"
  },
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "seoToolkit.module.workspace"
    },
    {
      alias: "SeoToolkit.WorkspaceEntityIdCondition",
      match: "20A2086E-7D72-44BA-B97B-5836CAF6E28E"
    }
  ]
};
class He extends re {
  constructor(e, o) {
    super(e, o), console.log(o), o.config.match === "Yes" && (this.permitted = !0, o.onChange());
  }
}
const Le = {
  type: "condition",
  name: "Workspace Entity Id Condition",
  alias: "SeoToolkit.WorkspaceEntityIdCondition",
  api: He
}, Ze = (t, e) => {
  t.consumeContext(Z, (o) => {
    const r = o.getOpenApiConfiguration();
    d.BASE = r.base, d.WITH_CREDENTIALS = r.withCredentials, d.CREDENTIALS = r.credentials, d.TOKEN = r.token;
  }), e.register(se), e.register(ge), e.register(_e), e.register(Le), e.registerMany([De, je, ve, Ue, Me, Ne, qe, Be, We, Pe]);
};
export {
  ke as S,
  Ze as o
};
//# sourceMappingURL=index-BslR42Is.js.map
