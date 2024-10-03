import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { RobotsTxtSource } from "./RobotsTxtSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";

export class RobotsTxtRepository extends UmbControllerBase{
    #robotsTxtSource: RobotsTxtSource;

    constructor(host: UmbControllerHost){
        super(host);

        this.#robotsTxtSource = new RobotsTxtSource(host);
    }

    async getContent(){
        return this.#robotsTxtSource.getContent();
    }

    async saveContent(content: string){
        return this.#robotsTxtSource.saveContent(content);
    }
}