import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
import { SeoToolkitNotFoundService } from "../api";

export default class NotFoundSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get() {
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitNotFoundService.getUmbracoSeoToolkitNotFoundNotFound()
    );
  }

  async save(data: string) {
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitNotFoundService.postUmbracoSeoToolkitNotFoundNotFound({
        data,
      })
    );
  }
}
