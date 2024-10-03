import { UmbElementMixin as p } from "@umbraco-cms/backoffice/element-api";
import { LitElement as c, html as d, state as b, customElement as u } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as m } from "./RobotsTxtModuleContext-CXpLTOa_.js";
var v = Object.defineProperty, h = Object.getOwnPropertyDescriptor, n = (t, o, s, r) => {
  for (var e = r > 1 ? void 0 : r ? h(o, s) : o, i = t.length - 1, a; i >= 0; i--)
    (a = t[i]) && (e = (r ? a(o, s, e) : a(e)) || e);
  return r && e && v(o, s, e), e;
};
let l = class extends p(c) {
  constructor() {
    super(), this.propertyAppearance = {
      labelOnTop: !0
    }, this.consumeContext(m, (t) => {
      console.log("Hello?"), this.observe(t.test, (o) => {
        this._test = o;
      });
    });
  }
  render() {
    return d`
            <umb-workspace-editor>
                <uui-box headline="Robots.txt" headline-variant="h5">
                    <div>
                        <umb-property 
                            alias='test'
                            label='testLabel'
                            description='testDescription'
                            property-editor-ui-alias='Umb.PropertyEditorUi.TextArea'
                            .appearance=${this.propertyAppearance}>

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
const g = l;
export {
  l as SeoToolkitRobotsTxtModuleElement,
  g as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-DwQpwfmR.js.map
