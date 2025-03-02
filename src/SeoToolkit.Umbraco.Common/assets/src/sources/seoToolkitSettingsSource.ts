import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
import { SeoSettingsPostModel, SeoToolkitService } from "../api";

export class SeoToolkitSettingsSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSettings(contentTypeId: string) {
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitService.getUmbracoSeoToolkitSettingsSeoSettings({
        contentTypeId
      })
    );
  }

  async postSettings(settings: SeoSettingsPostModel) {
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitService.postUmbracoSeoToolkitSettingsSeoSettings({
        requestBody: settings,
      })
    );
  }
}
