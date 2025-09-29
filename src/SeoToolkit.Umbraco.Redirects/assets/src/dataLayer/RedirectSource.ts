import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import {
  BackofficeSeoToolkitRedirectsService,
  GetUmbracoSeoToolkitRedirectsRedirectsResponse,
  ImportRedirectsFileExtension,
  SaveRedirectPostModel,
  UpdateStatusCodesRedirectPostModel,
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
      BackofficeSeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsRedirects({
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
      BackofficeSeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsRedirect({
        query: {
          id: id,
        },
      })
    );
  }

  async save(redirect: SaveRedirectPostModel) {
    await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsRedirect({
        body: redirect,
      })
    );
  }

  async delete(ids: number[]) {
    await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirectsService.deleteUmbracoSeoToolkitRedirectsRedirect({
        body: {
          ids: ids,
        },
      })
    );
  }

  async getDomains() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirectsService.getUmbracoSeoToolkitRedirectsDomains()
    );
  }

  async verifyImport(
    fileExtension: string,
    tempFileId: string,
    domain?: number
  ) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsValidate({
        query: {
          fileExtension: fileExtension! as ImportRedirectsFileExtension,
          domain: domain!,
          tempFileId: tempFileId!,
        },
      }),
      {
        disableNotifications: true,
      }
    );
  }

  async submitImport() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsImport()
    );
  }

  async updateStatusCodes(model: UpdateStatusCodesRedirectPostModel) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirectsService.postUmbracoSeoToolkitRedirectsUpdateStatusCodes(
        {
          body: model,
        }
      )
    );
  }
}
