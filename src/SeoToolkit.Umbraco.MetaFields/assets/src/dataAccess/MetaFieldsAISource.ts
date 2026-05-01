import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkitAI } from "../api";

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
    return tryExecute(this.#host, BackofficeSeoToolkitAI.getUmbracoSeoToolkitAiIsAvailable());
  }

  async generate(nodeId: string, culture: string) {
    return tryExecute(this.#host, BackofficeSeoToolkitAI.postUmbracoSeoToolkitAiGenerate({
      body: {
        nodeId: nodeId,
        culture: culture,
      }
    }));
  }
}
