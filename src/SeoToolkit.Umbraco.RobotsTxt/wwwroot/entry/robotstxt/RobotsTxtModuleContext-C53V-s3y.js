var O = (t) => {
  throw TypeError(t);
};
var j = (t, e, r) => e.has(t) || O("Cannot " + r);
var d = (t, e, r) => (j(t, e, "read from private field"), r ? r.call(t) : e.get(t)), h = (t, e, r) => e.has(t) ? O("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), f = (t, e, r, s) => (j(t, e, "write to private field"), s ? s.call(t, r) : e.set(t, r), r);
import { UmbControllerBase as N } from "@umbraco-cms/backoffice/class-api";
import { S as P } from "./index-EtzwR68l.js";
import { UmbStringState as B, UmbArrayState as H } from "@umbraco-cms/backoffice/observable-api";
import { UmbContextToken as L } from "@umbraco-cms/backoffice/context-api";
import { tryExecuteAndNotify as k } from "@umbraco-cms/backoffice/resources";
class A extends Error {
  constructor(e, r, s) {
    super(s), this.name = "ApiError", this.url = r.url, this.status = r.status, this.statusText = r.statusText, this.body = r.body, this.request = e;
  }
}
class $ extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class F {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((r, s) => {
      this._resolve = r, this._reject = s;
      const o = (a) => {
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
      }), e(o, n, i);
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new $("Request aborted"));
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
    const r = this._fns.indexOf(e);
    r !== -1 && (this._fns = [...this._fns.slice(0, r), ...this._fns.slice(r + 1)]);
  }
  use(e) {
    this._fns = [...this._fns, e];
  }
}
const y = {
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
}, S = (t) => typeof t == "string", g = (t) => S(t) && t !== "", C = (t) => t instanceof Blob, U = (t) => t instanceof FormData, M = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, G = (t) => {
  const e = [], r = (o, n) => {
    e.push(`${encodeURIComponent(o)}=${encodeURIComponent(String(n))}`);
  }, s = (o, n) => {
    n != null && (n instanceof Date ? r(o, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => s(o, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => s(`${o}[${i}]`, a)) : r(o, n));
  };
  return Object.entries(t).forEach(([o, n]) => s(o, n)), e.length ? `?${e.join("&")}` : "";
}, V = (t, e) => {
  const r = encodeURI, s = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? r(String(e.path[i])) : n;
  }), o = t.BASE + s;
  return e.query ? o + G(e.query) : o;
}, z = (t) => {
  if (t.formData) {
    const e = new FormData(), r = (s, o) => {
      S(o) || C(o) ? e.append(s, o) : e.append(s, JSON.stringify(o));
    };
    return Object.entries(t.formData).filter(([, s]) => s != null).forEach(([s, o]) => {
      Array.isArray(o) ? o.forEach((n) => r(s, n)) : r(s, o);
    }), e;
  }
}, E = async (t, e) => e, J = async (t, e) => {
  const [r, s, o, n] = await Promise.all([
    // @ts-ignore
    E(e, t.TOKEN),
    // @ts-ignore
    E(e, t.USERNAME),
    // @ts-ignore
    E(e, t.PASSWORD),
    // @ts-ignore
    E(e, t.HEADERS)
  ]), i = Object.entries({
    Accept: "application/json",
    ...n,
    ...e.headers
  }).filter(([, a]) => a != null).reduce((a, [u, c]) => ({
    ...a,
    [u]: String(c)
  }), {});
  if (g(r) && (i.Authorization = `Bearer ${r}`), g(s) && g(o)) {
    const a = M(`${s}:${o}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : C(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : S(e.body) ? i["Content-Type"] = "text/plain" : U(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, K = (t) => {
  var e, r;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (r = t.mediaType) != null && r.includes("+json") ? JSON.stringify(t.body) : S(t.body) || C(t.body) || U(t.body) ? t.body : JSON.stringify(t.body);
}, W = async (t, e, r, s, o, n, i) => {
  const a = new AbortController();
  let u = {
    headers: n,
    body: s ?? o,
    method: e.method,
    signal: a.signal
  };
  for (const c of t.interceptors.request._fns)
    u = await c(u);
  return i(() => a.abort()), await fetch(r, u);
}, X = (t, e) => {
  if (e) {
    const r = t.headers.get(e);
    if (S(r))
      return r;
  }
}, Q = async (t) => {
  if (t.status !== 204)
    try {
      const e = t.headers.get("Content-Type");
      if (e) {
        const r = ["application/octet-stream", "application/pdf", "application/zip", "audio/", "image/", "video/"];
        if (e.includes("application/json") || e.includes("+json"))
          return await t.json();
        if (r.some((s) => e.includes(s)))
          return await t.blob();
        if (e.includes("multipart/form-data"))
          return await t.formData();
        if (e.includes("text/"))
          return await t.text();
      }
    } catch (e) {
      console.error(e);
    }
}, Y = (t, e) => {
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
    throw new A(t, e, s);
  if (!e.ok) {
    const o = e.status ?? "unknown", n = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new A(
      t,
      e,
      `Generic Error: status: ${o}; status text: ${n}; body: ${i}`
    );
  }
}, T = (t, e) => new F(async (r, s, o) => {
  try {
    const n = V(t, e), i = z(e), a = K(e), u = await J(t, e);
    if (!o.isCancelled) {
      let c = await W(t, e, n, a, i, u, o);
      for (const D of t.interceptors.response._fns)
        c = await D(c);
      const _ = await Q(c), I = X(c, e.responseHeader);
      let w = _;
      e.responseTransformer && c.ok && (w = await e.responseTransformer(_));
      const x = {
        url: n,
        ok: c.ok,
        status: c.status,
        statusText: c.statusText,
        body: I ?? w
      };
      Y(e, x), r(x.body);
    }
  } catch (n) {
    s(n);
  }
});
class q {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return T(y, {
      method: "GET",
      url: "/umbraco/seoToolkit/modules"
    });
  }
  /**
   * @returns string OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitRobotsTxt() {
    return T(y, {
      method: "GET",
      url: "/umbraco/seoToolkit/robotsTxt"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.requestBody
   * @returns string OK
   * @throws ApiError
   */
  static postUmbracoSeoToolkitRobotsTxt(e = {}) {
    return T(y, {
      method: "POST",
      url: "/umbraco/seoToolkit/robotsTxt",
      body: e.requestBody,
      mediaType: "application/json"
    });
  }
  /**
   * @param data The data for the request.
   * @param data.descendantId
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitTreeInfoAncestors(e = {}) {
    return T(y, {
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
    return T(y, {
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
    return T(y, {
      method: "GET",
      url: "/umbraco/seoToolkit/tree/info/root",
      query: {
        skip: e.skip,
        take: e.take
      }
    });
  }
}
var b;
class Z {
  constructor(e) {
    h(this, b);
    f(this, b, e);
  }
  async getContent() {
    return await k(d(this, b), q.getUmbracoSeoToolkitRobotsTxt());
  }
  async saveContent(e) {
    return await k(d(this, b), q.postUmbracoSeoToolkitRobotsTxt({
      requestBody: {
        skipValidation: !1,
        content: e
      }
    }));
  }
}
b = new WeakMap();
var m;
class ee extends N {
  constructor(r) {
    super(r);
    h(this, m);
    f(this, m, new Z(r));
  }
  async getContent() {
    return d(this, m).getContent();
  }
  async saveContent(r) {
    return d(this, m).saveContent(r);
  }
}
m = new WeakMap();
var p, l, R;
class ce extends N {
  constructor(r) {
    super(r);
    h(this, p);
    h(this, l);
    h(this, R);
    this.workspaceAlias = "seoToolkit.context.robotsTxtModule", f(this, l, new B("")), this.content = d(this, l).asObservable(), f(this, R, new H([], (s) => s.error)), this.validationErrors = d(this, R).asObservable(), f(this, p, new ee(r)), this.provideContext(te, this), d(this, p).getContent().then((s) => {
      d(this, l).setValue(s.data);
    });
  }
  setContent(r) {
    d(this, l).setValue(r);
  }
  submit() {
    var r;
    (r = d(this, p)) == null || r.saveContent(d(this, l).value);
  }
  getEntityType() {
    return P;
  }
}
p = new WeakMap(), l = new WeakMap(), R = new WeakMap();
const te = new L(
  "robotsTxtModuleContext"
);
export {
  te as ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT,
  ce as default
};
//# sourceMappingURL=RobotsTxtModuleContext-C53V-s3y.js.map
