import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { SeoToolkitNotFoundService } from "../api";

export default class NotFoundSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get() {
    return await tryExecute(
      this.#host,
      SeoToolkitNotFoundService.getUmbracoSeoToolkitNotFoundNotFound()
    );
  }

  async save(data: string) {
    return await tryExecute(
      this.#host,
      SeoToolkitNotFoundService.postUmbracoSeoToolkitNotFoundNotFound({
        query: {
          data
        },
      })
    );
  }
}
