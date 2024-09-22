import { UMB_AUTH_CONTEXT as C } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as _, UmbTreeRepositoryBase as w, UmbUniqueTreeStore as A } from "@umbraco-cms/backoffice/tree";
const I = {
  type: "section",
  alias: "SeoToolkit",
  name: "SeoToolkit",
  weight: 10,
  meta: {
    label: "SeoToolkit",
    pathname: "SeoToolkit"
  }
}, j = {
  type: "dashboard",
  alias: "seoToolkitWelcomeDashboard",
  name: "Welcome",
  meta: {
    pathname: "welcome"
  },
  element: () => import("./welcomeDashboard.element-C2w034I_.js"),
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
};
class S extends Error {
  constructor(e, r, o) {
    super(o), this.name = "ApiError", this.url = r.url, this.status = r.status, this.statusText = r.statusText, this.body = r.body, this.request = e;
  }
}
class q extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class N {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((r, o) => {
      this._resolve = r, this._reject = o;
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
  then(e, r) {
    return this.promise.then(e, r);
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new q("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
class b {
  constructor() {
    this._fns = [];
  }
  eject(e) {
    const r = this._fns.indexOf(e);
    r !== -1 && (this._fns = [...this._fns.slice(0, r), ...this._fns.slice(r + 1)]);
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
    request: new b(),
    response: new b()
  }
}, u = (t) => typeof t == "string", h = (t) => u(t) && t !== "", T = (t) => t instanceof Blob, E = (t) => t instanceof FormData, O = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, D = (t) => {
  const e = [], r = (s, n) => {
    e.push(`${encodeURIComponent(s)}=${encodeURIComponent(String(n))}`);
  }, o = (s, n) => {
    n != null && (n instanceof Date ? r(s, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => o(s, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => o(`${s}[${i}]`, a)) : r(s, n));
  };
  return Object.entries(t).forEach(([s, n]) => o(s, n)), e.length ? `?${e.join("&")}` : "";
}, U = (t, e) => {
  const r = encodeURI, o = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? r(String(e.path[i])) : n;
  }), s = t.BASE + o;
  return e.query ? s + D(e.query) : s;
}, P = (t) => {
  if (t.formData) {
    const e = new FormData(), r = (o, s) => {
      u(s) || T(s) ? e.append(o, s) : e.append(o, JSON.stringify(s));
    };
    return Object.entries(t.formData).filter(([, o]) => o != null).forEach(([o, s]) => {
      Array.isArray(s) ? s.forEach((n) => r(o, n)) : r(o, s);
    }), e;
  }
}, m = async (t, e) => typeof e == "function" ? e(t) : e, H = async (t, e) => {
  const [r, o, s, n] = await Promise.all([
    // @ts-ignore
    m(e, t.TOKEN),
    // @ts-ignore
    m(e, t.USERNAME),
    // @ts-ignore
    m(e, t.PASSWORD),
    // @ts-ignore
    m(e, t.HEADERS)
  ]), i = Object.entries({
    Accept: "application/json",
    ...n,
    ...e.headers
  }).filter(([, a]) => a != null).reduce((a, [l, c]) => ({
    ...a,
    [l]: String(c)
  }), {});
  if (h(r) && (i.Authorization = `Bearer ${r}`), h(o) && h(s)) {
    const a = O(`${o}:${s}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : T(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : u(e.body) ? i["Content-Type"] = "text/plain" : E(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, x = (t) => {
  var e, r;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (r = t.mediaType) != null && r.includes("+json") ? JSON.stringify(t.body) : u(t.body) || T(t.body) || E(t.body) ? t.body : JSON.stringify(t.body);
}, v = async (t, e, r, o, s, n, i) => {
  const a = new AbortController();
  let l = {
    headers: n,
    body: o ?? s,
    method: e.method,
    signal: a.signal
  };
  t.WITH_CREDENTIALS && (l.credentials = t.CREDENTIALS);
  for (const c of t.interceptors.request._fns)
    l = await c(l);
  return i(() => a.abort()), await fetch(r, l);
}, B = (t, e) => {
  if (e) {
    const r = t.headers.get(e);
    if (u(r))
      return r;
  }
}, M = async (t) => {
  if (t.status !== 204)
    try {
      const e = t.headers.get("Content-Type");
      if (e) {
        const r = ["application/octet-stream", "application/pdf", "application/zip", "audio/", "image/", "video/"];
        if (e.includes("application/json") || e.includes("+json"))
          return await t.json();
        if (r.some((o) => e.includes(o)))
          return await t.blob();
        if (e.includes("multipart/form-data"))
          return await t.formData();
        if (e.includes("text/"))
          return await t.text();
      }
    } catch (e) {
      console.error(e);
    }
}, L = (t, e) => {
  const o = {
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
  if (o)
    throw new S(t, e, o);
  if (!e.ok) {
    const s = e.status ?? "unknown", n = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new S(
      t,
      e,
      `Generic Error: status: ${s}; status text: ${n}; body: ${i}`
    );
  }
}, R = (t, e) => new N(async (r, o, s) => {
  try {
    const n = U(t, e), i = P(e), a = x(e), l = await H(t, e);
    if (!s.isCancelled) {
      let c = await v(t, e, n, a, i, l, s);
      for (const k of t.interceptors.response._fns)
        c = await k(c);
      const f = await M(c), g = B(c, e.responseHeader);
      let y = f;
      e.responseTransformer && c.ok && (y = await e.responseTransformer(f));
      const p = {
        url: n,
        ok: c.ok,
        status: c.status,
        statusText: c.statusText,
        body: g ?? y
      };
      L(e, p), r(p.body);
    }
  } catch (n) {
    o(n);
  }
});
class F {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return R(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/modules"
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
    return R(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/tree/info/root",
      query: {
        skip: e.skip,
        take: e.take
      }
    });
  }
}
const $ = {
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
};
class W extends _ {
  constructor(e) {
    super(e, {
      getRootItems: G,
      getChildrenOf: V,
      getAncestorsOf: z,
      mapper: J
    });
  }
}
const G = () => F.getUmbracoSeoToolkitTreeInfoRoot(), V = () => new Promise((t) => {
  t({
    total: 0,
    items: []
  });
}), z = () => new Promise((t) => t([])), J = (t) => ({
  unique: t.id,
  name: t.name,
  parent: {
    unique: null,
    entityType: "root"
  },
  entityType: "seoToolkit",
  hasChildren: t.hasChildren,
  isFolder: !1,
  icon: "icon-alarm-clock"
});
class K extends w {
  constructor(e) {
    super(e, W, "seoToolkitTree");
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: "root",
      name: "time",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
class Q extends A {
  constructor(e) {
    super(e, "seoToolkitTree");
  }
}
const X = {
  type: "repository",
  alias: "SeoToolkitTreeRepository",
  name: "Time Tree repository",
  api: K
}, Y = {
  type: "treeStore",
  alias: "SeoToolkitTreeStore",
  name: "Time tree Store",
  api: Q
}, Z = {
  type: "tree",
  alias: "SeoToolkitTree",
  name: "Time tree",
  meta: {
    repositoryAlias: "SeoToolkitTreeRepository"
  }
}, ee = {
  type: "treeItem",
  kind: "unique",
  alias: "Time.Tree.RootItem",
  name: "Time Tree Item",
  forEntityTypes: [
    "root"
  ]
}, te = {
  type: "menu",
  alias: "SeoToolkitMenu",
  name: "Time Tree Menu",
  meta: {
    label: "Times"
  }
}, re = {
  type: "menuItem",
  kind: "tree",
  alias: "Time.Tree.MenuItem",
  name: "Time Tree Menu Item",
  weight: 400,
  meta: {
    label: "Times",
    icon: "icon-alarm-clock",
    entityType: "root",
    menus: ["SeoToolkitMenu"],
    treeAlias: "SeoToolkitTree",
    hideTreeRoot: !1
  }
}, ne = (t, e) => {
  t.consumeContext(C, (r) => {
    const o = r.getOpenApiConfiguration();
    d.BASE = o.base, d.WITH_CREDENTIALS = o.withCredentials, d.CREDENTIALS = o.credentials, d.TOKEN = o.token;
  }), e.register(I), e.register(j), e.register($), e.registerMany([X, Z, Y, ee, te, re]);
};
export {
  F as S,
  ne as o
};
//# sourceMappingURL=index-jV5LezuH.js.map
