import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import SeoToolkitSettingsContext, {
  ST_SETTINGS_MODULE_TOKEN_CONTEXT,
} from "./SeoToolkitSettingsContext";
import { SeoKeyValueSettingViewModel } from "../api";

import "../components/SeoSettingField.element";
import { SeoSettingFieldElement } from "../components/SeoSettingField.element";

@customElement("st-settings")
export default class SeoToolkitSettingsViewElement extends UmbElementMixin(
  LitElement,
) {
  #context?: SeoToolkitSettingsContext;

  @state()
  _fields: SeoKeyValueSettingViewModel[] = [];

  @state()
  _inheritedKeys: string[] = [];

  constructor() {
    super();

    this.consumeContext(ST_SETTINGS_MODULE_TOKEN_CONTEXT, (context) => {
      this.#context = context;

      context?.settings.subscribe((value) => {
        this._fields = value;
      });
      context?.inheritedKeys.subscribe((value) => this._inheritedKeys = value);
    });
  }

  #onPropertyDataChange(e: Event) {
    const field = (e.target as SeoSettingFieldElement).field!;
    this.#context?.updateValue(field.key, (field.value as string) ?? "");
    /*this.#context?.updateField(this._culture!, field!.alias!, field!.userValue);

    const value = (e.target as UmbPropertyDatasetElement).value;

    const values: { [key: string]: string } = {};
    value.forEach((item) => {
      values[item.alias] = (item.value as string) ?? "";
    });
    this.#context?.updateValues(values);*/
  }

  protected render() {
    return html`
      <umb-workspace-editor>
        <div class="settingsWorkspace">
          <uui-box headline="Settings" headline-variant="h5">
            ${repeat(
              this._fields,
              (item) => item.key,
              (item) => html`
                <st-setting-field
                  .field=${item}
                  @change=${this.#onPropertyDataChange}
                  @toggle-inheritance=${() => this.#context!.toggleInheritance(item.key)}
                  .inherited=${this._inheritedKeys.includes(item.key)}
                >
                </st-setting-field>
              `,
            )}
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
