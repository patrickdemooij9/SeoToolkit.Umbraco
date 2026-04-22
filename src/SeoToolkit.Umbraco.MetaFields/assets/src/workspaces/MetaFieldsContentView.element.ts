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

  @state()
  _isAIAvailable = false;

  @state()
  _isGenerating = false;

  constructor() {
    super();

    this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
      if (!datasetContext) {
        return;
      }
      this._culture = datasetContext.getVariantId().culture ?? 'invariant';

      this.consumeContext(ST_METAFIELDS_CONTENT_TOKEN_CONTEXT, (instance) => {
        if (!instance) {
          return;
        }
        this.#context = instance;
        
        this.observe(instance.getModel(this._culture!), (value) => {
          this._model = value;
        });

        this.observe(instance.isAIAvailable, (value) => {
          this._isAIAvailable = value;
        });

        this.observe(instance.isGenerating, (value) => {
          this._isGenerating = value;
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

  async #onGenerateWithAI() {
    await this.#context?.generate(this._culture!);
  }

  override render() {
    return html`
      ${when(
        this._model,
        () => html`
          <div>
            ${when(
              this._isAIAvailable,
              () => html`
                <div class="ai-toolbar">
                  <uui-button
                    look="secondary"
                    label="Generate with AI"
                    ?disabled=${this._isGenerating}
                    @click=${this.#onGenerateWithAI}
                  >
                    ${when(
                      this._isGenerating,
                      () => html`<uui-loader-circle></uui-loader-circle>&nbsp;Generating…`,
                      () => html`✨ Generate with AI`
                    )}
                  </uui-button>
                  <small class="ai-hint">AI suggestions will be applied to text meta fields. Review and save when ready.</small>
                </div>
              `
            )}
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

      .ai-toolbar {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 16px;
        padding: 12px;
        background: var(--uui-color-surface-alt, #f5f5f5);
        border-radius: var(--uui-border-radius, 3px);
      }

      .ai-hint {
        color: var(--uui-color-text-alt, #888);
      }
    `,
  ];
}
