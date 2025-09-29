import {
  UmbCollectionFilterModel,
  UmbCollectionRepository,
} from "@umbraco-cms/backoffice/collection";
import {
  UmbPagedModel,
  UmbRepositoryBase,
  UmbRepositoryResponse,
} from "@umbraco-cms/backoffice/repository";
import { RedirectSource } from "./RedirectSource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { Redirect } from "../models/Redirect";
import { SaveRedirectPostModel, UpdateStatusCodesRedirectPostModel } from "../api";
import { RedirectOverviewItem } from "../models/RedirectOverviewItem";

export default class RedirectRepository
  extends UmbRepositoryBase
  implements UmbCollectionRepository
{
  #source: RedirectSource;

  constructor(host: UmbControllerHost) {
    super(host);

    this.#source = new RedirectSource(host);
  }

  async requestCollection(
    filter?: UmbCollectionFilterModel | undefined
  ): Promise<UmbRepositoryResponse<UmbPagedModel<RedirectOverviewItem>>> {
    const pageNumber = !filter ? 1 : filter.skip! / filter.take! + 1;
    const data = await this.#source.getRedirects(
      pageNumber,
      filter?.take ?? 10
    );
    const result: UmbRepositoryResponse<UmbPagedModel<RedirectOverviewItem>> = {
      data: {
        total: data.data!.total,
        items: data.data!.items.map((item) => ({
          unique: item.id.toString(),
          entityType: "st-redirect",
          ...item,
        })),
      },
    };
    return result;
  }

  async get(unique: number): Promise<Redirect> {
    const { data } = (await this.#source.get(unique));

    return {
      unique: data.id.toString(),
      entityType: "st-redirect",
      ...data,
    };
  }

  async save(redirect: SaveRedirectPostModel) {
    await this.#source.save(redirect);
  }

  async delete(ids: number[]) {
    await this.#source.delete(ids);
  }

  async getDomains() {
    return await this.#source.getDomains();
  }

  async verifyImport(
    fileExtension: string,
    tempFileId: string,
    domain?: number
  ) {
    return this.#source.verifyImport(fileExtension, tempFileId, domain);
  }

  async submitImport() {
    return this.#source.submitImport();
  }

  async updateStatusCodes(model: UpdateStatusCodesRedirectPostModel){
    return this.#source.updateStatusCodes(model);
  }
}
