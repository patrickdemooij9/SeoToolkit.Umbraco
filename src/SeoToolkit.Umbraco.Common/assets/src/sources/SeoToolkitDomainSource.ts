import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { BackofficeSeoToolkit, SeoDomainCollection } from "../api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

export class SeoToolkitDomainSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(domainId: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.getUmbracoSeoToolkitDomainsGet({
        query: {
          domainId: domainId,
        },
      })
    );
  }

  async getPredefined(umbracoDomainId: number) {
    return BackofficeSeoToolkit.getUmbracoSeoToolkitDomainsGetPredefined({
      query: {
        umbracoDomainId: umbracoDomainId
      }
    })
  }

  async saveDomain(domain: SeoDomainCollection) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.postUmbracoSeoToolkitDomainsSave({
        body: domain,
      })
    );
  }

  async delete(domainId: string) {
    return await tryExecute(this.#host, BackofficeSeoToolkit.deleteUmbracoSeoToolkitDomainsDelete({
      query: {
        domainId: domainId
      }
    }))
  }

  async getConfig() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.getUmbracoSeoToolkitDomainsConfig()
    );
  }
}
