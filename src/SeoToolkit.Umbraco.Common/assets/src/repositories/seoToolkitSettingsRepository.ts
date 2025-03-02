import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { SeoToolkitSettingsSource } from "../sources/seoToolkitSettingsSource";
import { SeoSettingsPostModel } from "../api";

export class SeoToolkitSettingsRepository extends UmbControllerBase {
  #source: SeoToolkitSettingsSource;

  constructor(host: UmbControllerHost) {
    super(host);
    this.#source = new SeoToolkitSettingsSource(this);
  }

  async getSettings(contentTypeId: string) {
    return this.#source.getSettings(contentTypeId);
  }

  async setSettings(settings: SeoSettingsPostModel) {
    this.#source.postSettings(settings);
  }
}
