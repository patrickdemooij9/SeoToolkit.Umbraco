import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { GetUmbracoSeoToolkitScriptManagerDefinitionsResponse, GetUmbracoSeoToolkitScriptManagerScriptsResponse, ScriptDetailViewModel, SeoToolkitScriptManagerService } from "../api";

export class ScriptManagerSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getScript(id: number){
        return await tryExecute(this.#host, SeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerScript({
            query: {
                id: id
            }
        }));
    }

    async getScripts(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitScriptManagerScriptsResponse>>{
        return await tryExecute(this.#host, SeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerScripts());
    }

    async saveScript(model: ScriptDetailViewModel){
        return await tryExecute(this.#host, SeoToolkitScriptManagerService.postUmbracoSeoToolkitScriptManagerScript({
            body: {
                id: model.id,
                name: model.name!,
                definitionAlias: model.definitionAlias!,
                fields: model.config
            }
        }));
    }

    async deleteScripts(ids: number[]){
        return await tryExecute(this.#host, SeoToolkitScriptManagerService.deleteUmbracoSeoToolkitScriptManagerScript({
            body: {
                ids: ids
            }
        }));
    }

    async getScriptDefinitions(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitScriptManagerDefinitionsResponse>>{
        return await tryExecute(this.#host, SeoToolkitScriptManagerService.getUmbracoSeoToolkitScriptManagerDefinitions());
    }
}