import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { BackofficeSeoToolkitSitemapService, SitemapPageTypeSettingsPostModel } from "../api";

export class PageSettingsSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getPageSettings(contentTypeGuid: string) {
        return await tryExecute(this.#host, BackofficeSeoToolkitSitemapService.getUmbracoSeoToolkitSitemapSitemapSettings({
            query: {
                contentTypeGuid: contentTypeGuid
            }
        }));
    };

    async setPageSettings(settings: SitemapPageTypeSettingsPostModel){
        await tryExecute(this.#host, BackofficeSeoToolkitSitemapService.postUmbracoSeoToolkitSitemapSitemapSettings({
            body: settings
        }));
    };
}