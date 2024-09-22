import { UMB_AUTH_CONTEXT as w } from "@umbraco-cms/backoffice/auth";
import { UmbTreeServerDataSourceBase as q, UmbTreeRepositoryBase as j, UmbUniqueTreeStore as N } from "@umbraco-cms/backoffice/tree";
const U = {
  type: "section",
  alias: "SeoToolkit",
  name: "SeoToolkit",
  weight: 10,
  meta: {
    label: "SeoToolkit",
    pathname: "SeoToolkit"
  }
}, D = {
  type: "dashboard",
  alias: "seoToolkitWelcomeDashboard",
  name: "Welcome",
  meta: {
    pathname: "welcome"
  },
  element: () => import("./welcomeDashboard.element-CM4VSYm_.js"),
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "SeoToolkit"
    }
  ]
};
class R extends Error {
  constructor(e, o, r) {
    super(r), this.name = "ApiError", this.url = o.url, this.status = o.status, this.statusText = o.statusText, this.body = o.body, this.request = e;
  }
}
class x extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class H {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((o, r) => {
      this._resolve = o, this._reject = r;
      const n = (a) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isResolved = !0, this._resolve && this._resolve(a));
      }, s = (a) => {
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
      }), e(n, s, i);
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new x("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
class _ {
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
    request: new _(),
    response: new _()
  }
}, u = (t) => typeof t == "string", m = (t) => u(t) && t !== "", y = (t) => t instanceof Blob, g = (t) => t instanceof FormData, P = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, v = (t) => {
  const e = [], o = (n, s) => {
    e.push(`${encodeURIComponent(n)}=${encodeURIComponent(String(s))}`);
  }, r = (n, s) => {
    s != null && (s instanceof Date ? o(n, s.toISOString()) : Array.isArray(s) ? s.forEach((i) => r(n, i)) : typeof s == "object" ? Object.entries(s).forEach(([i, a]) => r(`${n}[${i}]`, a)) : o(n, s));
  };
  return Object.entries(t).forEach(([n, s]) => r(n, s)), e.length ? `?${e.join("&")}` : "";
}, B = (t, e) => {
  const o = encodeURI, r = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (s, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? o(String(e.path[i])) : s;
  }), n = t.BASE + r;
  return e.query ? n + v(e.query) : n;
}, L = (t) => {
  if (t.formData) {
    const e = new FormData(), o = (r, n) => {
      u(n) || y(n) ? e.append(r, n) : e.append(r, JSON.stringify(n));
    };
    return Object.entries(t.formData).filter(([, r]) => r != null).forEach(([r, n]) => {
      Array.isArray(n) ? n.forEach((s) => o(r, s)) : o(r, n);
    }), e;
  }
}, T = async (t, e) => typeof e == "function" ? e(t) : e, M = async (t, e) => {
  const [o, r, n, s] = await Promise.all([
    // @ts-ignore
    T(e, t.TOKEN),
    // @ts-ignore
    T(e, t.USERNAME),
    // @ts-ignore
    T(e, t.PASSWORD),
    // @ts-ignore
    T(e, t.HEADERS)
  ]), i = Object.entries({
    Accept: "application/json",
    ...s,
    ...e.headers
  }).filter(([, a]) => a != null).reduce((a, [l, c]) => ({
    ...a,
    [l]: String(c)
  }), {});
  if (m(o) && (i.Authorization = `Bearer ${o}`), m(r) && m(n)) {
    const a = P(`${r}:${n}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : y(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : u(e.body) ? i["Content-Type"] = "text/plain" : g(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, F = (t) => {
  var e, o;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (o = t.mediaType) != null && o.includes("+json") ? JSON.stringify(t.body) : u(t.body) || y(t.body) || g(t.body) ? t.body : JSON.stringify(t.body);
}, $ = async (t, e, o, r, n, s, i) => {
  const a = new AbortController();
  let l = {
    headers: s,
    body: r ?? n,
    method: e.method,
    signal: a.signal
  };
  t.WITH_CREDENTIALS && (l.credentials = t.CREDENTIALS);
  for (const c of t.interceptors.request._fns)
    l = await c(l);
  return i(() => a.abort()), await fetch(o, l);
}, W = (t, e) => {
  if (e) {
    const o = t.headers.get(e);
    if (u(o))
      return o;
  }
}, G = async (t) => {
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
}, K = (t, e) => {
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
    throw new R(t, e, r);
  if (!e.ok) {
    const n = e.status ?? "unknown", s = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new R(
      t,
      e,
      `Generic Error: status: ${n}; status text: ${s}; body: ${i}`
    );
  }
}, h = (t, e) => new H(async (o, r, n) => {
  try {
    const s = B(t, e), i = L(e), a = F(e), l = await M(t, e);
    if (!n.isCancelled) {
      let c = await $(t, e, s, a, i, l, n);
      for (const O of t.interceptors.response._fns)
        c = await O(c);
      const b = await G(c), A = W(c, e.responseHeader);
      let k = b;
      e.responseTransformer && c.ok && (k = await e.responseTransformer(b));
      const E = {
        url: s,
        ok: c.ok,
        status: c.status,
        statusText: c.statusText,
        body: A ?? k
      };
      K(e, E), o(E.body);
    }
  } catch (s) {
    r(s);
  }
});
class p {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return h(d, {
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
    return h(d, {
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
    return h(d, {
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
    return h(d, {
      method: "GET",
      url: "/umbraco/seoToolkit/tree/info/root",
      query: {
        skip: e.skip,
        take: e.take
      }
    });
  }
}
const V = {
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
}, S = "seoToolkit_tree_root", f = "seoToolkit_tree_entity", I = "seoToolkit_tree_store_context";
class z extends q {
  constructor(e) {
    super(e, {
      getRootItems: C,
      getChildrenOf: J,
      getAncestorsOf: X,
      mapper: Q
    });
  }
}
const C = () => p.getUmbracoSeoToolkitTreeInfoRoot(), J = (t) => t.parent.unique === null ? C() : p.getUmbracoSeoToolkitTreeInfoChildren({
  parentId: t.parent.unique,
  skip: t.skip,
  take: t.take
}), X = (t) => p.getUmbracoSeoToolkitTreeInfoAncestors({
  descendantId: t.treeItem.unique
}), Q = (t) => {
  var e;
  return {
    unique: t.id,
    parent: {
      unique: ((e = t.parent) == null ? void 0 : e.id) || null,
      entityType: t.parent ? f : S
    },
    name: t.name,
    entityType: f,
    hasChildren: t.hasChildren,
    isFolder: !1,
    icon: "icon-book-alt"
  };
};
class Y extends j {
  constructor(e) {
    super(e, z, I);
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: S,
      name: "SeoToolkit",
      hasChildren: !0,
      isFolder: !0
    } };
  }
}
class Z extends N {
  constructor(e) {
    super(e, I);
  }
}
const ee = {
  type: "repository",
  alias: "SeoToolkitTreeRepository",
  name: "SeoToolkit Tree repository",
  api: Y
}, te = {
  type: "treeStore",
  alias: "SeoToolkitTreeStore",
  name: "SeoToolkit tree Store",
  api: Z
}, oe = {
  type: "tree",
  kind: "default",
  alias: "SeoToolkitTree",
  name: "SeoToolkit tree",
  meta: {
    repositoryAlias: "SeoToolkitTreeRepository"
  }
}, re = {
  type: "treeItem",
  kind: "default",
  alias: "SeoToolkitTreeItem",
  name: "SeoToolkit Tree Item",
  forEntityTypes: [
    f,
    S
  ]
}, ne = {
  type: "menu",
  alias: "SeoToolkitMenu",
  name: "Time Tree Menu",
  meta: {
    label: "Times"
  }
}, se = {
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
}, ce = (t, e) => {
  t.consumeContext(w, (o) => {
    const r = o.getOpenApiConfiguration();
    d.BASE = r.base, d.WITH_CREDENTIALS = r.withCredentials, d.CREDENTIALS = r.credentials, d.TOKEN = r.token;
  }), e.register(U), e.register(D), e.register(V), e.registerMany([ee, oe, te, re, ne, se]);
};
export {
  p as S,
  ce as o
};
//# sourceMappingURL=index-CWy644A_.js.map
