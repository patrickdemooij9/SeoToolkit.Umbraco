import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkitMetaFieldsAi } from "../api";

export interface MetaFieldsAIFieldSuggestion {
  alias: string;
  value: string;
}

export interface MetaFieldsAIGenerateResponse {
  suggestions: MetaFieldsAIFieldSuggestion[];
}

export class MetaFieldsAISource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async isAvailable() {
    return tryExecute(this.#host, BackofficeSeoToolkitMetaFieldsAi.getUmbracoSeoToolkitMetaFieldsAiIsAvailable());
  }

  async generate(nodeId: string, culture: string) {
    return tryExecute(this.#host, BackofficeSeoToolkitMetaFieldsAi.postUmbracoSeoToolkitMetaFieldsAiGenerate({
      body: {
        nodeId: nodeId,
        culture: culture,
      }
    }));
  }
}
