import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  CreateSiteAuditRequest,
  ResourceQuery,
  SiteAuditApi,
} from "./SiteAuditApi";

export class SiteAuditSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSiteAudits(skip = 0, take = 100) {
    return await tryExecute(this.#host, SiteAuditApi.getRuns(skip, take));
  }

  async get(id: number) {
    return await tryExecute(this.#host, SiteAuditApi.getRun(id));
  }

  /**
   * Progress while a crawl is going. Separate from `get` on purpose: this reads a single row,
   * so watching a ten thousand page crawl costs no more than watching a ten page one.
   */
  async getStatus(id: number) {
    return await tryExecute(this.#host, SiteAuditApi.getStatus(id));
  }

  async getSummary(runId: number) {
    return await tryExecute(this.#host, SiteAuditApi.getSummary(runId));
  }

  async getResources(runId: number, query: ResourceQuery = {}) {
    return await tryExecute(this.#host, SiteAuditApi.getResources(runId, query));
  }

  async getResource(runId: number, resourceId: number) {
    return await tryExecute(this.#host, SiteAuditApi.getResource(runId, resourceId));
  }

  async getIssues(runId: number, skip = 0, take = 50, checkAlias?: string, severity?: string) {
    return await tryExecute(
      this.#host,
      SiteAuditApi.getIssues(runId, skip, take, checkAlias, severity)
    );
  }

  async save(model: CreateSiteAuditRequest) {
    return await tryExecute(this.#host, SiteAuditApi.create(model));
  }

  async delete(ids: number[]) {
    return await tryExecute(this.#host, SiteAuditApi.remove(ids));
  }

  async stopAudit(id: number) {
    return await tryExecute(this.#host, SiteAuditApi.stop(id));
  }

  async getConfiguration() {
    return await tryExecute(this.#host, SiteAuditApi.getCreateOptions());
  }

  async getStartNode(nodeId: string) {
    return await tryExecute(this.#host, SiteAuditApi.getStartNode(nodeId));
  }
}
