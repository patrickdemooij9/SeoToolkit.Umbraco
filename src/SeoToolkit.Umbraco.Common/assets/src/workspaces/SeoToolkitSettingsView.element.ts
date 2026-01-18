import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import SeoToolkitSettingsContext, { ST_SETTINGS_MODULE_TOKEN_CONTEXT } from "./SeoToolkitSettingsContext";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import { SeoKeyValueSettingViewModel } from "../api";

@customElement("st-settings")
export default class SeoToolkitSettingsViewElement extends UmbElementMixin(
  LitElement
) {
  #context?: SeoToolkitSettingsContext;

  @state()
  _fields: SeoKeyValueSettingViewModel[] = [];

  @state()
  _content: UmbPropertyValueData[] = [];

  constructor() {
    super();

    this.consumeContext(ST_SETTINGS_MODULE_TOKEN_CONTEXT, (context) => {
      this.#context = context;

      context?.settings.subscribe((value) => {
        this._fields = value;
        this._content = value.map((item) => ({
          alias: item.key,
          value: item.value,
        }));
      });
    });
  }

  #onPropertyDataChange(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;
    
    const values: {[key: string]: string} = {};
    value.forEach((item) => {
        values[item.alias] = item.value as string ?? "";
    })
    this.#context?.updateValues(values);
  }

  protected render() {
    return html`
      <umb-workspace-editor>
        <div class="settingsWorkspace">
          <uui-box headline="Settings" headline-variant="h5">
            <umb-property-dataset
              .value=${this._content}
              @change=${this.#onPropertyDataChange}
            >
              ${repeat(
                this._fields,
                (item) => item.key,
                (item) => html`
                  <umb-property
                    alias=${item.key!}
                    label=${item.title!}
                    description=${item.description!}
                    property-editor-ui-alias=${item.propertyAlias!}
                  >
                  </umb-property>
                `
              )}
            </umb-property-dataset>
          </uui-box>
        </div>
      </umb-workspace-editor>
    `;
  }

  static styles = css`
    .settingsWorkspace {
      padding: var(--uui-size-layout-1);
    }
  `;
}
