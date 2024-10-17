import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { ScriptManagerSource } from "../sources/ScriptManagerSource";
import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";

export class ScriptManagerRepository extends UmbControllerBase {
    #scriptManagerSource: ScriptManagerSource;

    constructor(host: UmbControllerHost){
        super(host);

        this.#scriptManagerSource = new ScriptManagerSource(host);
    }

    async getScripts(){
        return this.#scriptManagerSource.getScripts();
    }
}