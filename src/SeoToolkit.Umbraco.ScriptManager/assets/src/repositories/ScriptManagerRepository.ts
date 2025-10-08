import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { ScriptManagerSource } from "../sources/ScriptManagerSource";
import { ScriptDetailViewModel } from "../api";
import {
  UmbPagedModel,
  UmbRepositoryBase,
  UmbRepositoryResponse,
} from "@umbraco-cms/backoffice/repository";
import {
  UmbCollectionFilterModel,
  UmbCollectionRepository,
} from "@umbraco-cms/backoffice/collection";

export default class ScriptManagerRepository
  extends UmbRepositoryBase
  implements UmbCollectionRepository
{
  #scriptManagerSource: ScriptManagerSource;

  constructor(host: UmbControllerHost) {
    super(host);

    this.#scriptManagerSource = new ScriptManagerSource(host);
  }

  async requestCollection(
    _filter?: UmbCollectionFilterModel | undefined
  ): Promise<UmbRepositoryResponse<UmbPagedModel<any>>> {
    let domainId: number | undefined = undefined;

    const lastSegment = window.location.href.split("/").pop();
    if (lastSegment && lastSegment.includes("~")) {
      const parts = lastSegment.split("~");
      if (parts.length === 2) {
        domainId = Number.parseInt(parts[1]);
      }
    }

    const resp = await this.getScripts(domainId);

    const result: UmbRepositoryResponse<UmbPagedModel<any>> = {
      data: {
        total: resp.data!.length,
        items: resp.data!.map((item) => ({
          entityType: "st-script",
          ...item,
        })),
      },
    };
    return result;
  }

  async getScript(id: number) {
    return this.#scriptManagerSource.getScript(id);
  }

  async getScripts(domainId?: number) {
    return this.#scriptManagerSource.getScripts(domainId);
  }

  async saveScript(model: ScriptDetailViewModel) {
    return this.#scriptManagerSource.saveScript(model);
  }

  async deleteScripts(ids: number[]) {
    return this.#scriptManagerSource.deleteScripts(ids);
  }

  async getScriptDefinitions() {
    return this.#scriptManagerSource.getScriptDefinitions();
  }
}
