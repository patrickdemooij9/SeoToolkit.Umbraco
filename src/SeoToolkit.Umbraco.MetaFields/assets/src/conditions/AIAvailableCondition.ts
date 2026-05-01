import {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from "@umbraco-cms/backoffice/extension-api";
import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { MetaFieldsAISource } from "../dataAccess/MetaFieldsAISource";

export type AIAvailableConditionConfig = UmbConditionConfigBase;

/**
 * Condition that is permitted only when the SeoToolkit AI integration package
 * is installed and the `/umbraco/seoToolkitAI/isAvailable` endpoint responds successfully.
 */
export class AIAvailableCondition
  extends UmbConditionBase<AIAvailableConditionConfig>
  implements UmbExtensionCondition
{
  #source: MetaFieldsAISource;

  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<AIAvailableConditionConfig>
  ) {
    super(host, args);
    this.#source = new MetaFieldsAISource(host);
    this.#checkAvailability();
  }

  async #checkAvailability() {
    try {
      const { data } = await this.#source.isAvailable();
      this.permitted = data === true;
    } catch {
      this.permitted = false;
    }
  }
}
