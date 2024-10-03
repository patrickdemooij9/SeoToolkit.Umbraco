import { UmbElementMixin as c } from "@umbraco-cms/backoffice/element-api";
import { LitElement as d, html as b, state as m, customElement as p } from "@umbraco-cms/backoffice/external/lit";
import { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT as u } from "./RobotsTxtModuleContext-BKtW9xv0.js";
var v = Object.defineProperty, h = Object.getOwnPropertyDescriptor, n = (t, e, s, r) => {
  for (var o = r > 1 ? void 0 : r ? h(e, s) : e, i = t.length - 1, a; i >= 0; i--)
    (a = t[i]) && (o = (r ? a(e, s, o) : a(o)) || o);
  return r && o && v(e, s, o), o;
};
let l = class extends c(d) {
  constructor() {
    super(), this.consumeContext(u, (t) => {
      console.log("Hello?"), this.observe(t.test, (e) => {
        this._test = e;
      });
    });
  }
  render() {
    return b`
            <umb-workspace-editor>
                <uui-box headline="Robots.txt" headline-variant="h5">
                    <div>
                        <umb-property 
                            alias='test'
                            label='testLabel'
                            description='testDescription'
                            property-editor-ui-alias='Umb.PropertyEditorUi.TextArea'>

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
  m()
], l.prototype, "_test", 2);
l = n([
  p("seotoolkit-module-robotstxt")
], l);
const E = l;
export {
  l as SeoToolkitRobotsTxtModuleElement,
  E as default
};
//# sourceMappingURL=RobotsTxtModuleWorkspace.element-BgUM6hQT.js.map
