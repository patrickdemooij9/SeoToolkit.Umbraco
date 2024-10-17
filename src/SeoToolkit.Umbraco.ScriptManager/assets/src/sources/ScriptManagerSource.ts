import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { GetUmbracoSeoToolkitAllResponse, SeoToolkitService } from "../api";

export class ScriptManagerSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getScripts(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitAllResponse>>{
        return await tryExecuteAndNotify(this.#host, SeoToolkitService.getUmbracoSeoToolkitAll());
    }
}