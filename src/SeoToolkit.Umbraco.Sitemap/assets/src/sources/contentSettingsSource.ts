import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import {BackofficeSeoToolkitSitemap, SitemapContentSettingsPostModel } from "../api";

export class ContentSettingsSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getContentSettings(nodeKey: string) {
        return await tryExecute(this.#host, BackofficeSeoToolkitSitemap.getUmbracoSeoToolkitSitemapContentContentSettings({
            query: { nodeKey }
        }));
    }

    async setContentSettings(settings: SitemapContentSettingsPostModel) {
        return await tryExecute(this.#host, BackofficeSeoToolkitSitemap.postUmbracoSeoToolkitSitemapContentContentSettings({
            body: settings
        }));
    }
}
