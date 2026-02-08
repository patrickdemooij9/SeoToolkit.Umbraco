import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { BackofficeSeoToolkitSiteAudit } from "../api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

export default class SiteAuditContentCheckSource {

    #host: UmbControllerHost;

    constructor(host: UmbControllerHost) {
        this.#host = host;
    }

    async getPageChecks(){
        return tryExecute(this.#host, BackofficeSeoToolkitSiteAudit.getUmbracoSeoToolkitSiteAuditPageChecks());
    }

    async runPageChecks(nodeId: string){
        return tryExecute(this.#host, BackofficeSeoToolkitSiteAudit.postUmbracoSeoToolkitSiteAuditRun({
            body: {
                contentId: nodeId
            }
        }))
    }
}