import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import NotFoundSource from "./NotFoundSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";

export class NotFoundRepository extends UmbControllerBase {
  #source: NotFoundSource;

  constructor(host: UmbControllerHost) {
    super(host);

    this.#source = new NotFoundSource(host);
  }

  get(domainId?: string) {
    return this.#source.get(domainId);
  }

  save(data?: string, domainId?: string) {
    return this.#source.save(data, domainId);
  }
}
