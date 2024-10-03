import { UmbElementMixin as c } from "@umbraco-cms/backoffice/element-api";
import { LitElement as b, html as n, when as d, repeat as m, css as h, state as u, customElement as v } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as x } from "./RobotsTxtModuleContext-DkxNSS3b.js";
var T = Object.defineProperty, _ = Object.getOwnPropertyDescriptor, p = (o, t, i, s) => {
  for (var e = s > 1 ? void 0 : s ? _(t, i) : t, a = o.length - 1, l; a >= 0; a--)
    (l = o[a]) && (e = (s ? l(t, i, e) : l(e)) || e);
  return s && e && T(t, i, e), e;
};
let r = class extends c(b) {
  constructor() {
    super(), this._validationErrors = [], this.propertyAppearance = {
      labelOnTop: !0
    }, this.consumeContext(x, (o) => {
      console.log("Hello?"), this.observe(o.test, (t) => {
        this._test = t;
      });
    });
  }
  render() {
    return n`
            <umb-workspace-editor>
                <div class="robotsTxtWorkspace">
                <uui-box headline="Robots.txt" headline-variant="h5">
                    <div>
                        <umb-property 
                            alias='robotsTxt'
                            label='Robots.txt'
                            description='Robots.txt is used to let bots know what they are able to access. If you are looking for more information about how to configure robots.txt, then <a class="btn-link -underline" href="https://developers.google.com/search/docs/advanced/robots/intro">this guide</a> will be able to help you out.'
                            property-editor-ui-alias='Umb.PropertyEditorUi.TextArea'
                            .appearance=${this.propertyAppearance}
                            .config=${[{
      alias: "rows",
      value: 10
    }]}>
                        </umb-property>
                    </div>

                    ${d(this._validationErrors, () => n`
                        <h5>Validation Errors</h5>
                            ${m(this._validationErrors, (o) => o.error, (o) => n`
                            <p class="error">
                                <umb-icon icon="icon-delete" class="red"></umb-icon>
                                Line ${o.lineNumber} - ${o.error}
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
r.styles = [
  h`
            .robotsTxtWorkspace{
                padding: var(--uui-size-layout-1);
            }
        `
];
p([
  u()
], r.prototype, "_test", 2);
p([
  u()
], r.prototype, "_validationErrors", 2);
r = p([
  v("seotoolkit-module-robotstxt")
], r);
const w = r;
export {
  r as SeoToolkitRobotsTxtModuleElement,
  w as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-CV0e_5WK.js.map
