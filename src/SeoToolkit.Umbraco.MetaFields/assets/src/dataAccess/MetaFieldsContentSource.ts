import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
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
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsMetaFields({
        nodeGuid: contentGuid,
        culture
      })
    );
  }

  async save(model: MetaFieldsSettingsPostViewModel) {
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitMetaFieldsService.postUmbracoSeoToolkitMetaFieldsMetaFields({
        requestBody: model,
      })
    );
  }
}
