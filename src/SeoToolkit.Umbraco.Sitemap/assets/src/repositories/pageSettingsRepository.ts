import { UmbRepositoryBase } from "@umbraco-cms/backoffice/repository";
import { PageSettingsSource } from "../sources/pageSettingsSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { SitemapPageTypeSettingsPostModel } from "../api";

export default class PageSettingsRepository extends UmbRepositoryBase {
    #source: PageSettingsSource;

    constructor(host: UmbControllerHost) {
        super(host);

        this.#source = new PageSettingsSource(host);
    }

    async getPageSettings(contentTypeGuid: string) {
        return this.#source.getPageSettings(contentTypeGuid);
    }

    async setPageSettings(settings: SitemapPageTypeSettingsPostModel){
        this.#source.setPageSettings(settings);
    }

    async isModuleEnabled() {
        return this.#source.isModuleEnabled();
    }
}