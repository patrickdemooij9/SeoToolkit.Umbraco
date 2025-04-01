import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import {
  UMB_WORKSPACE_CONTEXT,
  UmbWorkspaceContext,
} from "@umbraco-cms/backoffice/workspace";
import { SEOTOOLKIT_NOTFOUND_ENTITY } from "../NotFoundConstants";
import { NotFoundRepository } from "../dataAccess/NotFoundRepository";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbStringState } from "@umbraco-cms/backoffice/observable-api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export default class NotFoundModuleWorkspaceContext
  extends UmbControllerBase
  implements UmbWorkspaceContext
{
  workspaceAlias = "seoToolkit.module.workspace.notFound";
  #repository: NotFoundRepository;

  #content = new UmbStringState("");
  public readonly content = this.#content.asObservable();

  constructor(host: UmbControllerHost) {
    super(host);

    this.provideContext(ST_NOTFOUND_MODULE_TOKEN_CONTEXT, this);
    this.provideContext(UMB_WORKSPACE_CONTEXT, this);

    this.#repository = new NotFoundRepository(this);

    this.#repository.get().then((resp) => {
      if (resp.data === "-1") {
        return;
      }
      this.#content.setValue(resp.data!);
    });
  }

  public update(data: string) {
    this.#content.setValue(data);
  }

  public async save() {
    const result = await this.#repository.save(this.#content.value);

    if (result.error) {
      return;
    }

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      instance.peek("positive", {
        data: {
          headline: "Saved",
          message: "Not found page successfully saved!",
        },
      });
    });
  }

  getEntityType(): string {
    return SEOTOOLKIT_NOTFOUND_ENTITY;
  }
}

export const ST_NOTFOUND_MODULE_TOKEN_CONTEXT =
  new UmbContextToken<NotFoundModuleWorkspaceContext>("notFoundModuleContext");
