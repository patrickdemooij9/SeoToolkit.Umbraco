import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement, nothing } from "lit";
import MetaFieldsDocumentContext, {
  ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT,
} from "./MetaFieldsDocumentContext";
import { SeoFieldViewModel } from "../api";
import { MetaFieldsSettingsGroup } from "./metaFieldsSettingsGroups";

import "./../components/MetaFieldsSettingsField.element";
import { UmbInputDocumentElement } from "@umbraco-cms/backoffice/document";
import { MetaFieldsSettingsField } from "./../components/MetaFieldsSettingsField.element";

@customElement("st-metafield-document-view")
export default class MetaFieldsDocumentView extends UmbElementMixin(
  LitElement
) {
  #context?: MetaFieldsDocumentContext;

  @state()
  _groups: MetaFieldsSettingsGroup[] = [];

  @state()
  _activeGroupAlias?: string;

  @state()
  _inheritanceId?: string;

  @state()
  _hasInheritance: boolean = false;

  constructor() {
    super();

    this.consumeContext(ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT, (instance) => {
      if (!instance) {
        return;
      }
      this.#context = instance;

      this.observe(instance.groups, (value) => {
        this._groups = value;
      });

      this.observe(instance.model, (value) => {
        this._hasInheritance = !!value.inheritance && !!value.inheritance.id;
        this._inheritanceId = value.inheritance?.id ?? undefined;
      });

      this.observe(instance.activeGroupAlias(), (value) => {
        this._activeGroupAlias = value;
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
    const activeGroup =
      this._groups.find((group) => group.alias === this._activeGroupAlias) ??
      this._groups[0];

    return html`
      <div class="inheritance">
        <umb-icon name="icon-split-alt"></umb-icon>
        <div class="inheritance-label">
          <strong>Inherit from</strong>
          <small>Fields use the values of this type unless overridden.</small>
        </div>
        <umb-input-document-type
          class="inheritance-picker"
          @change=${this.#onInheritanceSelect}
          .selection=${this._inheritanceId ? [this._inheritanceId] : []}
          .max=${1}
          .documentTypesOnly=${true}
          .value=${this._inheritanceId!}
        >
        </umb-input-document-type>
      </div>
      ${activeGroup ? this.#renderGroup(activeGroup) : nothing}
    `;
  }

  #renderGroup(group: MetaFieldsSettingsGroup) {
    return html`
      <uui-box>
        <div slot="headline" class="group-headline">
          <umb-icon name=${group.icon ?? "icon-settings"}></umb-icon>
          <span>${group.name}</span>
        </div>
        ${group.description
          ? html`<p class="group-description">${group.description}</p>`
          : nothing}
        ${repeat(
          group.fields,
          (field) => field.alias,
          (field) => html`
            <st-metafield-settingsfield
              .field=${field}
              .view=${field.editor!.view!}
              orientation="vertical"
              .hasGlobalInheritance=${this._hasInheritance}
              @toggle-inheritance=${() => this.#onInheritanceToggle(field)}
              @change=${this.#onPropertyDataChange}
            ></st-metafield-settingsfield>
          `
        )}
      </uui-box>
    `;
  }

  static styles = css`
    :host {
      display: block;
    }

    .inheritance {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
      padding: var(--uui-size-space-4) var(--uui-size-space-5);
      margin-bottom: var(--uui-size-space-5);
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
      box-shadow: var(--uui-shadow-depth-1);
    }

    .inheritance-label {
      display: flex;
      flex-direction: column;
    }

    .inheritance-label small {
      color: var(--uui-color-text-alt);
    }

    .inheritance-picker {
      flex: 1;
    }

    .group-headline {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .group-description {
      margin: 0 0 var(--uui-size-space-4);
      color: var(--uui-color-text-alt);
    }
  `;
}
