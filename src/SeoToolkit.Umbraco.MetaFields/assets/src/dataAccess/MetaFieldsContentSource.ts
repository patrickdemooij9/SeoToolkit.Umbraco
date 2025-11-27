import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import {
  BackofficeSeoToolkitMetaFieldsService,
  MetaFieldsSettingsPostViewModel,
} from "../api";

export class MetaFieldsContentSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(contentGuid: string, culture: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsMetaFields({
        query: {
          nodeGuid: contentGuid,
          culture,
        },
      })
    );
  }

  async save(model: MetaFieldsSettingsPostViewModel) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFieldsService.postUmbracoSeoToolkitMetaFieldsMetaFields({
        body: model,
      })
    );
  }

  async getImagePreview(mediaId: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsImagePreview({
        query: {
          mediaId,
        },
      })
    );
  }
}
