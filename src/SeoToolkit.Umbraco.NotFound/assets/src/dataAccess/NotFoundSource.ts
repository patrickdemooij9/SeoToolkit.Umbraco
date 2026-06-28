import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkitNotFound } from "../api";

export default class NotFoundSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(domainId?: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitNotFound.getUmbracoSeoToolkitNotFoundNotFound({
        query: { domainId }
      })
    );
  }

  async save(data?: string, domainId?: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitNotFound.postUmbracoSeoToolkitNotFoundNotFound({
        query: {
          data,
          domainId
        },
      })
    );
  }
}
