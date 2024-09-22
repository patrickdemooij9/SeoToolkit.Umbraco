import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { ModuleSource } from "../sources/moduleSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";

export class ModuleRepository extends UmbControllerBase {
    #moduleDataSource: ModuleSource;

    constructor(host: UmbControllerHost) {
        super(host);
        this.#moduleDataSource = new ModuleSource(this);
    }

    async getModules() {
        return this.#moduleDataSource.getModules();
    }
}