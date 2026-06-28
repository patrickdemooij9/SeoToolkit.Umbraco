import {
  UMB_WORKSPACE_CONTEXT,
  UmbRoutableWorkspaceContext,
  UmbWorkspaceContext,
  UmbWorkspaceRouteManager,
} from "@umbraco-cms/backoffice/workspace";
import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { SEOTOOLKIT_ROBOTSTXT_ENTITY } from "../Constants";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbArrayState,
  UmbStringState,
} from "@umbraco-cms/backoffice/observable-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { ValidationError } from "../types/ValidationError";
import { RobotsTxtRepository } from "../dataAccess/RobotsTxtRepository";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import SeoToolkitRobotsTxtModuleElement from "../workspaces/RobotsTxtModuleWorkspace.element";

export default class RobotsTxtModuleContext
  extends UmbControllerBase
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  workspaceAlias = "seoToolkit.module.workspace.robotsTxt";
  #repository?: RobotsTxtRepository;

  routes = new UmbWorkspaceRouteManager(this);

  #domainId : string | undefined = undefined;

  #content = new UmbStringState("");
  public readonly content = this.#content.asObservable();

  #validationErrors = new UmbArrayState<ValidationError>(
    [],
    (item) => item.error
  );
  public readonly validationErrors = this.#validationErrors.asObservable();

  constructor(host: UmbControllerHost) {
    super(host);

    this.provideContext(ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT, this);
    this.provideContext(UMB_WORKSPACE_CONTEXT, this);

    this.routes.setRoutes([
      {
        path: "edit/:unique",
        component: SeoToolkitRobotsTxtModuleElement,
        setup: (_component, info) => {
            this.#domainId = undefined;
            // This is a bit ugly, but the tree system is so difficult to understand
            if (info.match.params.unique.includes('~')){
                const parts = info.match.params.unique.split('~');
                if(parts.length === 2){
                    this.#domainId = parts[1];
                }
            }
            this.load();
        },
      },
    ]);

    this.#repository = new RobotsTxtRepository(host);
  }

  setContent(content: string) {
    this.#content.setValue(content);
  }

  async submit(skipValidation: boolean) {
    this.#validationErrors.setValue([]);
    const response = await this.#repository?.saveContent(
      this.#content.value,
      this.#domainId,
      skipValidation
    );

    if (response?.data?.errors) {
      this.#validationErrors.setValue(
        response.data.errors.map<ValidationError>((err) => ({
          lineNumber: err.lineNumber as number,
          error: err.error!,
        }))
      );

      this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (manager) => {
        manager?.open(this._host, "seoToolkit.modal.robotstxt.validation", {});
      });
    } else {
      this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
        instance?.peek("positive", {
          data: {
            headline: "Saved",
            message: "Robots.txt successfully saved!",
          },
        });
      });
    }
  }

  getEntityType(): string {
    return SEOTOOLKIT_ROBOTSTXT_ENTITY;
  }

  load() {
    this.#repository!.getContent(this.#domainId).then((result) => {
      const value = Object.keys(result.data!).length === 0 ? "" : result.data!;
      this.#content.setValue(value);
    });
  }
}

export const ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT =
  new UmbContextToken<RobotsTxtModuleContext>("robotsTxtModuleContext");
