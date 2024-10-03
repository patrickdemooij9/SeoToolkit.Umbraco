import { UmbElementMixin as m } from "@umbraco-cms/backoffice/element-api";
import { LitElement as f, html as p, when as x, repeat as T, css as y, state as v, customElement as E } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as w } from "./RobotsTxtModuleContext-BxRUJsBp.js";
var g = Object.defineProperty, k = Object.getOwnPropertyDescriptor, _ = (e) => {
  throw TypeError(e);
}, u = (e, t, o, a) => {
  for (var r = a > 1 ? void 0 : a ? k(t, o) : t, i = e.length - 1, l; i >= 0; i--)
    (l = e[i]) && (r = (a ? l(t, o, r) : l(r)) || r);
  return a && r && g(t, o, r), r;
}, d = (e, t, o) => t.has(e) || _("Cannot " + o), O = (e, t, o) => (d(e, t, "read from private field"), t.get(e)), h = (e, t, o) => t.has(e) ? _("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, o), $ = (e, t, o, a) => (d(e, t, "write to private field"), t.set(e, o), o), C = (e, t, o) => (d(e, t, "access private method"), o), n, c, b;
let s = class extends m(f) {
  constructor() {
    super(), h(this, c), h(this, n), this._validationErrors = [], this.propertyAppearance = {
      labelOnTop: !0
    }, this.consumeContext(w, (e) => {
      $(this, n, e), this.observe(e.content, (t) => {
        this._content = {
          alias: "robotsTxt",
          value: t
        };
      });
    });
  }
  render() {
    var e;
    return p`
            <umb-workspace-editor>
                <div class="robotsTxtWorkspace">
                    ${(e = this._content) == null ? void 0 : e.value}

                <uui-box headline="Robots.txt" headline-variant="h5">
                    <umb-property-dataset
                        .value=${[this._content]}
                        @change=${C(this, c, b)}>
                        <umb-property 
                            alias='robotsTxt'
                            label='Robots.txt'
                            description='Robots.txt is used to let bots know what they are able to access. If you are looking for more information about how to configure robots.txt, then <a class="btn-link -underline" href="https://developers.google.com/search/docs/advanced/robots/intro">this guide</a> will be able to help you out.'
                            property-editor-ui-alias='Umb.PropertyEditorUi.TextArea'
                            val
                            .appearance=${this.propertyAppearance}
                            .config=${[{
      alias: "rows",
      value: 10
    }]}>
                        </umb-property>
                    </umb-property-dataset>

                    ${x(this._validationErrors.length > 0, () => p`
                        <h5>Validation Errors</h5>
                            ${T(this._validationErrors, (t) => t.error, (t) => p`
                            <p class="error">
                                <umb-icon icon="icon-delete" class="red"></umb-icon>
                                Line ${t.lineNumber} - ${t.error}
                            </p>
                            `)}
                        </div>
                            `)}
                </uui-box>
                </div>
            </umb-workspace-editor>
        `;
  }
};
n = /* @__PURE__ */ new WeakMap();
c = /* @__PURE__ */ new WeakSet();
b = function(e) {
  var a, r;
  const o = (a = e.target.value.find((i) => i.alias === "robotsTxt")) == null ? void 0 : a.value;
  o && ((r = O(this, n)) == null || r.setContent(o));
};
s.styles = [
  y`
            .robotsTxtWorkspace{
                padding: var(--uui-size-layout-1);
            }
        `
];
u([
  v()
], s.prototype, "_content", 2);
u([
  v()
], s.prototype, "_validationErrors", 2);
s = u([
  E("seotoolkit-module-robotstxt")
], s);
const P = s;
export {
  s as SeoToolkitRobotsTxtModuleElement,
  P as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-Dy9S2KzY.js.map
