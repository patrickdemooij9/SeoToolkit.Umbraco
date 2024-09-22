import { LitElement as b, html as d, css as v, customElement as f } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as x } from "@umbraco-cms/backoffice/element-api";
/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
let g = class extends Event {
  constructor(t, s, i) {
    super("context-request", { bubbles: !0, composed: !0 }), this.context = t, this.callback = s, this.subscribe = i ?? !1;
  }
};
/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
class y {
  get value() {
    return this.o;
  }
  set value(t) {
    this.setValue(t);
  }
  setValue(t, s = !1) {
    const i = s || !Object.is(t, this.o);
    this.o = t, i && this.updateObservers();
  }
  constructor(t) {
    this.subscriptions = /* @__PURE__ */ new Map(), this.updateObservers = () => {
      for (const [s, { disposer: i }] of this.subscriptions) s(this.o, i);
    }, t !== void 0 && (this.value = t);
  }
  addCallback(t, s, i) {
    if (!i) return void t(this.value);
    this.subscriptions.has(t) || this.subscriptions.set(t, { disposer: () => {
      this.subscriptions.delete(t);
    }, consumerHost: s });
    const { disposer: o } = this.subscriptions.get(t);
    t(this.value, o);
  }
  clearCallbacks() {
    this.subscriptions.clear();
  }
}
/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
let w = class extends Event {
  constructor(t) {
    super("context-provider", { bubbles: !0, composed: !0 }), this.context = t;
  }
};
class h extends y {
  constructor(t, s, i) {
    var o, n;
    super(s.context !== void 0 ? s.initialValue : i), this.onContextRequest = (e) => {
      const a = e.composedPath()[0];
      e.context === this.context && a !== this.host && (e.stopPropagation(), this.addCallback(e.callback, a, e.subscribe));
    }, this.onProviderRequest = (e) => {
      const a = e.composedPath()[0];
      if (e.context !== this.context || a === this.host) return;
      const u = /* @__PURE__ */ new Set();
      for (const [l, { consumerHost: m }] of this.subscriptions) u.has(l) || (u.add(l), m.dispatchEvent(new g(this.context, l, !0)));
      e.stopPropagation();
    }, this.host = t, s.context !== void 0 ? this.context = s.context : this.context = s, this.attachListeners(), (n = (o = this.host).addController) == null || n.call(o, this);
  }
  attachListeners() {
    this.host.addEventListener("context-request", this.onContextRequest), this.host.addEventListener("context-provider", this.onProviderRequest);
  }
  hostConnected() {
    this.host.dispatchEvent(new w(this.context));
  }
}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
function E({ context: c }) {
  return (t, s) => {
    const i = /* @__PURE__ */ new WeakMap();
    if (typeof s == "object") return s.addInitializer(function() {
      i.set(this, new h(this, { context: c }));
    }), { get() {
      return t.get.call(this);
    }, set(o) {
      var n;
      return (n = i.get(this)) == null || n.setValue(o), t.set.call(this, o);
    }, init(o) {
      var n;
      return (n = i.get(this)) == null || n.setValue(o), o;
    } };
    {
      t.constructor.addInitializer((e) => {
        i.set(e, new h(e, { context: c }));
      });
      const o = Object.getOwnPropertyDescriptor(t, s);
      let n;
      if (o === void 0) {
        const e = /* @__PURE__ */ new WeakMap();
        n = { get() {
          return e.get(this);
        }, set(a) {
          i.get(this).setValue(a), e.set(this, a);
        }, configurable: !0, enumerable: !0 };
      } else {
        const e = o.set;
        n = { ...o, set(a) {
          i.get(this).setValue(a), e == null || e.call(this, a);
        } };
      }
      return void Object.defineProperty(t, s, n);
    }
  };
}
const O = "moduleRepository";
var P = Object.defineProperty, k = Object.getOwnPropertyDescriptor, p = (c, t, s, i) => {
  for (var o = i > 1 ? void 0 : i ? k(t, s) : t, n = c.length - 1, e; n >= 0; n--)
    (e = c[n]) && (o = (i ? e(t, s, o) : e(o)) || o);
  return i && o && P(t, s, o), o;
};
let r = class extends x(b) {
  async getModules() {
    return (await this.moduleRepository.getModules()).data;
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
          ${Object.entries(this.getModules()).forEach(() => {
      d`
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
r.styles = [
  v`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
p([
  E({ context: O })
], r.prototype, "moduleRepository", 2);
r = p([
  f("welcome-dashboard")
], r);
const V = r;
export {
  r as MyWelcomeDashboardElement,
  V as default
};
//# sourceMappingURL=welcomeDashboard.element-BolvzcDo.js.map
