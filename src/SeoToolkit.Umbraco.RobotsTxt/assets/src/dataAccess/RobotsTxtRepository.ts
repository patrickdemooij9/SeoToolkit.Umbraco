import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { RobotsTxtSource } from "./RobotsTxtSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";

export class RobotsTxtRepository extends UmbControllerBase{
    #robotsTxtSource: RobotsTxtSource;

    constructor(host: UmbControllerHost){
        super(host);

        this.#robotsTxtSource = new RobotsTxtSource(host);
    }

    async getContent(domainId: number | undefined){
        return this.#robotsTxtSource.getContent(domainId);
    }

    async saveContent(content: string, domainId: number | undefined, skipValidation: boolean){
        return this.#robotsTxtSource.saveContent(content, domainId, skipValidation);
    }
}