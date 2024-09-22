import { LitElement as n, html as p, css as i, customElement as c } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as d } from "@umbraco-cms/backoffice/element-api";
var h = Object.defineProperty, f = Object.getOwnPropertyDescriptor, b = (l, o, s, t) => {
  for (var e = t > 1 ? void 0 : t ? f(o, s) : o, m = l.length - 1, a; m >= 0; m--)
    (a = l[m]) && (e = (t ? a(o, s, e) : a(e)) || e);
  return t && e && h(o, s, e), e;
};
let r = class extends d(n) {
  render() {
    return p`
      <h1>Welcome Dashboard</h1>
      <div>
        <p>
          This is the Backoffice. From here, you can modify the content,
          media, and settings of your website.
        </p>
        <p>© Sample Company 20XX</p>
      </div>
    `;
  }
};
r.styles = [
  i`
      :host {
        display: block;
        padding: 24px;
      }
    `
];
r = b([
  c("my-welcome-dashboard")
], r);
const u = r;
export {
  r as MyWelcomeDashboardElement,
  u as default
};
//# sourceMappingURL=welcomeDashboard.element-CF4ZCtLK.js.map
