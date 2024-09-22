import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { GetUmbracoSeoToolkitModulesResponse, SeoToolkitService } from '../api';

export interface IModuleSource {
    getModules(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitModulesResponse>>;
}

export class ModuleSource implements IModuleSource {

    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getModules(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitModulesResponse>> {
        return await tryExecuteAndNotify(this.#host, SeoToolkitService.getUmbracoSeoToolkitModules())
    }
}