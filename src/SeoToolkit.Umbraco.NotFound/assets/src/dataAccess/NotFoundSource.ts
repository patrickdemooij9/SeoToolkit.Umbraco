import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkitNotFoundService } from "../api";

export default class NotFoundSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(domainId?: number) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitNotFoundService.getUmbracoSeoToolkitNotFoundNotFound({
        query: { domainId }
      })
    );
  }

  async save(data?: string, domainId?: number) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitNotFoundService.postUmbracoSeoToolkitNotFoundNotFound({
        query: {
          data,
          domainId
        },
      })
    );
  }
}
