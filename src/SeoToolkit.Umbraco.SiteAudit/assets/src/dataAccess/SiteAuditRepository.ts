import {
  UmbCollectionFilterModel,
  UmbCollectionRepository,
} from "@umbraco-cms/backoffice/collection";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbPagedModel,
  UmbRepositoryBase,
  UmbRepositoryResponse,
} from "@umbraco-cms/backoffice/repository";
import { SiteAuditSource } from "./SiteAuditSource";
import { CreateSiteAuditRequest, ResourceQuery } from "./SiteAuditApi";

export default class SiteAuditRepository
  extends UmbRepositoryBase
  implements UmbCollectionRepository
{
  #source: SiteAuditSource;

  constructor(host: UmbControllerHost) {
    super(host);

    this.#source = new SiteAuditSource(host);
  }

  async requestCollection(
    _filter?: UmbCollectionFilterModel | undefined
  ): Promise<UmbRepositoryResponse<UmbPagedModel<any>>> {
    const resp = await this.#source.getSiteAudits();

    // The overview is paged on the server now, so the total comes back with the page rather
    // than being inferred from the length of a complete list.
    return {
      data: {
        total: resp?.data?.total ?? 0,
        items: (resp?.data?.items ?? []).map((item) => ({
          entityType: "st-siteAudit",
          ...item,
        })),
      },
    };
  }

  async get(id: number) {
    return this.#source.get(id);
  }

  async getStatus(id: number) {
    return this.#source.getStatus(id);
  }

  async getSummary(runId: number) {
    return this.#source.getSummary(runId);
  }

  async getResources(runId: number, query: ResourceQuery = {}) {
    return this.#source.getResources(runId, query);
  }

  async getResource(runId: number, resourceId: number) {
    return this.#source.getResource(runId, resourceId);
  }

  async getIssues(runId: number, skip = 0, take = 50, checkAlias?: string, severity?: string) {
    return this.#source.getIssues(runId, skip, take, checkAlias, severity);
  }

  async save(model: CreateSiteAuditRequest) {
    return this.#source.save(model);
  }

  async delete(ids: number[]) {
    return this.#source.delete(ids);
  }

  async stopAudit(id: number) {
    return this.#source.stopAudit(id);
  }

  async getConfiguration() {
    return this.#source.getConfiguration();
  }

  async getStartNode(nodeId: string) {
    return this.#source.getStartNode(nodeId);
  }
}
