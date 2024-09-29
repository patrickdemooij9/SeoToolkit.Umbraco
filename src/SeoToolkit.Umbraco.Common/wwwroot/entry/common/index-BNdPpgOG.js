var D = (t) => {
  throw TypeError(t);
};
var j = (t, e, o) => e.has(t) || D("Cannot " + o);
var _ = (t, e, o) => (j(t, e, "read from private field"), o ? o.call(t) : e.get(t)), g = (t, e, o) => e.has(t) ? D("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, o), w = (t, e, o, s) => (j(t, e, "write to private field"), s ? s.call(t, o) : e.set(t, o), o);
import { LitElement as F, html as u, when as f, repeat as K, css as G, state as V, customElement as z } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as X } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as M } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as J } from "@umbraco-cms/backoffice/resources";
import { UMB_WORKSPACE_CONTEXT as N } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken as Y } from "@umbraco-cms/backoffice/context-api";
import { UMB_AUTH_CONTEXT as Q } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as Z, UmbTreeRepositoryBase as ee, UmbUniqueTreeStore as te } from "@umbraco-cms/backoffice/tree";
import { UmbConditionBase as oe } from "@umbraco-cms/backoffice/extension-registry";
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
class v extends Error {
  constructor(e, o, s) {
    super(s), this.name = "ApiError", this.url = o.url, this.status = o.status, this.statusText = o.statusText, this.body = o.body, this.request = e;
  }
}
class re extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class ne {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((o, s) => {
      this._resolve = o, this._reject = s;
      const r = (a) => {
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
      }), e(r, n, i);
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new re("Request aborted"));
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
}, T = (t) => typeof t == "string", C = (t) => T(t) && t !== "", R = (t) => t instanceof Blob, q = (t) => t instanceof FormData, ie = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, ae = (t) => {
  const e = [], o = (r, n) => {
    e.push(`${encodeURIComponent(r)}=${encodeURIComponent(String(n))}`);
  }, s = (r, n) => {
    n != null && (n instanceof Date ? o(r, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => s(r, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => s(`${r}[${i}]`, a)) : o(r, n));
  };
  return Object.entries(t).forEach(([r, n]) => s(r, n)), e.length ? `?${e.join("&")}` : "";
}, le = (t, e) => {
  const o = encodeURI, s = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? o(String(e.path[i])) : n;
  }), r = t.BASE + s;
  return e.query ? r + ae(e.query) : r;
}, ce = (t) => {
  if (t.formData) {
    const e = new FormData(), o = (s, r) => {
      T(r) || R(r) ? e.append(s, r) : e.append(s, JSON.stringify(r));
    };
    return Object.entries(t.formData).filter(([, s]) => s != null).forEach(([s, r]) => {
      Array.isArray(r) ? r.forEach((n) => o(s, n)) : o(s, r);
    }), e;
  }
}, y = async (t, e) => typeof e == "function" ? e(t) : e, de = async (t, e) => {
  const [o, s, r, n] = await Promise.all([
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
  if (C(o) && (i.Authorization = `Bearer ${o}`), C(s) && C(r)) {
    const a = ie(`${s}:${r}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : R(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : T(e.body) ? i["Content-Type"] = "text/plain" : q(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, ue = (t) => {
  var e, o;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (o = t.mediaType) != null && o.includes("+json") ? JSON.stringify(t.body) : T(t.body) || R(t.body) || q(t.body) ? t.body : JSON.stringify(t.body);
}, me = async (t, e, o, s, r, n, i) => {
  const a = new AbortController();
  let c = {
    headers: n,
    body: s ?? r,
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
    if (T(o))
      return o;
  }
}, pe = async (t) => {
  if (t.status !== 204)
    try {
      const e = t.headers.get("Content-Type");
      if (e) {
        const o = ["application/octet-stream", "application/pdf", "application/zip", "audio/", "image/", "video/"];
        if (e.includes("application/json") || e.includes("+json"))
          return await t.json();
        if (o.some((s) => e.includes(s)))
          return await t.blob();
        if (e.includes("multipart/form-data"))
          return await t.formData();
        if (e.includes("text/"))
          return await t.text();
      }
    } catch (e) {
      console.error(e);
    }
}, Te = (t, e) => {
  const s = {
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
  if (s)
    throw new v(t, e, s);
  if (!e.ok) {
    const r = e.status ?? "unknown", n = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new v(
      t,
      e,
      `Generic Error: status: ${r}; status text: ${n}; body: ${i}`
    );
  }
}, b = (t, e) => new ne(async (o, s, r) => {
  try {
    const n = le(t, e), i = ce(e), a = ue(e), c = await de(t, e);
    if (!r.isCancelled) {
      let l = await me(t, e, n, a, i, c, r);
      for (const $ of t.interceptors.response._fns)
        l = await $(l);
      const I = await pe(l), W = he(l, e.responseHeader);
      let x = I;
      e.responseTransformer && l.ok && (x = await e.responseTransformer(I));
      const A = {
        url: n,
        ok: l.ok,
        status: l.status,
        statusText: l.statusText,
        body: W ?? x
      };
      Te(e, A), o(A.body);
    }
  } catch (n) {
    s(n);
  }
});
class S {
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
class fe {
  constructor(e) {
    g(this, h);
    w(this, h, e);
  }
  async getModules() {
    return await J(_(this, h), S.getUmbracoSeoToolkitModules());
  }
}
h = new WeakMap();
var p;
class ye extends M {
  constructor(o) {
    super(o);
    g(this, p);
    w(this, p, new fe(this));
  }
  async getModules() {
    return _(this, p).getModules();
  }
}
p = new WeakMap();
const k = "seoToolkit_tree_root", E = "seoToolkit-module", be = "seoToolkit-robotstxt", P = "seoToolkit_tree_store_context";
class Se extends M {
  constructor(e) {
    super(e), this.workspaceAlias = "seoToolkit.module.workspace", this.provideContext(O, this), this.provideContext(N, this);
  }
  getEntityType() {
    return E;
  }
}
const O = new Y(
  "SeoToolkitModuleContext"
), ke = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  SEOTOOLKIT_MODULES_CONTEXT_TOKEN: O,
  default: Se
}, Symbol.toStringTag, { value: "Module" }));
var Ee = Object.defineProperty, _e = Object.getOwnPropertyDescriptor, L = (t, e, o, s) => {
  for (var r = s > 1 ? void 0 : s ? _e(e, o) : e, n = t.length - 1, i; n >= 0; n--)
    (i = t[n]) && (r = (s ? i(e, o, r) : i(r)) || r);
  return s && r && Ee(e, o, r), r;
};
let m = class extends X(F) {
  constructor() {
    super(), this.consumeContext(O, () => {
      console.log("Hello?");
    });
  }
  connectedCallback() {
    super.connectedCallback(), new ye(this).getModules().then((t) => {
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
      
        ${K(
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
  G`
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
L([
  V()
], m.prototype, "modules", 2);
m = L([
  z("welcome-dashboard")
], m);
const B = m, ge = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  get MyWelcomeDashboardElement() {
    return m;
  },
  default: B
}, Symbol.toStringTag, { value: "Module" })), we = {
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
}, Ce = {
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
class Re extends Z {
  constructor(e) {
    super(e, {
      getRootItems: H,
      getChildrenOf: Oe,
      getAncestorsOf: Ie,
      mapper: xe
    });
  }
}
const H = () => S.getUmbracoSeoToolkitTreeInfoRoot(), Oe = (t) => t.parent.unique === null ? H() : S.getUmbracoSeoToolkitTreeInfoChildren({
  parentId: t.parent.unique,
  skip: t.skip,
  take: t.take
}), Ie = (t) => S.getUmbracoSeoToolkitTreeInfoAncestors({
  descendantId: t.treeItem.unique
}), xe = (t) => {
  var s;
  const e = t.id === "CDF429D1-2380-4AC2-AC3E-22D619EE4529".toLowerCase(), o = e ? E : be;
  return {
    unique: t.id,
    parent: {
      unique: ((s = t.parent) == null ? void 0 : s.id) || null,
      entityType: t.parent ? o : k
    },
    name: t.name,
    entityType: o,
    hasChildren: t.hasChildren,
    isFolder: !1,
    icon: e ? "icon-book" : "icon-book-alt"
  };
};
class Ae extends ee {
  constructor(e) {
    super(e, Re, P);
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: k,
      name: "SeoToolkit",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
class De extends te {
  constructor(e) {
    super(e, P);
  }
}
const je = {
  type: "repository",
  alias: "SeoToolkitTreeRepository",
  name: "SeoToolkit Tree repository",
  api: Ae
}, ve = {
  type: "treeStore",
  alias: "SeoToolkitTreeStore",
  name: "SeoToolkit tree Store",
  api: De
}, Ue = {
  type: "tree",
  kind: "default",
  alias: "SeoToolkitTree",
  name: "SeoToolkit tree",
  meta: {
    repositoryAlias: "SeoToolkitTreeRepository"
  }
}, Me = {
  type: "treeItem",
  kind: "default",
  alias: "SeoToolkitTreeItem",
  name: "SeoToolkit Tree Item",
  forEntityTypes: [
    E,
    k
  ]
}, Ne = {
  type: "menu",
  alias: "SeoToolkitMenu",
  name: "SeoToolkit Menu"
}, qe = {
  type: "menuItem",
  kind: "tree",
  alias: "SeoToolkitMenuItem",
  name: "SeoToolkit Menu Item",
  weight: 400,
  meta: {
    label: "Times",
    icon: "icon-alarm-clock",
    entityType: k,
    menus: ["SeoToolkitMenu"],
    treeAlias: "SeoToolkitTree",
    hideTreeRoot: !0
  }
}, Pe = {
  type: "workspace",
  alias: "seoToolkit.module.workspace",
  name: "SeoToolkit Module Workspace",
  element: () => import("./seoToolkitModuleWorkspace.element-DdkQSU56.js"),
  meta: {
    entityType: E
  }
}, Le = {
  type: "workspaceContext",
  alias: "seoToolkit.module.context",
  name: "SeoTOolkit workspace context",
  js: () => Promise.resolve().then(() => ke),
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
  js: () => Promise.resolve().then(() => ge),
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
}, He = [je, ve, Ue, Me, Ne, qe, Pe, Le, Be];
class We extends oe {
  constructor(e, o) {
    super(e, o), this.consumeContext(N, (s) => {
      console.log(s);
    }), console.log(o), o.config.match === "Yes" && (this.permitted = !0, o.onChange());
  }
}
const $e = {
  type: "condition",
  name: "Workspace Entity Id Condition",
  alias: "SeoToolkit.WorkspaceEntityIdCondition",
  api: We
}, et = (t, e) => {
  t.consumeContext(Q, (o) => {
    const s = o.getOpenApiConfiguration();
    d.BASE = s.base, d.WITH_CREDENTIALS = s.withCredentials, d.CREDENTIALS = s.credentials, d.TOKEN = s.token;
  }), e.register(se), e.register(we), e.register(Ce), e.register($e), e.registerMany(He);
};
export {
  Se as S,
  et as o
};
//# sourceMappingURL=index-BNdPpgOG.js.map
