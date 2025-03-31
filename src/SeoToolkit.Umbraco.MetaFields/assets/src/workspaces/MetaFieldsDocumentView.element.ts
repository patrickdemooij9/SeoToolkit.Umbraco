import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import MetaFieldsDocumentContext, {
  ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT,
} from "./MetaFieldsDocumentContext";
import { SeoFieldViewModel } from "../api";

import "./../components/MetaFieldsSettingsField.element";
import { UmbInputDocumentElement } from "@umbraco-cms/backoffice/document";
import { MetaFieldsSettingsField } from "./../components/MetaFieldsSettingsField.element";

@customElement("st-metafield-document-view")
export default class MetaFieldsDocumentView extends UmbElementMixin(
  LitElement
) {
  #context?: MetaFieldsDocumentContext;

  @state()
  _fields: SeoFieldViewModel[] = [];

  @state()
  _inheritanceId?: string;

  @state()
  _hasInheritance: boolean = false;

  constructor() {
    super();

    this.consumeContext(ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT, (instance) => {
      this.#context = instance;

      instance.model.subscribe((value) => {
        this._fields = value.fields ?? [];

        this._hasInheritance = !!value.inheritance && !!value.inheritance.id;
        this._inheritanceId = value.inheritance?.id ?? undefined;
      });
    });
  }

  #onInheritanceSelect(e: Event) {
    const value = (e.target as UmbInputDocumentElement).value;
    this.#context?.setInheritance(value);
  }

  #onPropertyDataChange(e: Event) {
    const field = (e.target as MetaFieldsSettingsField).field;
    this.#context?.updateField(field!.alias!, field!.value);
  }

  #onInheritanceToggle(field: SeoFieldViewModel) {
    this.#context?.toggleInheritance(field.alias!);
  }

  render() {
    return html`
      <uui-box>
      <umb-property-layout
            label="Inheritance"
            description="Control from which type this should be inherited. It'll then use the values from that type."
          >
            <umb-input-document-type
              slot="editor"
              @change=${this.#onInheritanceSelect}
              .selection=${this._inheritanceId ? [this._inheritanceId] : []}
              .max=${1}
              .documentTypesOnly=${true}
              .value=${this._inheritanceId!}
            >
            </umb-input-document-type>
          </umb-property-layout>
        ${repeat(
          this._fields,
          (field) => field.alias,
          (field) => html`
            <st-metafield-settingsfield
              .field=${field}
              .view=${field.editor!.view!}
              .hasGlobalInheritance=${this._hasInheritance}
              @toggle-inheritance=${() => this.#onInheritanceToggle(field)}
              @change=${this.#onPropertyDataChange}
            ></st-metafield-settingsfield>
          `
        )}
      </uui-box>
    `;
  }
}
