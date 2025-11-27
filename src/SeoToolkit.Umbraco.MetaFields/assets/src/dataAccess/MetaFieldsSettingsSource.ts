import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import {
  BackofficeSeoToolkitMetaFieldsService,
  DocumentTypeSettingsPostViewModel
} from "../api";

export class MetaFieldsSettingsSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async get(contentTypeGuid: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsSettingsMetaFieldsSettings(
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
      BackofficeSeoToolkitMetaFieldsService.getUmbracoSeoToolkitMetaFieldsSettingsMetaFieldsAdditionalFields()
    );
  }

  async save(model: DocumentTypeSettingsPostViewModel) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitMetaFieldsService.postUmbracoSeoToolkitMetaFieldsSettingsMetaFieldsSettings(
        {
          body: model,
        }
      )
    );
  }
}
