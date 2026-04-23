import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import {
  BackofficeSeoToolkit,
  BackofficeSeoToolkitSitemap,
  SitemapPageTypeSettingsPostModel,
} from "../api";

export class PageSettingsSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getPageSettings(contentTypeGuid: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSitemap.getUmbracoSeoToolkitSitemapSitemapSettings({
        query: {
          contentTypeGuid: contentTypeGuid,
        },
      }),
    );
  }

  async setPageSettings(settings: SitemapPageTypeSettingsPostModel) {
    await tryExecute(
      this.#host,
      BackofficeSeoToolkitSitemap.postUmbracoSeoToolkitSitemapSitemapSettings({
        body: settings,
      }),
    );
  }

  async isModuleEnabled() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.getUmbracoSeoToolkitIsEnabled({
        query: {
          moduleAlias: "sitemap",
        },
      }),
    );
  }
}
