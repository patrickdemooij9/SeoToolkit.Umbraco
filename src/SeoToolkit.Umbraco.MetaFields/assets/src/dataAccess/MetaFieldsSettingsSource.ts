import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import {
  DocumentTypeSettingsPostViewModel,
  SeoToolkitMetaFieldsService,
} from "../api";

export class MetaFieldsSettingsSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(contentTypeGuid: string) {
    return await tryExecute(
      this.#host,
      SeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsSettingsMetaFieldsSettings(
        {
          query: {
            nodeId: contentTypeGuid,
          }
        }
      )
    );
  }

  async getAdditionalFields() {
    return await tryExecute(
      this.#host,
      SeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsSettingsMetaFieldsAdditionalFields()
    );
  }

  async save(model: DocumentTypeSettingsPostViewModel) {
    return await tryExecute(
      this.#host,
      SeoToolkitMetaFieldsService.postUmbracoSeoToolkitMetaFieldsSettingsMetaFieldsSettings(
        {
          body: model,
        }
      )
    );
  }
}
