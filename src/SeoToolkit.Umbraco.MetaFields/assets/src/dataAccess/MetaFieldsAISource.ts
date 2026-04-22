import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { client } from "../api";

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
    return tryExecute(
      this.#host,
      client.get<{ isAvailable: boolean }, unknown, false>({
        url: "/umbraco/seoToolkitMetaFieldsAI/isAvailable",
        throwOnError: false,
      })
    );
  }

  async generate(nodeId: string, culture: string) {
    return tryExecute(
      this.#host,
      client.post<MetaFieldsAIGenerateResponse, unknown, false>({
        url: "/umbraco/seoToolkitMetaFieldsAI/generate",
        body: { nodeId, culture },
        headers: { "Content-Type": "application/json" },
        throwOnError: false,
      })
    );
  }
}
