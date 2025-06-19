import {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from "@umbraco-cms/backoffice/extension-api";
import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { SeoToolkitSettingsRepository } from "../repositories/seoToolkitSettingsRepository";

export type SeoEnabledConditionConfig = UmbConditionConfigBase;

export class SeoEnabledCondition
  extends UmbConditionBase<SeoEnabledConditionConfig>
  implements UmbExtensionCondition
{
  #repository: SeoToolkitSettingsRepository = new SeoToolkitSettingsRepository(
    this
  );

  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<SeoEnabledConditionConfig>
  ) {
    super(host, args);

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
      context?.contentTypeUnique.subscribe((value) => {
        if (!value) {
          return;
        }

        this.#repository.getSettings(value).then((resp) => {
          this.permitted = resp.data.isEnabled ?? false;
        });
      });
    });
  }
}
