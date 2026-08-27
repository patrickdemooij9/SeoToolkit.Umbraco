import { client } from "../api";

/**
 * Typed calls for the site audit endpoints.
 *
 * Hand written rather than generated, and deliberately so for now: `npm run generate` needs the
 * Site project running, so regenerating is a local step. It uses the same `client` the generated
 * sdk uses - configured with the backoffice bearer token in `index.ts` - so authentication,
 * base url and interceptors all behave identically. Once the client is regenerated these can be
 * swapped for the generated equivalents without touching anything above this layer.
 */

const base = "/umbraco/seoToolkitSiteAudit";

export type SiteAuditStatus =
  | "Created"
  | "Scheduled"
  | "Running"
  | "Finished"
  | "Error"
  | "Stopped"
  | "Interrupted";

export interface SiteAuditPaged<T> {
  total: number;
  items: T[];
}

export interface SiteAuditRunOverview {
  id: number;
  key: string;
  name: string;
  status: SiteAuditStatus;
  createdDate: string;
  totalCrawled: number;
  errorCount: number;
  warningCount: number;
  score?: number | null;
}

export interface SiteAuditRunStatus {
  id: number;
  status: SiteAuditStatus;
  isFinished: boolean;
  progress: number;
  totalCrawled: number;
  totalDiscovered: number;
  criticalCount: number;
  errorCount: number;
  warningCount: number;
  noticeCount: number;
  score?: number | null;
}

export interface SiteAuditRunDetail extends SiteAuditRunStatus {
  key: string;
  name: string;
  startingUrl: string;
  baseUrl?: string | null;
  createdDate: string;
  startedDate?: string | null;
  finishedDate?: string | null;
  maxPages?: number | null;
  startNodeKey?: string | null;
  culture?: string | null;
}

export interface SiteAuditCheckSummary {
  alias: string;
  name: string;
  description?: string | null;
  category: string;
  severity: string;
  didRun: boolean;
  applicableCount: number;
  failedCount: number;
  issueCount: number;
  weight: number;
}

export interface SiteAuditCategorySummary {
  category: string;
  failedCount: number;
  issueCount: number;
  checks: SiteAuditCheckSummary[];
}

export interface SiteAuditSummary {
  categories: SiteAuditCategorySummary[];
}

export interface SiteAuditResource {
  id: number;
  url: string;
  path: string;
  statusCode: number;
  kind: string;
  depth: number;
  isInternal: boolean;
  responseTimeMs: number;
  sizeBytes: number;
  redirectCount: number;
  title?: string | null;
  metaDescription?: string | null;
  h1?: string | null;
  wordCount: number;
  isIndexable: boolean;
  indexabilityReasons: string[];
  failure?: string | null;
  umbracoContentKey?: string | null;
  issueCount: number;
  errorCount: number;
  warningCount: number;
}

export interface SiteAuditIssue {
  id: number;
  checkAlias: string;
  checkName: string;
  category: string;
  variant?: string | null;
  severity: string;
  isError: boolean;
  isWarning: boolean;
  message: string;
  url?: string | null;
  evidence?: string | null;
  resourceId?: number | null;
  data: Record<string, string>;
}

export interface SiteAuditResourceDetail extends SiteAuditResource {
  finalUrl?: string | null;
  contentType?: string | null;
  culture?: string | null;
  issues: SiteAuditIssue[];
}

export interface SiteAuditCheckOption {
  key: string;
  name: string;
  description?: string | null;
  type: string;
  defaultValue?: unknown;
  minimum?: number | null;
  maximum?: number | null;
}

export interface SiteAuditCheckCatalogueEntry {
  alias: string;
  name: string;
  description?: string | null;
  category: string;
  defaultSeverity: string;
  weight: number;
  supportsSinglePage: boolean;
  documentationUrl?: string | null;
  providerName?: string | null;
  requiresFeature?: string | null;
  isAvailable: boolean;
  options: SiteAuditCheckOption[];
}

export interface SiteAuditCreateOptions {
  checks: SiteAuditCheckCatalogueEntry[];
  allowMinimumDelayBetweenRequestSetting: boolean;
  minimumDelayBetweenRequest: number;
}

/** One language a start node is published in, with the url the crawl would use for it. */
export interface SiteAuditStartNodeCulture {
  isoCode: string;
  name: string;
  url: string;
}

export interface SiteAuditStartNode {
  key: string;
  name: string;
  variesByCulture: boolean;
  url?: string | null;
  cultures: SiteAuditStartNodeCulture[];
}

/**
 * A crawl starts from either a node or a url, never both. The url form exists because a
 * decoupled frontend can route however it likes, so there may be no node whose url resembles
 * the page that is actually served.
 */
export interface CreateSiteAuditRequest {
  name: string;
  selectedNodeId?: string | null;
  culture?: string | null;
  startingUrl?: string | null;
  checks: string[];
  startAudit: boolean;
  maxPagesToCrawl: number;
  delayBetweenRequests: number;
}

export interface SiteAuditPageCheckResult {
  url: string;
  issues: SiteAuditIssue[];
}

export interface ResourceQuery {
  skip?: number;
  take?: number;
  checkAlias?: string;
  severity?: string;
  statusCode?: number;
  statusClass?: number;
  kind?: string;
  hasIssues?: boolean;
  search?: string;
  sort?: string;
  descending?: boolean;
}

export const SiteAuditApi = {
  getRuns: (skip = 0, take = 100) =>
    client.get<SiteAuditPaged<SiteAuditRunOverview>>({
      url: `${base}/siteAudits`,
      query: { skip, take },
    }),

  getRun: (id: number) =>
    client.get<SiteAuditRunDetail>({ url: `${base}/siteAudit`, query: { id } }),

  /** The small payload polled while a crawl is going. Reads one row, whatever the crawl size. */
  getStatus: (id: number) =>
    client.get<SiteAuditRunStatus>({ url: `${base}/siteAuditStatus`, query: { id } }),

  getSummary: (runId: number) =>
    client.get<SiteAuditSummary>({ url: `${base}/siteAuditSummary`, query: { runId } }),

  getResources: (runId: number, query: ResourceQuery = {}) =>
    client.get<SiteAuditPaged<SiteAuditResource>>({
      url: `${base}/siteAuditResources`,
      query: { runId, skip: 0, take: 50, ...query },
    }),

  getResource: (runId: number, resourceId: number) =>
    client.get<SiteAuditResourceDetail>({
      url: `${base}/siteAuditResource`,
      query: { runId, resourceId },
    }),

  getIssues: (runId: number, skip = 0, take = 50, checkAlias?: string, severity?: string) =>
    client.get<SiteAuditPaged<SiteAuditIssue>>({
      url: `${base}/siteAuditIssues`,
      query: { runId, skip, take, checkAlias, severity },
    }),

  getCreateOptions: () =>
    client.get<SiteAuditCreateOptions>({ url: `${base}/siteAuditConfiguration` }),

  /** The languages a chosen node is published in, and the url each one would crawl. */
  getStartNode: (nodeId: string) =>
    client.get<SiteAuditStartNode>({ url: `${base}/siteAuditStartNode`, query: { nodeId } }),

  create: (body: CreateSiteAuditRequest) =>
    client.post<number>({ url: `${base}/siteAudit`, body }),

  stop: (id: number) => client.post<void>({ url: `${base}/stopSiteAudit`, body: { id } }),

  remove: (ids: number[]) => client.delete<void>({ url: `${base}/siteAudit`, body: { ids } }),

  getPageChecks: () =>
    client.get<SiteAuditCheckCatalogueEntry[]>({ url: `${base}/pageChecks` }),

  runPageChecks: (contentId: string, culture?: string | null) =>
    client.post<SiteAuditPageCheckResult>({ url: `${base}/run`, body: { contentId, culture } }),

  /** Absolute url for the csv export, used as an anchor href rather than fetched. */
  exportUrl: (runId: number) => `${base}/siteAuditExport?runId=${runId}`,
};
