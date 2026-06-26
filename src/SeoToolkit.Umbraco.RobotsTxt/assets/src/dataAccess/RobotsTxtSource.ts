import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { BackofficeSeoToolkitRobotsTxt, GetUmbracoSeoToolkitRobotsTxtResponse, PostUmbracoSeoToolkitRobotsTxtResponse } from "../api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class RobotsTxtSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getContent(domainId: string | undefined): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitRobotsTxtResponse>>{
        return await tryExecute(this.#host, BackofficeSeoToolkitRobotsTxt.getUmbracoSeoToolkitRobotsTxt({
            query: {
                domainId: domainId
            }
        }));
    }

    async saveContent(content: string, domainId: string | undefined, skipValidation: boolean): Promise<UmbDataSourceResponse<PostUmbracoSeoToolkitRobotsTxtResponse>>{
        return await tryExecute(this.#host, BackofficeSeoToolkitRobotsTxt.postUmbracoSeoToolkitRobotsTxt({
            body: {
                skipValidation: skipValidation,
                content: content,
                domainId: domainId
            }
        }))
    }
}