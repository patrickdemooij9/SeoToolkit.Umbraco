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
import { CreateAuditPostModel } from "../api";

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
    const resp = (await this.#source.getSiteAudits())!;

    const result: UmbRepositoryResponse<UmbPagedModel<any>> = {
      data: {
        total: resp.data.length,
        items: resp.data.map((item) => ({
          entityType: "st-siteAudit",
          ...item,
        })),
      },
    };
    return result;
  }

  async get(id: number){
    return this.#source.get(id);
  }

  async save(model: CreateAuditPostModel){
    return this.#source.save(model);
  }

  async delete(ids: number[]) {
    return this.#source.delete(ids);
  }

  async stopAudit(id: number){
    return this.#source.stopAudit(id);
  }

  async getConfiguration() {
    return this.#source.getConfiguration();
  }
}
