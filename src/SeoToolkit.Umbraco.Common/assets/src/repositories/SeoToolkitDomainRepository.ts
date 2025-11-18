import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { SeoToolkitDomainSource } from "../sources/SeoToolkitDomainSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { SeoDomainCollection } from "../api";

export class SeoToolkitDomainRepository extends UmbControllerBase {
  #source: SeoToolkitDomainSource;

  constructor(host: UmbControllerHost) {
    super(host);
    this.#source = new SeoToolkitDomainSource(this);
  }

  async get(domainId: string) {
    return this.#source.get(domainId);
  }

  async getPredefined(umbracoDomainId: string) {
    return this.#source.getPredefined(umbracoDomainId);
  }

  async saveDomain(domain: SeoDomainCollection) {
    return this.#source.saveDomain(domain);
  }

  async delete(domainId: string) {
    return this.#source.delete(domainId);
  }

  async getConfig() {
    return this.#source.getConfig();
  }
}
