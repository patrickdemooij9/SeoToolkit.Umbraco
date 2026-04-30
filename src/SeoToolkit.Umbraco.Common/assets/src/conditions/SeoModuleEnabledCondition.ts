import {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from "@umbraco-cms/backoffice/extension-api";
import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { ModuleRepository } from "../repositories/moduleRepository";

export type SeoModuleEnabledConditionConfig = UmbConditionConfigBase & {
  moduleAlias: string;
};

export class SeoModuleEnabledCondition
  extends UmbConditionBase<SeoModuleEnabledConditionConfig>
  implements UmbExtensionCondition
{
  #repository: ModuleRepository = new ModuleRepository(
    this
  );

  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<SeoModuleEnabledConditionConfig>
  ) {
    super(host, args);

    this.#repository.isEnabled(this.config.moduleAlias).then((resp) => {
      this.permitted = resp.data ?? false;
    });
  }
}
