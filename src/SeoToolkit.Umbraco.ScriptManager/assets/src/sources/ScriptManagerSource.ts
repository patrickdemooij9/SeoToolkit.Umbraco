import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { GetUmbracoSeoToolkitScriptManagerDefinitionsResponse, GetUmbracoSeoToolkitScriptManagerScriptsResponse, ScriptDetailViewModel, SeoToolkitScriptManagerService } from "../api";

export class ScriptManagerSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getScript(id: number){
        return await tryExecuteAndNotify(this.#host, SeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerScript({
            id: id
        }));
    }

    async getScripts(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitScriptManagerScriptsResponse>>{
        return await tryExecuteAndNotify(this.#host, SeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerScripts());
    }

    async saveScript(model: ScriptDetailViewModel){
        return await tryExecuteAndNotify(this.#host, SeoToolkitScriptManagerService.postUmbracoSeoToolkitScriptManagerScript({
            requestBody: {
                id: model.id,
                name: model.name,
                definitionAlias: model.definitionAlias,
                fields: model.config
            }
        }));
    }

    async deleteScripts(ids: number[]){
        return await tryExecuteAndNotify(this.#host, SeoToolkitScriptManagerService.deleteUmbracoSeoToolkitScriptManagerScript({
            requestBody: {
                ids: ids
            }
        }));
    }

    async getScriptDefinitions(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitScriptManagerDefinitionsResponse>>{
        return await tryExecuteAndNotify(this.#host, SeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerDefinitions());
    }
}