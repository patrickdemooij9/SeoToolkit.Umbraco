import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkitMetaFields } from "../api";
import { SchemaEntryPostModel } from "../api/types.gen";

export class SchemaEntrySource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  getEntries(ownerType: string, ownerKey: string) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFields.getUmbracoSeoToolkitSchemaEntries({
        query: { ownerType, ownerKey },
      })
    );
  }

  getEntry(id: string) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFields.getUmbracoSeoToolkitSchemaEntriesById({
        path: { id },
      })
    );
  }

  getReusableEntries(ownerType: string, ownerKey: string) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFields.getUmbracoSeoToolkitSchemaEntriesReusable({
        query: { ownerType, ownerKey },
      })
    );
  }

  createEntry(model: SchemaEntryPostModel) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFields.postUmbracoSeoToolkitSchemaEntries({
        body: model,
      })
    );
  }

  updateEntry(id: string, model: SchemaEntryPostModel) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFields.putUmbracoSeoToolkitSchemaEntriesById({
        path: { id },
        body: model,
      })
    );
  }

  deleteEntry(id: string) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFields.deleteUmbracoSeoToolkitSchemaEntriesById({
        path: { id },
      })
    );
  }
}
