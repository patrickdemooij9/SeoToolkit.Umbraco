import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import { SeoFieldViewModel } from "../api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import { createExtensionElement } from "@umbraco-cms/backoffice/extension-api";
import {
  ManifestPropertyEditorUi,
  UmbPropertyEditorConfigCollection,
  UmbPropertyValueChangeEvent,
} from "@umbraco-cms/backoffice/property-editor";

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
        this.dispatchEvent(new UmbPropertyValueChangeEvent());
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
        <div slot="action-menu" class="action-menu">
          <uui-button
            id="popover-trigger"
            popovertarget="property-action-popover"
            data-mark="open-property-actions"
            label=${this.localize.term("actions_viewActionsFor")}
            compact
          >
            <uui-symbol-more id="more-symbol"></uui-symbol-more>
          </uui-button>
          <uui-popover-container id="property-action-popover">
            <umb-popover-layout>
              <div class="actions">
                ${when(
                  this.field!.useInheritedValue,
                  () => html`
                    <uui-button
                      look="placeholder"
                      @click=${this.toggleInheritance}
                      >Override</uui-button
                    >
                  `,
                  () => html`
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
                <uui-button look="placeholder"> Set format </uui-button>
              </div>
            </umb-popover-layout>
          </uui-popover-container>
        </div>
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
              <uui-input placeholder="Format for the field"></uui-input>
              ${this._element}
            `
          )}
        </div>
      </umb-property-layout>
    `;
  }

  static styles = [
    css`
      .action-menu {
        display: inline;
      }

      .actions {
        display: flex;
        flex-direction: column;
        gap: 4px;
        padding: 8px 4px;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "st-metafield-settingsfield": MetaFieldsSettingsField;
  }
}
