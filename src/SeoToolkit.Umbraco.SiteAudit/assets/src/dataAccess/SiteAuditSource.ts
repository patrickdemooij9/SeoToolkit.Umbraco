import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { BackofficeSeoToolkitSiteAudit, CreateAuditPostModel } from "../api";

export class SiteAuditSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSiteAudits() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAudit.getUmbracoSeoToolkitSiteAuditSiteAudits()
    );
  }

  async get(id: number) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAudit.getUmbracoSeoToolkitSiteAuditSiteAudit({
        query: {
          id: id,
        }
      })
    );
  }

  async save(model: CreateAuditPostModel) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAudit.postUmbracoSeoToolkitSiteAuditSiteAudit({
        body: model,
      })
    );
  }

  async delete(ids: number[]) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAudit.deleteUmbracoSeoToolkitSiteAuditSiteAudit({
        body: {
          ids: ids,
        },
      })
    );
  }

  async stopAudit(id: number) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAudit.postUmbracoSeoToolkitSiteAuditStopSiteAudit({
        body: {
          id: id,
        },
      })
    );
  }

  async getConfiguration() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAudit.getUmbracoSeoToolkitSiteAuditSiteAuditConfiguration()
    );
  }
}
