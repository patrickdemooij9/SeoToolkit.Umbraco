var C = (t) => {
  throw TypeError(t);
};
var j = (t, e, r) => e.has(t) || C("Cannot " + r);
var b = (t, e, r) => (j(t, e, "read from private field"), r ? r.call(t) : e.get(t)), g = (t, e, r) => e.has(t) ? C("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), R = (t, e, r, o) => (j(t, e, "write to private field"), o ? o.call(t, r) : e.set(t, r), r);
import { LitElement as q, html as d, when as y, repeat as N, css as P, state as $, customElement as I } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as U } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as H } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as M } from "@umbraco-cms/backoffice/resources";
import { O as B } from "./index-CuDNWOxm.js";
class v extends Error {
  constructor(e, r, o) {
    super(o), this.name = "ApiError", this.url = r.url, this.status = r.status, this.statusText = r.statusText, this.body = r.body, this.request = e;
  }
}
class k extends Error {
  constructor(e) {
    super(e), this.name = "CancelError";
  }
  get isCancelled() {
    return !0;
  }
}
class F {
  constructor(e) {
    this._isResolved = !1, this._isRejected = !1, this._isCancelled = !1, this.cancelHandlers = [], this.promise = new Promise((r, o) => {
      this._resolve = r, this._reject = o;
      const s = (i) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isResolved = !0, this._resolve && this._resolve(i));
      }, n = (i) => {
        this._isResolved || this._isRejected || this._isCancelled || (this._isRejected = !0, this._reject && this._reject(i));
      }, a = (i) => {
        this._isResolved || this._isRejected || this._isCancelled || this.cancelHandlers.push(i);
      };
      return Object.defineProperty(a, "isResolved", {
        get: () => this._isResolved
      }), Object.defineProperty(a, "isRejected", {
        get: () => this._isRejected
      }), Object.defineProperty(a, "isCancelled", {
        get: () => this._isCancelled
      }), e(s, n, a);
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
      this.cancelHandlers.length = 0, this._reject && this._reject(new k("Request aborted"));
    }
  }
  get isCancelled() {
    return this._isCancelled;
  }
}
const m = (t) => typeof t == "string", T = (t) => m(t) && t !== "", w = (t) => t instanceof Blob, A = (t) => t instanceof FormData, L = (t) => {
  try {
    return btoa(t);
  } catch {
    return Buffer.from(t).toString("base64");
  }
}, W = (t) => {
  const e = [], r = (s, n) => {
    e.push(`${encodeURIComponent(s)}=${encodeURIComponent(String(n))}`);
  }, o = (s, n) => {
    n != null && (n instanceof Date ? r(s, n.toISOString()) : Array.isArray(n) ? n.forEach((a) => o(s, a)) : typeof n == "object" ? Object.entries(n).forEach(([a, i]) => o(`${s}[${a}]`, i)) : r(s, n));
  };
  return Object.entries(t).forEach(([s, n]) => o(s, n)), e.length ? `?${e.join("&")}` : "";
}, G = (t, e) => {
  const r = encodeURI, o = e.url.replace("{api-version}", t.VERSION).replace(/{(.*?)}/g, (n, a) => {
    var i;
    return (i = e.path) != null && i.hasOwnProperty(a) ? r(String(e.path[a])) : n;
  }), s = t.BASE + o;
  return e.query ? s + W(e.query) : s;
}, z = (t) => {
  if (t.formData) {
    const e = new FormData(), r = (o, s) => {
      m(s) || w(s) ? e.append(o, s) : e.append(o, JSON.stringify(s));
    };
    return Object.entries(t.formData).filter(([, o]) => o != null).forEach(([o, s]) => {
      Array.isArray(s) ? s.forEach((n) => r(o, n)) : r(o, s);
    }), e;
  }
}, p = async (t, e) => typeof e == "function" ? e(t) : e, J = async (t, e) => {
  const [r, o, s, n] = await Promise.all([
    // @ts-ignore
    p(e, t.TOKEN),
    // @ts-ignore
    p(e, t.USERNAME),
    // @ts-ignore
    p(e, t.PASSWORD),
    // @ts-ignore
    p(e, t.HEADERS)
  ]), a = Object.entries({
    Accept: "application/json",
    ...n,
    ...e.headers
  }).filter(([, i]) => i != null).reduce((i, [l, c]) => ({
    ...i,
    [l]: String(c)
  }), {});
  if (T(r) && (a.Authorization = `Bearer ${r}`), T(o) && T(s)) {
    const i = L(`${o}:${s}`);
    a.Authorization = `Basic ${i}`;
  }
  return e.body !== void 0 && (e.mediaType ? a["Content-Type"] = e.mediaType : w(e.body) ? a["Content-Type"] = e.body.type || "application/octet-stream" : m(e.body) ? a["Content-Type"] = "text/plain" : A(e.body) || (a["Content-Type"] = "application/json")), new Headers(a);
}, V = (t) => {
  var e, r;
  if (t.body !== void 0)
    return (e = t.mediaType) != null && e.includes("application/json") || (r = t.mediaType) != null && r.includes("+json") ? JSON.stringify(t.body) : m(t.body) || w(t.body) || A(t.body) ? t.body : JSON.stringify(t.body);
}, K = async (t, e, r, o, s, n, a) => {
  const i = new AbortController();
  let l = {
    headers: n,
    body: o ?? s,
    method: e.method,
    signal: i.signal
  };
  t.WITH_CREDENTIALS && (l.credentials = t.CREDENTIALS);
  for (const c of t.interceptors.request._fns)
    l = await c(l);
  return a(() => i.abort()), await fetch(r, l);
}, Q = (t, e) => {
  if (e) {
    const r = t.headers.get(e);
    if (m(r))
      return r;
  }
}, X = async (t) => {
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
}, Y = (t, e) => {
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
    throw new v(t, e, o);
  if (!e.ok) {
    const s = e.status ?? "unknown", n = e.statusText ?? "unknown", a = (() => {
      try {
        return JSON.stringify(e.body, null, 2);
      } catch {
        return;
      }
    })();
    throw new v(
      t,
      e,
      `Generic Error: status: ${s}; status text: ${n}; body: ${a}`
    );
  }
}, Z = (t, e) => new F(async (r, o, s) => {
  try {
    const n = G(t, e), a = z(e), i = V(e), l = await J(t, e);
    if (!s.isCancelled) {
      let c = await K(t, e, n, i, a, l, s);
      for (const x of t.interceptors.response._fns)
        c = await x(c);
      const _ = await X(c), O = Q(c, e.responseHeader);
      let S = _;
      e.responseTransformer && c.ok && (S = await e.responseTransformer(_));
      const E = {
        url: n,
        ok: c.ok,
        status: c.status,
        statusText: c.statusText,
        body: O ?? S
      };
      Y(e, E), r(E.body);
    }
  } catch (n) {
    o(n);
  }
});
class ee {
  /**
   * @returns unknown OK
   * @throws ApiError
   */
  static getUmbracoSeoToolkitModules() {
    return Z(B, {
      method: "GET",
      url: "/umbraco/seoToolkit/modules"
    });
  }
}
var h;
class te {
  constructor(e) {
    g(this, h);
    R(this, h, e);
  }
  async getModules() {
    return await M(b(this, h), ee.getUmbracoSeoToolkitModules());
  }
}
h = new WeakMap();
var f;
class re extends H {
  constructor(r) {
    super(r);
    g(this, f);
    R(this, f, new te(this));
  }
  async getModules() {
    return b(this, f).getModules();
  }
}
f = new WeakMap();
var se = Object.defineProperty, oe = Object.getOwnPropertyDescriptor, D = (t, e, r, o) => {
  for (var s = o > 1 ? void 0 : o ? oe(e, r) : e, n = t.length - 1, a; n >= 0; n--)
    (a = t[n]) && (s = (o ? a(e, r, s) : a(s)) || s);
  return o && s && se(e, r, s), s;
};
let u = class extends U(q) {
  connectedCallback() {
    super.connectedCallback(), new re(this).getModules().then((t) => {
      this.modules = t.data;
    });
  }
  render() {
    return d`
      <div>
        <umb-load-indicator ng-if="vm.loading"></umb-load-indicator>

        <h1>Welcome!</h1>
        <div class="intro">
            <p>This is the dashboard for SeoToolkit. The SEO package for Umbraco.</p>
            <p>Here you can see what modules are installed. Each functionality is shipped in its own package. So you can mix and match to your liking!</p>
        </div>
        <div class="modules">
          ${y(this.modules, () => {
      N(this.modules, (t) => t.alias, (t) => {
        d`
                <a href="#" href="${t.link}" class="module" target="_blank">
                    <div class="module-icon">
                        <umb-icon name="${t.icon}"></umb-icon>
                    </div>
                    <p class="module-title">${t.title}</p>
            ${y(t.status === "Disabled", () => d`<p class="module-status module-status-disabled">Disabled</p>`)}
            ${y(t.status === "Installed", () => d`<p class="module-status module-status-installed">Installed</p>`)}
            ${y(t.status === "NotInstalled", () => d`<p class="module-status">Not installed</p>`)}
                </a>`;
      });
    })}
            
        </div>
      </div>
    `;
  }
};
u.styles = [
  P`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
D([
  $()
], u.prototype, "modules", 2);
u = D([
  I("welcome-dashboard")
], u);
const ue = u;
export {
  u as MyWelcomeDashboardElement,
  ue as default
};
//# sourceMappingURL=welcomeDashboard.element-C2cGqukb.js.map
