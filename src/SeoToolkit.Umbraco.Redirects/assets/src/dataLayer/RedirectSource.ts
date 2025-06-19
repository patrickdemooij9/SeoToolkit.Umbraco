import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import {
  GetUmbracoSeoToolkitRedirectsRedirectsResponse,
  ImportRedirectsFileExtension,
  SaveRedirectPostModel,
  SeoToolkitRedirectsService,
} from "../api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";

export class RedirectSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getRedirects(
    pageNumber: number,
    pageSize: number,
    orderBy?: string,
    orderDirection?: string,
    search?: string
  ): Promise<
    UmbDataSourceResponse<GetUmbracoSeoToolkitRedirectsRedirectsResponse>
  > {
    return await tryExecute(
      this.#host,
      SeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsRedirects({
        query: {
          pageNumber,
          pageSize,
          orderBy,
          orderDirection,
          search,
        },
      })
    );
  }

  async get(id: number) {
    return await tryExecute(
      this.#host,
      SeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsRedirect({
        query: {
          id: id,
        },
      })
    );
  }

  async save(redirect: SaveRedirectPostModel) {
    await tryExecute(
      this.#host,
      SeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsRedirect({
        body: redirect,
      })
    );
  }

  async delete(ids: number[]) {
    await tryExecute(
      this.#host,
      SeoToolkitRedirectsService.deleteUmbracoSeoToolkitRedirectsRedirect({
        body: {
          ids: ids,
        },
      })
    );
  }

  async getDomains() {
    return await tryExecute(
      this.#host,
      SeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsDomains()
    );
  }

  async verifyImport(
    fileExtension: string,
    tempFileId: string,
    domain?: number
  ) {
    return tryExecute(
      this.#host,
      SeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsValidate({
        query: {
          fileExtension: fileExtension! as ImportRedirectsFileExtension,
          domain: domain!,
          tempFileId: tempFileId!,
        },
      })
    );
  }

  async submitImport() {
    return await tryExecute(
      this.#host,
      SeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsImport()
    );
  }
}
