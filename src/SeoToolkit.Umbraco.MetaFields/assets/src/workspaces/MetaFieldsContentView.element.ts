import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import MetaFieldsContentContext, {
  ST_METAFIELDS_CONTENT_TOKEN_CONTEXT,
} from "./MetaFieldsContentContext";
import { MetaFieldsSettingsViewModel } from "../api";

import "./../components/MetaFieldsContentField.element";
import "./../previewers/SeoContentPreviewer.element";
import { MetaFieldsContentField } from "./../components/MetaFieldsContentField.element";
import { UMB_PROPERTY_DATASET_CONTEXT } from "@umbraco-cms/backoffice/property";

@customElement("st-metafield-content-view")
export default class MetaFieldsContentView extends UmbElementMixin(LitElement) {
  #context?: MetaFieldsContentContext;

  @state()
  _model?: MetaFieldsSettingsViewModel;

  @state()
  _culture?: string;

  constructor() {
    super();

    this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
      this._culture = datasetContext.getVariantId().culture ?? 'invariant';

      this.consumeContext(ST_METAFIELDS_CONTENT_TOKEN_CONTEXT, (instance) => {
        this.#context = instance;
        
        instance
          .getModel(this._culture!)
          .subscribe((value) => {
            this._model = value;
          });
      });
    });
  }

  #getFieldsByGroup(groupAlias: string) {
    return this._model?.fields?.filter((item) => item.groupAlias == groupAlias);
  }

  #onPropertyDataChange(e: Event) {
    const field = (e.target as MetaFieldsContentField).field;
    this.#context?.updateField(this._culture!, field!.alias!, field!.userValue);
  }

  override render() {
    return html`
      ${when(
        this._model,
        () => html`
          <div>
            ${repeat(
              this._model!.groups!,
              (group) => group.alias,
              (group) => html`
                <uui-box headline=${group.name!} class="seo-group">
                  <div slot="header"><small>${group.description}</small></div>
                  <div class="content">
                    <div class="fields">
                      ${repeat(
                        this.#getFieldsByGroup(group.alias!)!,
                        (item) => item.alias,
                        (item) => html`
                          <st-metafield-contentfield
                            .field=${item}
                            .view=${item.editView!}
                            @change=${this.#onPropertyDataChange}
                          ></st-metafield-contentfield>
                        `
                      )}
                    </div>
                    <div class="preview">
                      <st-contentpreviewer
                        .group=${group.alias!}
                        .fields=${this.#getFieldsByGroup(group.alias!)}
                      >
                      </st-contentpreviewer>
                    </div>
                  </div>
                </uui-box>
              `
            )}
          </div>
        `
      )}
    `;
  }

  static styles = [
    css`
      .seo-group {
        margin-bottom: 16px;
      }

      .content {
        display: flex;
        gap: 16px;

        .fields {
          width: 60%;
        }
      }
    `,
  ];
}
