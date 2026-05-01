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
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import {
  ST_AI_SUGGESTIONS_MODAL,
  type MetaFieldsAISuggestionsModalConfig,
  type MetaFieldsAISuggestionsModalValue,
} from "../popups/MetaFieldsAISuggestionsModal.element";

@customElement("st-metafield-content-view")
export default class MetaFieldsContentView extends UmbElementMixin(LitElement) {
  #context?: MetaFieldsContentContext;

  @state()
  _model?: MetaFieldsSettingsViewModel;

  @state()
  _culture?: string;

  @state()
  _isAIAvailable: boolean = false;

  @state()
  _isGenerating: boolean = false;

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

  async #generateWithAI() {
    if (!this.#context || !this._culture) return;

    this._isGenerating = true;
    try {
      const suggestions = await this.#context.generateSuggestions(this._culture);
      if (!suggestions.length) return;

      const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
      if (!modalManager) return;

      const modal = modalManager.open<
        MetaFieldsAISuggestionsModalConfig,
        MetaFieldsAISuggestionsModalValue
      >(this, ST_AI_SUGGESTIONS_MODAL, {
        modal: { type: "sidebar", size: "medium" },
        data: { suggestions },
        value: [],
      });

      try {
        await modal.onSubmit();
        const accepted = modal.getValue();
        if (accepted?.length) {
          this.#context.applyAISuggestions(this._culture, accepted);
        }
      } catch {
        // user cancelled — do nothing
      }
    } finally {
      this._isGenerating = false;
    }
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
                <div class="view-toolbar">
                  <uui-button
                    look="secondary"
                    ?disabled=${this._isGenerating}
                    @click=${this.#generateWithAI}
                  >✨ Generate with AI</uui-button>
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

      .view-toolbar {
        display: flex;
        justify-content: flex-end;
        margin-bottom: 16px;
      }
    `,
  ];
}

