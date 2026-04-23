import {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from "@umbraco-cms/backoffice/extension-api";
import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import PageSettingsRepository from "../repositories/pageSettingsRepository";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";

export type SitemapEnabledConditionConfig = UmbConditionConfigBase;

export class SitemapEnabledCondition
  extends UmbConditionBase<SitemapEnabledConditionConfig>
  implements UmbExtensionCondition
{
  #repository: PageSettingsRepository = new PageSettingsRepository(this);

  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<SitemapEnabledConditionConfig>,
  ) {
    super(host, args);

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
      this.#repository.isModuleEnabled().then((resp) => {
        this.permitted = resp.data ?? false;

        if (!this.permitted) {
          return;
        }

        this.observe(context?.contentTypeUnique, (value) => {
          if (!value) {
            return;
          }

          this.#repository.getPageSettings(value).then((resp) => {
            this.permitted = !resp.data.hideFromSitemap;
          });
        });
      });
    });
  }
}
