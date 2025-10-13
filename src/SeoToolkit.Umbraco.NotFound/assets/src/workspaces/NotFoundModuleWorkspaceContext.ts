import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import {
  UMB_WORKSPACE_CONTEXT,
  UmbRoutableWorkspaceContext,
  UmbWorkspaceContext,
  UmbWorkspaceRouteManager,
} from "@umbraco-cms/backoffice/workspace";
import { SEOTOOLKIT_NOTFOUND_ENTITY } from "../NotFoundConstants";
import { NotFoundRepository } from "../dataAccess/NotFoundRepository";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbStringState } from "@umbraco-cms/backoffice/observable-api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import NotFoundModuleWorkspaceElement from "./NotFoundModuleWorkspace.element";

export default class NotFoundModuleWorkspaceContext
  extends UmbControllerBase
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  workspaceAlias = "seoToolkit.module.workspace.notFound";
  #repository: NotFoundRepository;

  routes = new UmbWorkspaceRouteManager(this);

  #content = new UmbStringState("");
  public readonly content = this.#content.asObservable();

  #domainId?: number;

  constructor(host: UmbControllerHost) {
    super(host);

    this.provideContext(ST_NOTFOUND_MODULE_TOKEN_CONTEXT, this);
    this.provideContext(UMB_WORKSPACE_CONTEXT, this);

    this.routes.setRoutes([
      {
        path: "edit/:unique",
        component: NotFoundModuleWorkspaceElement,
        setup: (_component, info) => {
          this.#domainId = undefined;
          // This is a bit ugly, but the tree system is so difficult to understand
          if (info.match.params.unique.includes("~")) {
            const parts = info.match.params.unique.split("~");
            if (parts.length === 2) {
              this.#domainId = Number.parseInt(parts[1]);
            }
          }
          this.load();
        },
      },
    ]);

    this.#repository = new NotFoundRepository(this);
  }

  private load() {
    this.#repository.get(this.#domainId).then((resp) => {
      if (Object.keys(resp.data).length === 0) {
        return;
      }
      this.#content.setValue(resp.data);
    });
  }

  public update(data: string) {
    this.#content.setValue(data);
  }

  public async save() {
    const result = await this.#repository.save(this.#content.value, this.#domainId);

    if (result.error) {
      return;
    }

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      instance?.peek("positive", {
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
