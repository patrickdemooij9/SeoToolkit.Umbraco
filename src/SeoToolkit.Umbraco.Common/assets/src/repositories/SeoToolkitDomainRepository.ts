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

  async saveDomain(domain: SeoDomainCollection) {
    return this.#source.saveDomain(domain);
  }
}
