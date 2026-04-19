import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { BackofficeSeoToolkitScriptManagerService, GetUmbracoSeoToolkitScriptManagerDefinitionsResponse, GetUmbracoSeoToolkitScriptManagerScriptsResponse, ScriptDetailViewModel } from "../api";

export class ScriptManagerSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getScript(id: string){
        return await tryExecute(this.#host, BackofficeSeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerScript({
            query: {
                id: id
            }
        }));
    }

    async getScripts(domainId?: string): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitScriptManagerScriptsResponse>>{
        return await tryExecute(this.#host, BackofficeSeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerScripts({
            query: {
                domainId
            }
        }));
    }

    async saveScript(model: ScriptDetailViewModel){
        return await tryExecute(this.#host, BackofficeSeoToolkitScriptManagerService.postUmbracoSeoToolkitScriptManagerScript({
            body: {
                id: model.id,
                key: model.key,
                name: model.name!,
                definitionAlias: model.definitionAlias!,
                fields: model.config,
                domainId: model.domainId,
                sortOrder: model.sortOrder
            }
        }));
    }

    async deleteScripts(ids: string[]){
        return await tryExecute(this.#host, BackofficeSeoToolkitScriptManagerService.deleteUmbracoSeoToolkitScriptManagerScript({
            body: {
                ids: ids
            }
        }));
    }

    async getScriptDefinitions(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitScriptManagerDefinitionsResponse>>{
        return await tryExecute(this.#host, BackofficeSeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerDefinitions());
    }
}
