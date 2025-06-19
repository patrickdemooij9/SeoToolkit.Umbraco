import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { SeoSettingsPostModel, SeoToolkitService } from "../api";

export class SeoToolkitSettingsSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSettings(contentTypeId: string) {
    return await tryExecute(
      this.#host,
      SeoToolkitService.getUmbracoSeoToolkitSettingsSeoSettings({
        query: { contentTypeId },
      })
    );
  }

  async postSettings(settings: SeoSettingsPostModel) {
    return await tryExecute(
      this.#host,
      SeoToolkitService.postUmbracoSeoToolkitSettingsSeoSettings({
        body: settings,
      })
    );
  }
}
