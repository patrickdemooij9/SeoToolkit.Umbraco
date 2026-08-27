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
import { UmbArrayState, UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import SiteAuditRepository from "../dataAccess/SiteAuditRepository";
import {
  ResourceQuery,
  SiteAuditApi,
  SiteAuditCategorySummary,
  SiteAuditIssue,
  SiteAuditResource,
  SiteAuditResourceDetail,
  SiteAuditRunDetail,
} from "../dataAccess/SiteAuditApi";

/** Statuses for which the run is still expected to progress on the server. */
const ACTIVE_STATUSES = ["Running", "Scheduled"];

/** How often progress is refreshed while a crawl is going. */
const POLL_INTERVAL_MS = 2000;

export const RESOURCE_PAGE_SIZE = 25;

export default class SiteAuditDetailContext
  extends UmbContextBase
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  #repository: SiteAuditRepository;
  #unique?: number;

  routes = new UmbWorkspaceRouteManager(this);
  workspaceAlias = "seoToolkit.siteAudit.detail";
  runHeartbeat = false;

  #model = new UmbObjectState<SiteAuditRunDetail | undefined>(undefined);
  public readonly model = this.#model.asObservable();

  #categories = new UmbArrayState<SiteAuditCategorySummary>([], (item) => item.category);
  public readonly categories = this.#categories.asObservable();

  #resources = new UmbArrayState<SiteAuditResource>([], (item) => item.id);
  public readonly resources = this.#resources.asObservable();

  #issues = new UmbArrayState<SiteAuditIssue>([], (item) => item.id);
  public readonly issues = this.#issues.asObservable();

  #resourceTotal = new UmbObjectState<number>(0);
  public readonly resourceTotal = this.#resourceTotal.asObservable();

  #issueTotal = new UmbObjectState<number>(0);
  public readonly issueTotal = this.#issueTotal.asObservable();

  #selectedResource = new UmbObjectState<SiteAuditResourceDetail | undefined>(undefined);
  public readonly selectedResource = this.#selectedResource.asObservable();

  #loadingResource = new UmbObjectState<number | undefined>(undefined);
  public readonly loadingResource = this.#loadingResource.asObservable();

  #resourceQuery: ResourceQuery = { skip: 0, take: RESOURCE_PAGE_SIZE };

  constructor(host: UmbControllerHost) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());
    this.provideContext(ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT, this);

    this.#repository = new SiteAuditRepository(host);

    this.routes.setRoutes([
      {
        path: "detail/:unique",
        component: SiteAuditDetailWorkspace,
        setup: (_component, info) => {
          this.#unique = Number.parseInt(info.match.params.unique);
          this.runHeartbeat = true;
          this.loadAll();
          this.heartbeat();
        },
      },
    ]);
  }

  get unique() {
    return this.#unique;
  }

  get exportUrl() {
    return this.#unique ? SiteAuditApi.exportUrl(this.#unique) : undefined;
  }

  /**
   * Refreshes progress only. Deliberately does not reload results: while a crawl is going the
   * only thing changing on screen is the progress bar, and re-fetching every page of results
   * once a second is what made watching a large crawl expensive.
   */
  heartbeat() {
    setTimeout(() => {
      if (!this.runHeartbeat) return;
      this.heartbeat();
    }, POLL_INTERVAL_MS);

    this.loadStatus();
  }

  private loadStatus() {
    if (!this.#unique) return;

    this.#repository
      .getStatus(this.#unique)
      .then((res) => {
        if (!res.data) return;

        const current = this.#model.getValue();
        this.#model.update({ ...(current ?? {}), ...res.data } as SiteAuditRunDetail);

        if (!ACTIVE_STATUSES.includes(res.data.status)) {
          const wasRunning = this.runHeartbeat;
          this.runHeartbeat = false;

          // The results only become worth reading once the crawl has stopped moving.
          if (wasRunning) this.loadResults();
        }
      })
      .catch(() => {
        // The run is gone or unreachable; there is nothing left to poll for.
        this.runHeartbeat = false;
      });
  }

  async loadAll() {
    if (!this.#unique) return;

    const detail = await this.#repository.get(this.#unique);
    this.#model.update(detail.data);

    await this.loadResults();
  }

  async loadResults() {
    await Promise.all([this.loadSummary(), this.loadResources(), this.loadIssues()]);
  }

  async loadSummary() {
    if (!this.#unique) return;

    const res = await this.#repository.getSummary(this.#unique);
    this.#categories.setValue(res.data?.categories ?? []);
  }

  /** Paging, sorting and filtering all happen on the server. */
  async loadResources(query?: Partial<ResourceQuery>) {
    if (!this.#unique) return;

    this.#resourceQuery = { ...this.#resourceQuery, ...query };

    // The open row belongs to the page that is being replaced.
    this.clearResource();

    const res = await this.#repository.getResources(this.#unique, this.#resourceQuery);
    this.#resources.setValue(res.data?.items ?? []);
    this.#resourceTotal.setValue(res.data?.total ?? 0);
  }

  async loadIssues(checkAlias?: string) {
    if (!this.#unique) return;

    const res = await this.#repository.getIssues(this.#unique, 0, 100, checkAlias);
    this.#issues.setValue(res.data?.items ?? []);
    this.#issueTotal.setValue(res.data?.total ?? 0);
  }

  get resourceQuery() {
    return this.#resourceQuery;
  }

  /**
   * Loads one page with the issues found on it. The list endpoint only carries counts, so the
   * findings themselves are fetched when a page is actually opened rather than for every row.
   */
  async selectResource(resourceId: number) {
    if (!this.#unique) return;

    // Toggling the open row shut - no request needed for that.
    if (this.#selectedResource.getValue()?.id === resourceId) {
      this.clearResource();
      return;
    }

    this.#selectedResource.setValue(undefined);
    this.#loadingResource.setValue(resourceId);

    try {
      const res = await this.#repository.getResource(this.#unique, resourceId);
      // Another row may have been opened while this was in flight; that one wins.
      if (this.#loadingResource.getValue() !== resourceId) return;

      this.#selectedResource.setValue(res.data ?? undefined);
    } finally {
      if (this.#loadingResource.getValue() === resourceId)
        this.#loadingResource.setValue(undefined);
    }
  }

  clearResource() {
    this.#selectedResource.setValue(undefined);
    this.#loadingResource.setValue(undefined);
  }

  deleteAudit() {
    if (!this.#unique) return;

    this.#repository.delete([this.#unique]).then(() => {
      location.href =
        "/umbraco/section/SeoToolkit/workspace/seoToolkit-siteAudit/overview";
    });
  }

  stopAudit() {
    if (!this.#unique) return;
    this.#repository.stopAudit(this.#unique);
  }

  override destroy() {
    super.destroy();
    this.runHeartbeat = false;
  }

  getEntityType(): string {
    return "st-siteAudit";
  }
}

export const ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT =
  new UmbContextToken<SiteAuditDetailContext>("siteAuditDetailContext");
