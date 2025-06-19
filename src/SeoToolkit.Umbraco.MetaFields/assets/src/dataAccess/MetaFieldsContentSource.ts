import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import {
  MetaFieldsSettingsPostViewModel,
  SeoToolkitMetaFieldsService,
} from "../api";

export class MetaFieldsContentSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(contentGuid: string, culture: string) {
    return await tryExecute(
      this.#host,
      SeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsMetaFields({
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
      SeoToolkitMetaFieldsService.postUmbracoSeoToolkitMetaFieldsMetaFields({
        body: model,
      })
    );
  }

  async getImagePreview(mediaId: string) {
    return await tryExecute(
      this.#host,
      SeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsImagePreview({
        query: {
          mediaId,
        },
      })
    );
  }
}
