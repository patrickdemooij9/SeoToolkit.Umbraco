import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { GetUmbracoSeoToolkitModulesResponse, BackofficeSeoToolkit } from '../api';

export interface IModuleSource {
    getModules(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitModulesResponse>>;
}

export class ModuleSource implements IModuleSource {

    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getModules(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitModulesResponse>> {
        return await tryExecute(this.#host, BackofficeSeoToolkit.getUmbracoSeoToolkitModules())
    }
}