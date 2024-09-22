var S = (t) => {
  throw TypeError(t);
};
var E = (t, e, r) => e.has(t) || S("Cannot " + r);
var y = (t, e, r) => (E(t, e, "read from private field"), r ? r.call(t) : e.get(t)), p = (t, e, r) => e.has(t) ? S("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), b = (t, e, r, o) => (E(t, e, "write to private field"), o ? o.call(t, r) : e.set(t, r), r);
import { LitElement as D, html as j, css as q, state as P, customElement as N } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as M } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as U } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as H } from "@umbraco-cms/backoffice/resources";
import { O as I } from "./index-BKC2l3XO.js";
class C extends Error {
  constructor(e, r, o) {
    super(o), this.name = "ApiError", this.url = r.url, this.status = r.status, this.statusText = r.statusText, this.body = r.body, this.request = e;
  }
}
class B extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class $ {
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new B("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
const f = (t) => typeof t == "string", g = (t) => f(t) && t !== "", R = (t) => t instanceof Blob, v = (t) => t instanceof FormData, F = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, k = (t) => {
  const e = [], r = (s, n) => {
    e.push(`${encodeURIComponent(s)}=${encodeURIComponent(String(n))}`);
  }, o = (s, n) => {
    n != null && (n instanceof Date ? r(s, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => o(s, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => o(`${s}[${i}]`, a)) : r(s, n));
  };
  return Object.entries(t).forEach(([s, n]) => o(s, n)), e.length ? `?${e.join("&")}` : "";
}, L = (t, e) => {
  const r = encodeURI, o = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? r(String(e.path[i])) : n;
  }), s = t.BASE + o;
  return e.query ? s + k(e.query) : s;
}, W = (t) => {
  if (t.formData) {
    const e = new FormData(), r = (o, s) => {
      f(s) || R(s) ? e.append(o, s) : e.append(o, JSON.stringify(s));
    };
    return Object.entries(t.formData).filter(([, o]) => o != null).forEach(([o, s]) => {
      Array.isArray(s) ? s.forEach((n) => r(o, n)) : r(o, s);
    }), e;
  }
}, m = async (t, e) => typeof e == "function" ? e(t) : e, G = async (t, e) => {
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
  if (g(r) && (i.Authorization = `Bearer ${r}`), g(o) && g(s)) {
    const a = F(`${o}:${s}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : R(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : f(e.body) ? i["Content-Type"] = "text/plain" : v(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, z = (t) => {
  var e, r;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (r = t.mediaType) != null && r.includes("+json") ? JSON.stringify(t.body) : f(t.body) || R(t.body) || v(t.body) ? t.body : JSON.stringify(t.body);
}, J = async (t, e, r, o, s, n, i) => {
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
}, V = (t, e) => {
  if (e) {
    const r = t.headers.get(e);
    if (f(r))
      return r;
  }
}, K = async (t) => {
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
}, Q = (t, e) => {
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
    throw new C(t, e, o);
  if (!e.ok) {
    const s = e.status ?? "unknown", n = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new C(
      t,
      e,
      `Generic Error: status: ${s}; status text: ${n}; body: ${i}`
    );
  }
}, X = (t, e) => new $(async (r, o, s) => {
  try {
    const n = L(t, e), i = W(e), a = z(e), l = await G(t, e);
    if (!s.isCancelled) {
      let c = await J(t, e, n, a, i, l, s);
      for (const x of t.interceptors.response._fns)
        c = await x(c);
      const T = await K(c), A = V(c, e.responseHeader);
      let w = T;
      e.responseTransformer && c.ok && (w = await e.responseTransformer(T));
      const _ = {
        url: n,
        ok: c.ok,
        status: c.status,
        statusText: c.statusText,
        body: A ?? w
      };
      Q(e, _), r(_.body);
    }
  } catch (n) {
    o(n);
  }
});
class Y {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return X(I, {
      method: "GET",
      url: "/umbraco/seoToolkit/modules"
    });
  }
}
var u;
class Z {
  constructor(e) {
    p(this, u);
    b(this, u, e);
  }
  async getModules() {
    return await H(y(this, u), Y.getUmbracoSeoToolkitModules());
  }
}
u = new WeakMap();
var h;
class ee extends U {
  constructor(r) {
    super(r);
    p(this, h);
    b(this, h, new Z(this));
  }
  async getModules() {
    return y(this, h).getModules();
  }
}
h = new WeakMap();
var te = Object.defineProperty, re = Object.getOwnPropertyDescriptor, O = (t, e, r, o) => {
  for (var s = o > 1 ? void 0 : o ? re(e, r) : e, n = t.length - 1, i; n >= 0; n--)
    (i = t[n]) && (s = (o ? i(e, r, s) : i(s)) || s);
  return o && s && te(e, r, s), s;
};
let d = class extends M(D) {
  constructor() {
    super(), new ee(this).getModules().then((t) => {
      console.log(t);
    });
  }
  async getModules() {
    return [];
  }
  render() {
    return j`
      <div>
        <umb-load-indicator ng-if="vm.loading"></umb-load-indicator>

        <h1>Welcome!</h1>
        <div class="intro">
            <p>This is the dashboard for SeoToolkit. The SEO package for Umbraco.</p>
            <p>Here you can see what modules are installed. Each functionality is shipped in its own package. So you can mix and match to your liking!</p>
        </div>
        <div class="modules">
          ${Object.entries(this.getModules()).forEach(() => {
      j`
              <a href="#" ng-href="{{item.link}}" ng-repeat="item in vm.modules" class="module" target="_blank">
                  <div class="module-icon">
                      <umb-icon icon="{{item.icon}}"></umb-icon>
                  </div>
                  <p class="module-title">{{item.title}}</p>
                  <p class="module-status module-status-disabled" ng-if="item.status === 2">Disabled</p>
                  <p class="module-status module-status-installed" ng-if="item.status === 1">Installed</p>
                  <p class="module-status" ng-if="item.status === 0">Not installed</p>
              </a>`;
    })}
            
        </div>
      </div>
    `;
  }
};
d.styles = [
  q`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
O([
  P()
], d.prototype, "modules", 2);
d = O([
  N("welcome-dashboard")
], d);
const le = d;
export {
  d as MyWelcomeDashboardElement,
  le as default
};
//# sourceMappingURL=welcomeDashboard.element-Da_yVKPs.js.map
