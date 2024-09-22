import { LitElement as r, html as d, css as c, state as u, customElement as p } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as h } from "@umbraco-cms/backoffice/element-api";
var f = Object.defineProperty, b = Object.getOwnPropertyDescriptor, m = (s, o, a, l) => {
  for (var e = l > 1 ? void 0 : l ? b(o, a) : o, i = s.length - 1, n; i >= 0; i--)
    (n = s[i]) && (e = (l ? n(o, a, e) : n(e)) || e);
  return l && e && f(o, a, e), e;
};
let t = class extends h(r) {
  constructor() {
    super(), this.consumeContext("moduleRepository", (s) => {
      console.log(s);
    });
  }
  async getModules() {
    return [];
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
t.styles = [
  c`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
m([
  u()
], t.prototype, "modules", 2);
t = m([
  p("welcome-dashboard")
], t);
const y = t;
export {
  t as MyWelcomeDashboardElement,
  y as default
};
//# sourceMappingURL=welcomeDashboard.element-BiGDaoQp.js.map
