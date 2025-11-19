import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { BackofficeSeoToolkitSiteAuditService, CreateAuditPostModel } from "../api";

export class SiteAuditSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSiteAudits() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAuditService.getUmbracoSeoToolkitSiteAuditSiteAudits()
    );
  }

  async get(id: number) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAuditService.getUmbracoSeoToolkitSiteAuditSiteAudit({
        query: {
          id: id,
        }
      })
    );
  }

  async save(model: CreateAuditPostModel) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAuditService.postUmbracoSeoToolkitSiteAuditSiteAudit({
        body: model,
      })
    );
  }

  async delete(ids: number[]) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAuditService.deleteUmbracoSeoToolkitSiteAuditSiteAudit({
        body: {
          ids: ids,
        },
      })
    );
  }

  async stopAudit(id: number) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAuditService.postUmbracoSeoToolkitSiteAuditStopSiteAudit({
        body: {
          id: id,
        },
      })
    );
  }

  async getConfiguration() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitSiteAuditService.getUmbracoSeoToolkitSiteAuditSiteAuditConfiguration()
    );
  }
}
