import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { GetUmbracoSeoToolkitRobotsTxtResponse, PostUmbracoSeoToolkitRobotsTxtResponse, SeoToolkitService } from "../api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class RobotsTxtSource{
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost){
        this.#host = host;
    }

    async getContent(): Promise<UmbDataSourceResponse<GetUmbracoSeoToolkitRobotsTxtResponse>>{
        return await tryExecuteAndNotify(this.#host, SeoToolkitService.getUmbracoSeoToolkitRobotsTxt());
    }

    async saveContent(content: string, skipValidation: boolean): Promise<UmbDataSourceResponse<PostUmbracoSeoToolkitRobotsTxtResponse>>{
        return await tryExecuteAndNotify(this.#host, SeoToolkitService.postUmbracoSeoToolkitRobotsTxt({
            requestBody: {
                skipValidation: skipValidation,
                content: content
            }
        }))
    }
}