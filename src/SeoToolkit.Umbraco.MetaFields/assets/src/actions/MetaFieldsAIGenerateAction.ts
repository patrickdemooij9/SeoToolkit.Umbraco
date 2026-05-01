import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbWorkspaceActionArgs,
  UmbWorkspaceActionBase,
} from "@umbraco-cms/backoffice/workspace";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import MetaFieldsContentContext, {
  ST_METAFIELDS_CONTENT_TOKEN_CONTEXT,
} from "../workspaces/MetaFieldsContentContext";
import { UMB_PROPERTY_DATASET_CONTEXT } from "@umbraco-cms/backoffice/property";
import type {
  MetaFieldsAISuggestionsModalConfig,
  MetaFieldsAISuggestionsModalValue,
} from "../popups/MetaFieldsAISuggestionsModal.element";

export const ST_AI_SUGGESTIONS_MODAL = "seoToolkit.modal.aiSuggestions";

export class MetaFieldsAIGenerateAction extends UmbWorkspaceActionBase<MetaFieldsContentContext> {
  constructor(
    host: UmbControllerHost,
    args: UmbWorkspaceActionArgs<MetaFieldsContentContext>
  ) {
    super(host, args);
  }

  override async execute() {
    const metaContext = await this.getContext(ST_METAFIELDS_CONTENT_TOKEN_CONTEXT);
    if (!metaContext) return;

    const datasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
    const culture = datasetContext?.getVariantId().culture ?? "invariant";

    const suggestions = await metaContext.generateSuggestions(culture);
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
        metaContext.applyAISuggestions(culture, accepted);
      }
    } catch {
      // user cancelled — do nothing
    }
  }
}
