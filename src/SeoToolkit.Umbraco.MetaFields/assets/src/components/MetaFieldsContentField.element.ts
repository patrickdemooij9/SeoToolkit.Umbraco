import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import { SeoSettingsFieldViewModel } from "../api";
import {
  umbExtensionsRegistry,
} from "@umbraco-cms/backoffice/extension-registry";
import { createExtensionElement } from "@umbraco-cms/backoffice/extension-api";
import {
  ManifestPropertyEditorUi,
  UmbPropertyEditorConfigCollection,
  UmbPropertyValueChangeEvent,
} from "@umbraco-cms/backoffice/property-editor";

@customElement("st-metafield-contentfield")
export class MetaFieldsContentField extends UmbElementMixin(LitElement) {
  @property({ type: Object })
  public set field(value: SeoSettingsFieldViewModel | undefined) {
    this._field = value;
    if (this._element) {
      this._element.value = value?.userValue;
    }
  }
  public get field() {
    return this._field;
  }
  private _field?: SeoSettingsFieldViewModel;

  @property({ type: String })
  public set view(value: string | undefined) {
    this._view = value;
    this.observePropertyView();
  }
  public get view() {
    return this._view;
  }
  private _view?: string;

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
          userValue: this._element!.value,
        };
        console.log("Hello world");
        this.dispatchEvent(new UmbPropertyValueChangeEvent());
      });

      this._element.value = this.field?.userValue;
      if (this.field?.editConfig) {
        this._element.config = new UmbPropertyEditorConfigCollection(
          Object.entries(this.field?.editConfig).map((item) => ({
            alias: item[0],
            value: item[1],
          }))
        );
      }
    }
  }

  render() {
    return html`
      <umb-property-layout
        .label=${this.field!.title!}
        .description=${this.field!.description!}
        orientation="vertical"
      >
        <div slot="editor">
          ${this._element}
          ${(when(
            this.field?.value,
            () => html` <small>Fallback value: ${this.field?.value}</small> `,
            () => html` <small>No fallback value found</small>`
          ))}
        </div>
      </umb-property-layout>
    `;
  }
}

declare global {
  interface HTMLElementTagNameMap {
    "st-metafield-contentfield": MetaFieldsContentField;
  }
}
