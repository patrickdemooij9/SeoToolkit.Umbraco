import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import { SeoKeyValueSettingViewModel } from "../api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import { createExtensionElement } from "@umbraco-cms/backoffice/extension-api";
import { ManifestPropertyEditorUi } from "@umbraco-cms/backoffice/property-editor";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";

@customElement("st-setting-field")
export class SeoSettingFieldElement extends UmbElementMixin(LitElement) {
  @property({ type: Object })
  public set field(value: SeoKeyValueSettingViewModel | undefined) {
    this._field = value;
    if (this._element) {
      this._element.value = value?.value;
    }
    this.observePropertyView();
  }
  public get field() {
    return this._field;
  }
  private _field?: SeoKeyValueSettingViewModel;

  @property({ type: Boolean })
  public inherited!: boolean;

  @state()
  private _element?: ManifestPropertyEditorUi["ELEMENT_TYPE"];

  @state()
  private _observing = false;

  private observePropertyView() {
    if (!this._field || this._observing) {
      return;
    }
    this._observing = true;

    this.observe(
      umbExtensionsRegistry.byTypeAndAlias(
        "propertyEditorUi",
        this._field.propertyAlias,
      ),
      (manifest) => {
        this._gotEditorUI(manifest);
      },
      "_observePropertyEditorUI",
    );
  }

  private async _gotEditorUI(
    manifest?: ManifestPropertyEditorUi | null,
  ): Promise<void> {
    if (!manifest) {
      return;
    }

    const el = await createExtensionElement(manifest);
    if (el) {
      this._element = el;
      this._element.addEventListener("change", () => {
        this._field = {
          ...this._field!,
          value: this._element!.value,
        };
        this.dispatchEvent(new UmbChangeEvent());
      });

      this._element.value = this.field?.value;
      /*if (this.field?.editConfig) {
        this._element.config = new UmbPropertyEditorConfigCollection(
          Object.entries(this.field?.editConfig).map((item) => ({
            alias: item[0],
            value: item[1],
          }))
        );
      }*/
    }
  }

  private toggleInheritance() {
    const event = new Event("toggle-inheritance", { bubbles: true });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <umb-property-layout
        .label=${this.field!.title!}
        .description=${this.field!.description!}
      >
        <div slot="editor" class="relative">
          ${this._element}
          ${when(this.field?.hasRootValue, () => {
            return this.inherited
              ? html`<div class="overlay">
                  <uui-button look="outline" @click=${this.toggleInheritance}>
                    Override from root value
                  </uui-button>
                </div>`
              : html`<div>
                  <uui-button look="outline" @click=${this.toggleInheritance}>
                    Use root value
                  </uui-button>
                </div>`;
          })}
        </div>
      </umb-property-layout>
    `;
  }

  static styles = css`
    .relative {
      position: relative;
    }

    .overlay {
      position: absolute;

      top: 0;
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: rgba(0, 0, 0, 0.2);
      padding: 12px 0;

      > uui-button {
        background-color: white;
      }
    }
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    "st-setting-field": SeoSettingFieldElement;
  }
}
