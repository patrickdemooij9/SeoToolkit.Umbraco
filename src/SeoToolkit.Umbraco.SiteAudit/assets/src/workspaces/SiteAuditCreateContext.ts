import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import {
  UMB_WORKSPACE_CONTEXT,
  UmbRoutableWorkspaceContext,
  UmbWorkspaceContext,
  UmbWorkspaceRouteManager,
} from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import SiteAuditCreateWorkspace from "./SiteAuditCreateWorkspace.element";
import { CreateAuditPostModel, SiteAuditCreateConfigViewModel } from "../api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import SiteAuditRepository from "../dataAccess/SiteAuditRepository";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export default class SiteAuditCreateContext
  extends UmbContextBase<SiteAuditCreateContext>
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  #repository: SiteAuditRepository;

  routes = new UmbWorkspaceRouteManager(this);
  workspaceAlias = "seoToolkit.siteAudit.create";

  #model = new UmbObjectState<CreateAuditPostModel>({
    selectedNodeId: "",
    startAudit: false,
    maxPagesToCrawl: 10,
    delayBetweenRequests: 1,
  });
  public readonly model = this.#model.asObservable();

  #config = new UmbObjectState<SiteAuditCreateConfigViewModel>({
    minimumDelayBetweenRequest: 1000,
    allowMinimumDelayBetweenRequestSetting: false,
  });
  public readonly config = this.#config.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());
    this.provideContext(ST_SITEAUDIT_CREATE_TOKEN_CONTEXT, this);

    this.#repository = new SiteAuditRepository(host);
    this.#repository.getConfiguration().then((resp) => {
      this.#model.update({
        delayBetweenRequests: resp.data!.minimumDelayBetweenRequest,
      });
      this.#config.update(resp.data!);
    });

    this.routes.setRoutes([
      {
        path: "create",
        component: SiteAuditCreateWorkspace,
      },
    ]);
  }

  update(model: Partial<CreateAuditPostModel>) {
    this.#model.update(model);
  }

  async save(start: boolean) {
    this.update({
      startAudit: start,
      checks: this.#model.value.checks ?? []
    });
    const model = this.#model.getValue();
    const response = await this.#repository.save(model);
    if (response.error){
      return;
    }
    const auditId = response.data;

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      instance.peek("positive", {
        data: {
          headline: "Created",
          message: "Site audit successfully created!",
        },
      });
    });

    //TODO: Find a way to SPA to another page
    location.href = `/umbraco/section/SeoToolkit/workspace/st-siteAudit/detail/${auditId}`;
  }

  getEntityType(): string {
    return "st-siteAudit";
  }
}

export const ST_SITEAUDIT_CREATE_TOKEN_CONTEXT =
  new UmbContextToken<SiteAuditCreateContext>("siteAuditCreateContext");
