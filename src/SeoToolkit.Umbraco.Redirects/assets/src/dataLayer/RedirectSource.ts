import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { GetUmbracoSeoToolkitRedirectsRedirectsResponse, ImportRedirectsFileExtension, SaveRedirectPostModel, SeoToolkitRedirectsService } from "../api";
import { tryExecuteAndNotify, UmbResourceController } from '@umbraco-cms/backoffice/resources';

export class RedirectSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getRedirects(pageNumber: number, pageSize: number, orderBy?: string, orderDirection?: string, search?: string): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitRedirectsRedirectsResponse>>
    {
        return await tryExecuteAndNotify(this.#host, SeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsRedirects({
            pageNumber,
            pageSize,
            orderBy,
            orderDirection,
            search
        }));
    };

    async get(id: number){
        return await tryExecuteAndNotify(this.#host, SeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsRedirect({
            id: id
        }));
    }

    async save(redirect: SaveRedirectPostModel){
        await tryExecuteAndNotify(this.#host, SeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsRedirect({
            requestBody: redirect
        }));
    };

    async delete(ids: number[]){
        await tryExecuteAndNotify(this.#host, SeoToolkitRedirectsService.deleteUmbracoSeoToolkitRedirectsRedirect({
            requestBody: {
                ids: ids
            }
        }))
    }

    async getDomains(){
        return await tryExecuteAndNotify(this.#host, SeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsDomains());
    }

    async verifyImport(fileExtension: string, tempFileId: string, domain?: number) {
        return UmbResourceController.tryExecute(SeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsValidate({
            fileExtension: fileExtension! as ImportRedirectsFileExtension,
            domain: domain,
            tempFileId: tempFileId
        }));
    }

    async submitImport(){
        return await tryExecuteAndNotify(this.#host, SeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsImport());
    }
}