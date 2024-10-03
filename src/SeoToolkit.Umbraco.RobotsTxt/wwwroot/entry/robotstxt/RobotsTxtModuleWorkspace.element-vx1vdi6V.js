import { UmbElementMixin as c } from "@umbraco-cms/backoffice/element-api";
import { LitElement as p, html as d, state as b, customElement as u } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as m } from "./RobotsTxtModuleContext-MGrZW9Ey.js";
var h = Object.defineProperty, v = Object.getOwnPropertyDescriptor, n = (t, e, s, r) => {
  for (var o = r > 1 ? void 0 : r ? v(e, s) : e, a = t.length - 1, i; a >= 0; a--)
    (i = t[a]) && (o = (r ? i(e, s, o) : i(o)) || o);
  return r && o && h(e, s, o), o;
};
let l = class extends c(p) {
  constructor() {
    super(), this.propertyAppearance = {
      labelOnTop: !0
    }, this.consumeContext(m, (t) => {
      console.log("Hello?"), this.observe(t.test, (e) => {
        this._test = e;
      });
    });
  }
  render() {
    return d`
            <umb-workspace-editor>
                <uui-box headline="Robots.txt" headline-variant="h5">
                    <div>
                        <umb-property 
                            alias='robotsTxt'
                            label='Robots.txt'
                            description='Robots.txt is used to let bots know what they are able to access. If you are looking for more information about how to configure robots.txt, then <a class="btn-link -underline" href="https://developers.google.com/search/docs/advanced/robots/intro">this guide</a> will be able to help you out.'
                            property-editor-ui-alias='Umb.PropertyEditorUi.TextArea'
                            .appearance=${this.propertyAppearance}
                            .config=${[{
      alias: "placeholder",
      value: "test"
    }, {
      alias: "rows",
      value: 10
    }]}>

                        </umb-property>
                    </div>
                <div class="umb-control-group control-group --label-on-top">
                                <div class="umb-el-wrap">
                                    <div class="control-header">
                                        <label class="control-label">Robots.txt</label>
                                        <small class="control-description full-width-description">Robots.txt is used to let bots know what they are able to access. If you are looking for more information about how to configure robots.txt, then <a class="btn-link -underline" href="https://developers.google.com/search/docs/advanced/robots/intro">this guide</a> will be able to help you out.</small>
                                    </div>
                                    <div class="controls">
                                        <textarea class="umb-property-editor" ng-model="vm.model" rows="10"></textarea>
                                    </div>
                                </div>
                            </div>
                            <div ng-if="vm.validationErrors.length > 0">
                                <h5>Validation Errors</h5>
                                <p class="error" ng-repeat="error in vm.validationErrors">
                                    <umb-icon icon="icon-delete" class="red"></umb-icon>
                                    Line {{error.lineNumber}} - {{error.error}}
                                </p>
                            </div>
                </uui-box>
            </umb-workspace-editor>
        `;
  }
};
n([
  b()
], l.prototype, "_test", 2);
l = n([
  u("seotoolkit-module-robotstxt")
], l);
const w = l;
export {
  l as SeoToolkitRobotsTxtModuleElement,
  w as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-vx1vdi6V.js.map
