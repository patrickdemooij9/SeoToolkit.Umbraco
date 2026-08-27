import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { SiteAuditApi } from "./SiteAuditApi";

export default class SiteAuditContentCheckSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  /** Only the checks that make sense against a single page in isolation. */
  async getPageChecks() {
    return tryExecute(this.#host, SiteAuditApi.getPageChecks());
  }

  async runPageChecks(nodeId: string) {
    return tryExecute(this.#host, SiteAuditApi.runPageChecks(nodeId));
  }
}
