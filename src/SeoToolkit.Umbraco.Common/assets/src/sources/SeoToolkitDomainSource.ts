import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { BackofficeSeoToolkitService, SeoDomainCollection } from "../api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

export class SeoToolkitDomainSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async saveDomain(domain: SeoDomainCollection) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitService.postUmbracoSeoToolkitDomainsSave({
        body: domain,
      })
    );
  }
}
