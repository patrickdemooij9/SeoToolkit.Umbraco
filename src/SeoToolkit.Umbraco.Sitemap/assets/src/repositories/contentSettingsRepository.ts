import { UmbRepositoryBase } from "@umbraco-cms/backoffice/repository";
import { ContentSettingsSource } from "../sources/contentSettingsSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { SitemapContentSettingsPostModel } from "../api";

export class ContentSettingsRepository extends UmbRepositoryBase {
    #source: ContentSettingsSource;

    constructor(host: UmbControllerHost) {
        super(host);
        this.#source = new ContentSettingsSource(host);
    }

    async getContentSettings(nodeKey: string) {
        return this.#source.getContentSettings(nodeKey);
    }

    async setContentSettings(settings: SitemapContentSettingsPostModel) {
        return this.#source.setContentSettings(settings);
    }
}
