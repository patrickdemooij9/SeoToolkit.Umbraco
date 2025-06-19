import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { GetUmbracoSeoToolkitRobotsTxtResponse, PostUmbracoSeoToolkitRobotsTxtResponse, SeoToolkitService } from "../api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class RobotsTxtSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getContent(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitRobotsTxtResponse>>{
        return await tryExecute(this.#host, SeoToolkitService.getUmbracoSeoToolkitRobotsTxt());
    }

    async saveContent(content: string, skipValidation: boolean): Promise<UmbDataSourceResponse<PostUmbracoSeoToolkitRobotsTxtResponse>>{
        return await tryExecute(this.#host, SeoToolkitService.postUmbracoSeoToolkitRobotsTxt({
            body: {
                skipValidation: skipValidation,
                content: content
            }
        }))
    }
}