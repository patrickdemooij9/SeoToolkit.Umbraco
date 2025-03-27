import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UMB_WORKSPACE_CONTEXT,
  UmbRoutableWorkspaceContext,
  UmbWorkspaceContext,
  UmbWorkspaceRouteManager,
} from "@umbraco-cms/backoffice/workspace";
import SiteAuditDetailWorkspace from "./SiteAuditDetailWorkspace.element";
import { SiteAuditDetailViewModel } from "../api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import SiteAuditRepository from "../dataAccess/SiteAuditRepository";

export default class SiteAuditDetailContext
  extends UmbContextBase<SiteAuditDetailContext>
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  #repository: SiteAuditRepository;

  routes = new UmbWorkspaceRouteManager(this);
  workspaceAlias = "seoToolkit.siteAudit.detail";
  runHeartbeat = false;

  #model = new UmbObjectState<SiteAuditDetailViewModel>({
    id: 0,
    totalPagesFound: 0,
    progress: 0,
  });
  public readonly model = this.#model.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());
    this.provideContext(ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT, this);

    this.#repository = new SiteAuditRepository(host);

    this.routes.setRoutes([
      {
        path: "detail/:unique",
        component: SiteAuditDetailWorkspace,
        setup: (_component, info) => {
          this.runHeartbeat = true;
          this.heartbeat(info.match.params.unique);
        },
      },
    ]);
  }

  heartbeat(unique: string) {
    setTimeout(() => {
      if (!this.runHeartbeat) {
        return;
      }
      this.heartbeat(unique);
    }, 1000);
    this.loadData(unique);
  }

  loadData(unique: string) {
    this.#repository.get(Number.parseInt(unique)).then((res) => {
      this.#model.update(res.data!);
      if (res.data?.status === 'Finished'){
        this.runHeartbeat = false;
      }
    });
  }

  deleteAudit() {
    this.#repository.delete([this.#model.getValue().id]).then(() => {
      location.href =
        "/umbraco/section/SeoToolkit/workspace/seoToolkit-siteAudit/overview";
    });
  }

  stopAudit() {
    this.#repository.stopAudit(this.#model.getValue().id);
  }

  override destroy() {
    this.runHeartbeat = false;
  }

  getEntityType(): string {
    return "st-siteAudit";
  }
}

export const ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT =
  new UmbContextToken<SiteAuditDetailContext>("siteAuditDetailContext");
