var T = (t) => {
  throw TypeError(t);
};
var w = (t, e, s) => e.has(t) || T("Cannot " + s);
var y = (t, e, s) => (w(t, e, "read from private field"), s ? s.call(t) : e.get(t)), p = (t, e, s) => e.has(t) ? T("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, s), b = (t, e, s, o) => (w(t, e, "write to private field"), o ? o.call(t, s) : e.set(t, s), s);
import { LitElement as N, html as v, css as P, state as q, customElement as M } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as H } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as U } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as I } from "@umbraco-cms/backoffice/resources";
class j extends Error {
  constructor(e, s, o) {
    super(o), this.name = "ApiError", this.url = s.url, this.status = s.status, this.statusText = s.statusText, this.body = s.body, this.request = e;
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
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((s, o) => {
      this._resolve = s, this._reject = o;
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
  then(e, s) {
    return this.promise.then(e, s);
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
class C {
  constructor() {
    this._fns = [];
  }
  eject(e) {
    const s = this._fns.indexOf(e);
    s !== -1 && (this._fns = [...this._fns.slice(0, s), ...this._fns.slice(s + 1)]);
  }
  use(e) {
    this._fns = [...this._fns, e];
  }
}
const F = {
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
    request: new C(),
    response: new C()
  }
}, f = (t) => typeof t == "string", g = (t) => f(t) && t !== "", R = (t) => t instanceof Blob, A = (t) => t instanceof FormData, L = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, k = (t) => {
  const e = [], s = (r, n) => {
    e.push(`${encodeURIComponent(r)}=${encodeURIComponent(String(n))}`);
  }, o = (r, n) => {
    n != null && (n instanceof Date ? s(r, n.toISOString()) : Array.isArray(n) ? n.forEach((i) => o(r, i)) : typeof n == "object" ? Object.entries(n).forEach(([i, a]) => o(`${r}[${i}]`, a)) : s(r, n));
  };
  return Object.entries(t).forEach(([r, n]) => o(r, n)), e.length ? `?${e.join("&")}` : "";
}, W = (t, e) => {
  const s = encodeURI, o = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, i) => {
    var a;
    return (a = e.path) != null && a.hasOwnProperty(i) ? s(String(e.path[i])) : n;
  }), r = t.BASE + o;
  return e.query ? r + k(e.query) : r;
}, G = (t) => {
  if (t.formData) {
    const e = new FormData(), s = (o, r) => {
      f(r) || R(r) ? e.append(o, r) : e.append(o, JSON.stringify(r));
    };
    return Object.entries(t.formData).filter(([, o]) => o != null).forEach(([o, r]) => {
      Array.isArray(r) ? r.forEach((n) => s(o, n)) : s(o, r);
    }), e;
  }
}, m = async (t, e) => e, V = async (t, e) => {
  const [s, o, r, n] = await Promise.all([
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
  if (g(s) && (i.Authorization = `Bearer ${s}`), g(o) && g(r)) {
    const a = L(`${o}:${r}`);
    i.Authorization = `Basic ${a}`;
  }
  return e.body !== void 0 && (e.mediaType ? i["Content-Type"] = e.mediaType : R(e.body) ? i["Content-Type"] = e.body.type || "application/octet-stream" : f(e.body) ? i["Content-Type"] = "text/plain" : A(e.body) || (i["Content-Type"] = "application/json")), new Headers(i);
}, z = (t) => {
  var e, s;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (s = t.mediaType) != null && s.includes("+json") ? JSON.stringify(t.body) : f(t.body) || R(t.body) || A(t.body) ? t.body : JSON.stringify(t.body);
}, J = async (t, e, s, o, r, n, i) => {
  const a = new AbortController();
  let l = {
    headers: n,
    body: o ?? r,
    method: e.method,
    signal: a.signal
  };
  for (const c of t.interceptors.request._fns)
    l = await c(l);
  return i(() => a.abort()), await fetch(s, l);
}, K = (t, e) => {
  if (e) {
    const s = t.headers.get(e);
    if (f(s))
      return s;
  }
}, Q = async (t) => {
  if (t.status !== 204)
    try {
      const e = t.headers.get("Content-Type");
      if (e) {
        const s = ["application/octet-stream", "application/pdf", "application/zip", "audio/", "image/", "video/"];
        if (e.includes("application/json") || e.includes("+json"))
          return await t.json();
        if (s.some((o) => e.includes(o)))
          return await t.blob();
        if (e.includes("multipart/form-data"))
          return await t.formData();
        if (e.includes("text/"))
          return await t.text();
      }
    } catch (e) {
      console.error(e);
    }
}, X = (t, e) => {
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
    throw new j(t, e, o);
  if (!e.ok) {
    const r = e.status ?? "unknown", n = e.statusText ?? "unknown", i = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new j(
      t,
      e,
      `Generic Error: status: ${r}; status text: ${n}; body: ${i}`
    );
  }
}, Y = (t, e) => new $(async (s, o, r) => {
  try {
    const n = W(t, e), i = G(e), a = z(e), l = await V(t, e);
    if (!r.isCancelled) {
      let c = await J(t, e, n, a, i, l, r);
      for (const x of t.interceptors.response._fns)
        c = await x(c);
      const _ = await Q(c), D = K(c, e.responseHeader);
      let E = _;
      e.responseTransformer && c.ok && (E = await e.responseTransformer(_));
      const S = {
        url: n,
        ok: c.ok,
        status: c.status,
        statusText: c.statusText,
        body: D ?? E
      };
      X(e, S), s(S.body);
    }
  } catch (n) {
    o(n);
  }
});
class Z {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return Y(F, {
      method: "GET",
      url: "/umbraco/seoToolkit/modules"
    });
  }
}
var u;
class ee {
  constructor(e) {
    p(this, u);
    b(this, u, e);
  }
  async getModules() {
    return await I(y(this, u), Z.getUmbracoSeoToolkitModules());
  }
}
u = new WeakMap();
var h;
class te extends U {
  constructor(s) {
    super(s);
    p(this, h);
    b(this, h, new ee(this));
  }
  async getModules() {
    return y(this, h).getModules();
  }
}
h = new WeakMap();
var se = Object.defineProperty, re = Object.getOwnPropertyDescriptor, O = (t, e, s, o) => {
  for (var r = o > 1 ? void 0 : o ? re(e, s) : e, n = t.length - 1, i; n >= 0; n--)
    (i = t[n]) && (r = (o ? i(e, s, r) : i(r)) || r);
  return o && r && se(e, s, r), r;
};
let d = class extends H(N) {
  constructor() {
    super(), new te(this).getModules().then((t) => {
      console.log(t);
    });
  }
  async getModules() {
    return [];
  }
  render() {
    return v`
      <div>
        <umb-load-indicator ng-if="vm.loading"></umb-load-indicator>

        <h1>Welcome!</h1>
        <div class="intro">
            <p>This is the dashboard for SeoToolkit. The SEO package for Umbraco.</p>
            <p>Here you can see what modules are installed. Each functionality is shipped in its own package. So you can mix and match to your liking!</p>
        </div>
        <div class="modules">
          ${Object.entries(this.getModules()).forEach(() => {
      v`
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
  P`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
O([
  q()
], d.prototype, "modules", 2);
d = O([
  M("welcome-dashboard")
], d);
const le = d;
export {
  d as MyWelcomeDashboardElement,
  le as default
};
//# sourceMappingURL=welcomeDashboard.element-ByyiMuK4.js.map
