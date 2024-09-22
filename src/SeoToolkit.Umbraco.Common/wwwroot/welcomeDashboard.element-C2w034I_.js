var b = (e) => {
  throw TypeError(e);
};
var g = (e, o, t) => o.has(e) || b("Cannot " + t);
var m = (e, o, t) => (g(e, o, "read from private field"), t ? t.call(e) : o.get(e)), h = (e, o, t) => o.has(e) ? b("Cannot add the same private member more than once") : o instanceof WeakSet ? o.add(e) : o.set(e, t), p = (e, o, t, s) => (g(e, o, "write to private field"), s ? s.call(e, t) : o.set(e, t), t);
import { LitElement as f, html as a, when as n, repeat as x, css as D, state as v, customElement as y } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as M } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase as S } from "@umbraco-cms/backoffice/class-api";
import { tryExecuteAndNotify as $ } from "@umbraco-cms/backoffice/resources";
import { S as E } from "./index-jV5LezuH.js";
var r;
class k {
  constructor(o) {
    h(this, r);
    p(this, r, o);
  }
  async getModules() {
    return await $(m(this, r), E.getUmbracoSeoToolkitModules());
  }
}
r = new WeakMap();
var c;
class _ extends S {
  constructor(t) {
    super(t);
    h(this, c);
    p(this, c, new k(this));
  }
  async getModules() {
    return m(this, c).getModules();
  }
}
c = new WeakMap();
var O = Object.defineProperty, T = Object.getOwnPropertyDescriptor, w = (e, o, t, s) => {
  for (var l = s > 1 ? void 0 : s ? T(o, t) : o, i = e.length - 1, u; i >= 0; i--)
    (u = e[i]) && (l = (s ? u(o, t, l) : u(l)) || l);
  return s && l && O(o, t, l), l;
};
let d = class extends M(f) {
  connectedCallback() {
    super.connectedCallback(), new _(this).getModules().then((e) => {
      this.modules = e.data, console.log(this.modules);
    });
  }
  render() {
    return a`
      <div class="welcomeDashboard">
        <h1>Welcome!</h1>
        <div class="intro">
            <p>This is the dashboard for SeoToolkit. The SEO package for Umbraco.</p>
            <p>Here you can see what modules are installed. Each functionality is shipped in its own package. So you can mix and match to your liking!</p>
        </div>
        <div class="modules">
          ${n(
      this.modules,
      () => a`
      
        ${x(
        this.modules,
        (e) => e.alias,
        (e) => a`
                <a href="#" href="${e.link}" class="module" target="_blank">
                    <div class="module-icon">
                        <umb-icon name="${e.icon}"></umb-icon>
                    </div>
                    <p class="module-title">${e.title}</p>
            ${n(e.status === "Disabled", () => a`<p class="module-status module-status-disabled">Disabled</p>`)}
            ${n(e.status === "Installed", () => a`<p class="module-status module-status-installed">Installed</p>`)}
            ${n(e.status === "NotInstalled", () => a`<p class="module-status">Not installed</p>`)}
                </a>`
      )}
      `
    )}
            
        </div>
      </div>
    `;
  }
};
d.styles = [
  D`
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
w([
  v()
], d.prototype, "modules", 2);
d = w([
  y("welcome-dashboard")
], d);
const j = d;
export {
  d as MyWelcomeDashboardElement,
  j as default
};
//# sourceMappingURL=welcomeDashboard.element-C2w034I_.js.map
