import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import NotFoundSource from "./NotFoundSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";

export class NotFoundRepository extends UmbControllerBase {
  #source: NotFoundSource;

  constructor(host: UmbControllerHost) {
    super(host);

    this.#source = new NotFoundSource(host);
  }

  get() {
    return this.#source.get();
  }

  save(data: string) {
    return this.#source.save(data);
  }
}
