import { tryExecute, tryExecuteAndNotify } from "@umbraco-cms/backoffice/resources";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { CreateAuditPostModel, SeoToolkitSiteAuditService } from "../api";

export class SiteAuditSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSiteAudits() {
    return await tryExecuteAndNotify(
      this.#host,
      SeoToolkitSiteAuditService.getUmbracoSeoToolkitSiteAuditSiteAudits()
    );
  }

  async get(id: number) {
    return await tryExecute(
      this.#host,
      SeoToolkitSiteAuditService.getUmbracoSeoToolkitSiteAuditSiteAudit({
        query: {
          id: id,
        }
      })
    );
  }

  async save(model: CreateAuditPostModel) {
    return await tryExecute(
      this.#host,
      SeoToolkitSiteAuditService.postUmbracoSeoToolkitSiteAuditSiteAudit({
        body: model,
      })
    );
  }

  async delete(ids: number[]) {
    return await tryExecute(
      this.#host,
      SeoToolkitSiteAuditService.deleteUmbracoSeoToolkitSiteAuditSiteAudit({
        body: {
          ids: ids,
        },
      })
    );
  }

  async stopAudit(id: number) {
    return await tryExecute(
      this.#host,
      SeoToolkitSiteAuditService.postUmbracoSeoToolkitSiteAuditStopSiteAudit({
        body: {
          id: id,
        },
      })
    );
  }

  async getConfiguration() {
    return await tryExecute(
      this.#host,
      SeoToolkitSiteAuditService.getUmbracoSeoToolkitSiteAuditSiteAuditConfiguration()
    );
  }
}
