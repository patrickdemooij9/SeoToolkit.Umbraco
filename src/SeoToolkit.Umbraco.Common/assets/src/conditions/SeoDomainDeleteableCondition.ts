import {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from "@umbraco-cms/backoffice/extension-api";
import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { ST_DOMAIN_DETAIL_TOKEN_CONTEXT } from "../workspaces/SeoToolkitDomainContext";

export type SeoDeleteableConditionConfig = UmbConditionConfigBase;

export class SeoDomainDeletableCondition
  extends UmbConditionBase<SeoDeleteableConditionConfig>
  implements UmbExtensionCondition
{
  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<SeoDeleteableConditionConfig>
  ) {
    super(host, args);

    this.consumeContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT, (context) => {
      this.observe(context?.domain, (value) => {
        if (!value) {
          return;
        }

        this.permitted = value.id !== 0 && location.href.includes('~', 2);
      });
    });
  }
}
