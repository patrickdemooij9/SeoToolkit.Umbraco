import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkitMetaFields } from "../api";

export class MetaFieldsSchemaSource {
    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    getSchemas(){
        return tryExecute(this.#host, BackofficeSeoToolkitMetaFields.getUmbracoSeoToolkitSchemaTypes());
    }
}