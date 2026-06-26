import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import {
  BackofficeSeoToolkitRedirects,
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
      BackofficeSeoToolkitRedirects.getUmbracoSeoToolkitRedirectsRedirects({
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

  async get(id: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.getUmbracoSeoToolkitRedirectsRedirect({
        query: {
          id: id,
        },
      })
    );
  }

  async save(redirect: SaveRedirectPostModel) {
    await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.postUmbracoSeoToolkitRedirectsRedirect({
        body: redirect,
      })
    );
  }

  async delete(ids: string[]) {
    await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.deleteUmbracoSeoToolkitRedirectsRedirect({
        body: {
          ids: ids,
        },
      })
    );
  }

  async getDomains() {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.getUmbracoSeoToolkitRedirectsDomains()
    );
  }

  async verifyImport(
    fileExtension: number,
    tempFileId: string,
    domain?: number
  ) {
    return tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.postUmbracoSeoToolkitRedirectsValidate({
        query: {
          fileExtension: fileExtension as ImportRedirectsFileExtension,
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
      BackofficeSeoToolkitRedirects.postUmbracoSeoToolkitRedirectsImport()
    );
  }

  async export(){
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.getUmbracoSeoToolkitRedirectsExport({
        parseAs: "blob"
      })
    )
  }

  async updateStatusCodes(model: UpdateStatusCodesRedirectPostModel) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkitRedirects.postUmbracoSeoToolkitRedirectsUpdateStatusCodes(
        {
          body: model,
        }
      )
    );
  }
}
