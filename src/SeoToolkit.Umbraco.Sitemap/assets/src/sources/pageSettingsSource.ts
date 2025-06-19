import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { SeoToolkitSitemapService, SitemapPageTypeSettingsPostModel } from "../api";

export class PageSettingsSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getPageSettings(contentTypeGuid: string) {
        return await tryExecute(this.#host, SeoToolkitSitemapService.getUmbracoSeoToolkitSitemapSitemapSettings({
            query: {
                contentTypeGuid: contentTypeGuid
            }
        }));
    };

    async setPageSettings(settings: SitemapPageTypeSettingsPostModel){
        await tryExecute(this.#host, SeoToolkitSitemapService.postUmbracoSeoToolkitSitemapSitemapSettings({
            body: settings
        }));
    };
}