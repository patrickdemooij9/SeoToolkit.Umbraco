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
import {
  CreateSiteAuditRequest,
  SiteAuditCreateOptions,
  SiteAuditStartNode,
} from "../dataAccess/SiteAuditApi";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import SiteAuditRepository from "../dataAccess/SiteAuditRepository";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

/**
 * Where the crawl begins. A node is what an editor knows; a url is the only workable option for
 * a decoupled frontend, whose routing need not resemble the content tree at all.
 */
export type SiteAuditStartMode = "node" | "url";

export default class SiteAuditCreateContext
  extends UmbContextBase
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  #repository: SiteAuditRepository;

  routes = new UmbWorkspaceRouteManager(this);
  workspaceAlias = "seoToolkit.siteAudit.create";

  #model = new UmbObjectState<CreateSiteAuditRequest>({
    name: "",
    selectedNodeId: null,
    culture: null,
    startingUrl: null,
    checks: [],
    startAudit: false,
    maxPagesToCrawl: 10,
    delayBetweenRequests: 1,
  });
  public readonly model = this.#model.asObservable();

  #config = new UmbObjectState<SiteAuditCreateOptions>({
    checks: [],
    minimumDelayBetweenRequest: 1000,
    allowMinimumDelayBetweenRequestSetting: false,
  });
  public readonly config = this.#config.asObservable();

  #mode = new UmbObjectState<SiteAuditStartMode>("node");
  public readonly mode = this.#mode.asObservable();

  #startNode = new UmbObjectState<SiteAuditStartNode | undefined>(undefined);
  public readonly startNode = this.#startNode.asObservable();

  #loadingStartNode = new UmbObjectState<boolean>(false);
  public readonly loadingStartNode = this.#loadingStartNode.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());
    this.provideContext(ST_SITEAUDIT_CREATE_TOKEN_CONTEXT, this);

    this.#repository = new SiteAuditRepository(host);
    this.#repository.getConfiguration().then((resp) => {
      if (!resp.data) return;

      this.#config.update(resp.data);
      this.#model.update({
        delayBetweenRequests: resp.data.minimumDelayBetweenRequest,
        // Checks are selected by alias now, and only ones this installation can actually run
        // are pre-selected - a check belonging to an add-on that is not installed would
        // otherwise be requested and silently skipped.
        checks:
          resp.data.checks?.filter((check) => check.isAvailable).map((check) => check.alias) ?? [],
      });
    });

    this.routes.setRoutes([
      {
        path: "create",
        component: SiteAuditCreateWorkspace,
      },
    ]);
  }

  update(model: Partial<CreateSiteAuditRequest>) {
    this.#model.update(model);
  }

  /** Switching mode clears the other side, so only one starting point ever reaches the server. */
  setMode(mode: SiteAuditStartMode) {
    if (this.#mode.getValue() === mode) return;

    this.#mode.setValue(mode);

    if (mode === "node") {
      this.update({ startingUrl: null });
      return;
    }

    this.update({ selectedNodeId: null, culture: null });
    this.#startNode.setValue(undefined);
  }

  /**
   * Loads what the chosen node can offer. The languages come from the server because the url
   * for each one is the server's to decide - it is the same BaseUrl retarget the sitemap does,
   * so what is shown here is what will genuinely be fetched.
   */
  async selectNode(nodeId?: string | null) {
    if (this.#model.getValue().selectedNodeId === (nodeId ?? null)) return;

    this.update({ selectedNodeId: nodeId ?? null, culture: null });
    this.#startNode.setValue(undefined);

    if (!nodeId) return;

    this.#loadingStartNode.setValue(true);
    try {
      const res = await this.#repository.getStartNode(nodeId);

      // The picker may have moved on while this was in flight.
      if (this.#model.getValue().selectedNodeId !== nodeId) return;

      const node = res.data ?? undefined;
      this.#startNode.setValue(node);

      // A node published in one language needs no choice, so it is made here rather than
      // presented as a decision. One published in several is left for the user.
      if (node && !node.variesByCulture) {
        this.update({ culture: node.cultures[0]?.isoCode ?? null });
      }
    } finally {
      this.#loadingStartNode.setValue(false);
    }
  }

  /** The url the crawl will start from, as far as the form can tell. */
  get previewUrl(): string | undefined {
    if (this.#mode.getValue() === "url") return this.#model.getValue().startingUrl ?? undefined;

    const node = this.#startNode.getValue();
    if (!node) return undefined;

    if (!node.variesByCulture) return node.url ?? node.cultures[0]?.url;

    const culture = this.#model.getValue().culture;
    return node.cultures.find((it) => it.isoCode === culture)?.url;
  }

  /** Whether the form describes a crawl that can actually be started. */
  get isComplete(): boolean {
    const model = this.#model.getValue();

    if (!model.name || (model.checks?.length ?? 0) === 0) return false;

    if (this.#mode.getValue() === "url") return !!model.startingUrl?.trim();

    if (!model.selectedNodeId) return false;

    // A node in several languages has to be told which one; there is no sensible default, and
    // guessing would quietly audit a language nobody asked about.
    const node = this.#startNode.getValue();
    return !node?.variesByCulture || !!model.culture;
  }

  async save(start: boolean) {
    this.update({
      startAudit: start,
      checks: this.#model.value.checks ?? [],
    });
    const model = this.#model.getValue();
    const response = await this.#repository.save(model);
    if (response.error) {
      return;
    }
    const auditId = response;

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      instance?.peek("positive", {
        data: {
          headline: "Created",
          message: "Site audit successfully created!",
        },
      });
    });

    history.pushState(
      {},
      "",
      `/umbraco/section/SeoToolkit/workspace/st-siteAudit/detail/${auditId.data}`
    );
  }

  getEntityType(): string {
    return "st-siteAudit";
  }
}

export const ST_SITEAUDIT_CREATE_TOKEN_CONTEXT =
  new UmbContextToken<SiteAuditCreateContext>("siteAuditCreateContext");
