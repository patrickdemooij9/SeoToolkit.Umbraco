import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  classMap,
  customElement,
  property,
  repeat,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
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

  private getValue(): string | undefined | null {
    return this.field?.userValue?.toString() ?? this.field?.value;
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
          ${when(
            this.field?.allowFallback ?? true,
            () =>
              when(
                this.field?.value,
                () => html` <small>Fallback value: ${this.field?.value}</small> `,
                () => html` <small>No fallback value found</small>`
              )
          )}
          ${when(this.field!.suggestions!.length > 0, () => html`
            ${repeat(this.field!.suggestions!, (item) => item.alias, (item) => html`
              <div class="field-suggestion">
                ${when(item.alias === 'maxLength', () => html`
                <p class=${classMap({"valid": !this.getValue() || this.getValue()!.length < (item.config!.maxLength as number)})}>
                  Best practice: Length of the text to be lower than ${item.config!.maxLength} characters.
                </p>
                `)}
              </div>
              `)}
            `)}
        </div>
      </umb-property-layout>
    `;
  }

  static styles = css`
    .field-suggestion {
      font-size: 0.9em;
      color: #f44336;
      margin-top: 2px;

      p {
        margin: 0;
      }

      > .valid {
        color: #4caf50;
      }
    }
  `
}

declare global {
  interface HTMLElementTagNameMap {
    "st-metafield-contentfield": MetaFieldsContentField;
  }
}
