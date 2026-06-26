import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import { SeoFieldViewModel } from "../api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import { createExtensionElement } from "@umbraco-cms/backoffice/extension-api";
import {
  ManifestPropertyEditorUi,
  UmbPropertyEditorConfigCollection
} from "@umbraco-cms/backoffice/property-editor";
import { UmbChangeEvent } from "@umbraco-cms/backoffice/event";

@customElement("st-metafield-settingsfield")
export class MetaFieldsSettingsField extends UmbElementMixin(LitElement) {
  @property({ type: Object })
  public set field(value: SeoFieldViewModel | undefined) {
    this._field = value;
    if (this._element) {
      this._element.value = value?.value;
    }
  }
  public get field() {
    return this._field;
  }
  private _field?: SeoFieldViewModel;

  @property({ type: String })
  public set view(value: string | undefined) {
    this._view = value;
    this.observePropertyView();
  }
  public get view() {
    return this._view;
  }
  private _view?: string;

  @property({ type: Boolean })
  public hasGlobalInheritance: boolean = false;

  @state()
  private _element?: ManifestPropertyEditorUi["ELEMENT_TYPE"];

  private observePropertyView() {
    if (!this._view) {
      return;
    }

    this.observe(
      umbExtensionsRegistry.byTypeAndAlias("propertyEditorUi", this._view),
      (manifest) => {
        this._gotEditorUI(manifest);
      },
      "_observePropertyEditorUI"
    );
  }

  private async _gotEditorUI(
    manifest?: ManifestPropertyEditorUi | null
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
      if (this.field?.editor?.config) {
        this._element.config = new UmbPropertyEditorConfigCollection(
          Object.entries(this.field?.editor?.config).map((item) => ({
            alias: item[0],
            value: item[1],
          }))
        );
      }
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
        <div slot="editor">
          ${when(
            this.field!.useInheritedValue,
            () => html` <uui-tag>
                <span
                  >This field uses the value of the inherited component.</span
                >
              </uui-tag>
              <uui-button look="placeholder" @click=${this.toggleInheritance}
                >Override</uui-button
              >`,
            () => html`
              ${this._element}
              ${when(
                this.hasGlobalInheritance,
                () => html`
                  <uui-button
                    look="placeholder"
                    @click=${this.toggleInheritance}
                    >Set to inherited</uui-button
                  >
                `
              )}
            `
          )}
        </div>
      </umb-property-layout>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    "st-metafield-settingsfield": MetaFieldsSettingsField;
  }
}
