import {
  css,
  customElement,
  html,
  nothing,
  repeat,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { UUIPaginationEvent } from "@umbraco-cms/backoffice/external/uui";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbWorkspaceViewElement } from "@umbraco-cms/backoffice/workspace";
import {
  SiteAuditPageDetailViewModel,
  SiteAuditDetailViewModel,
} from "../../api";
import SiteAuditCheckResult from "../../models/SiteAuditCheckResultModel";
import SiteAuditDetailContext, {
  ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT,
} from "../SiteAuditDetailContext";

enum DetailTab {
  Overview = "overview",
  Issues = "issues",
  Pages = "pages",
}

enum PageFilter {
  All = "all",
  WithIssues = "issues",
  WithErrors = "errors",
  WithWarnings = "warnings",
}

interface PageItem {
  data: SiteAuditPageDetailViewModel;
  open: boolean;
  errors: number;
  warnings: number;
}

interface IssueItem {
  checkId: number;
  checkName: string;
  errorMessage: string;
  isError: boolean;
  isWarning: boolean;
  count: number;
  pages: string[];
  expanded: boolean;
}

@customElement("seotoolkit-site-audit-detail-main")
export default class SiteAuditDetailMain
  extends UmbLitElement
  implements UmbWorkspaceViewElement
{
  #context?: SiteAuditDetailContext;

  @state()
  _model?: SiteAuditDetailViewModel;

  @state()
  _activeTab: DetailTab = DetailTab.Overview;

  @state()
  _pageIndex = 0;

  @state()
  _pageFilter: PageFilter = PageFilter.All;

  @state()
  _openedPages: string[] = [];

  @state()
  _checkResults: SiteAuditCheckResult[] = [];

  @state()
  _issueItems: IssueItem[] = [];

  constructor() {
    super();

    this.consumeContext(ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT, (instance) => {
      if (!instance) {
        return;
      }
      this.#context = instance;

      this.observe(instance.model, (value) => {
        this._model = value;
        this.#updateStats();
      });
    });
  }

  #updateStats() {
    const issueMap = new Map<number, IssueItem>();
    this._checkResults = [];

    this._model?.pagesCrawled?.forEach((page) => {
      page.results?.forEach((result) => {
        if (!result?.isError && !result?.isWarning) return;

        const existingCheckResult = this._checkResults.find(
          (check) => check.id === result.checkId,
        );
        if (!existingCheckResult) {
          this._checkResults.push({
            id: result.checkId,
            count: 1,
            isError: result.isError,
            isWarning: result.isWarning,
          });
        } else {
          existingCheckResult.count++;
        }

        const checkDef = this._model?.checks?.find(
          (c) => c.id === result.checkId,
        );
        if (!issueMap.has(result.checkId)) {
          issueMap.set(result.checkId, {
            checkId: result.checkId,
            checkName: checkDef?.name ?? `Check ${result.checkId}`,
            errorMessage: checkDef?.errorMessage ?? "",
            isError: !!result.isError,
            isWarning: !!result.isWarning,
            count: 0,
            pages: [],
            expanded: false,
          });
        }
        const issueItem = issueMap.get(result.checkId)!;
        issueItem.count++;
        if (page.url && !issueItem.pages.includes(page.url)) {
          issueItem.pages.push(page.url);
        }
      });
    });

    this._issueItems = Array.from(issueMap.values()).sort((a, b) => {
      if (a.isError && !b.isError) return -1;
      if (!a.isError && b.isError) return 1;
      return b.count - a.count;
    });
  }

  #getFilteredPages(): SiteAuditPageDetailViewModel[] {
    const pages = this._model?.pagesCrawled ?? [];
    switch (this._pageFilter) {
      case PageFilter.WithErrors:
        return pages.filter((p) => p.results?.some((r) => r.isError));
      case PageFilter.WithWarnings:
        return pages.filter((p) => p.results?.some((r) => r.isWarning));
      case PageFilter.WithIssues:
        return pages.filter((p) =>
          p.results?.some((r) => r.isError || r.isWarning),
        );
      default:
        return pages;
    }
  }

  #getPagesPaged(): PageItem[] {
    return this.#getFilteredPages()
      .slice(this._pageIndex * 10, this._pageIndex * 10 + 10)
      .map<PageItem>((page) => ({
        data: page,
        open: this.#pageIsOpen(page),
        errors:
          page.results?.reduce(
            (prev, cur) => prev + (cur.isError ? 1 : 0),
            0,
          ) ?? 0,
        warnings:
          page.results?.reduce(
            (prev, cur) => prev + (cur.isWarning ? 1 : 0),
            0,
          ) ?? 0,
      }));
  }

  #getScoreColor(score: number | null | undefined): string {
    if (score == null) return "var(--uui-color-text-alt)";
    if (score >= 90) return "var(--uui-color-positive)";
    if (score >= 70) return "var(--uui-color-warning)";
    return "var(--uui-color-danger)";
  }

  #getScoreLabel(score: number | null | undefined): string {
    if (score == null) return "";
    if (score >= 90) return "Good";
    if (score >= 70) return "Needs work";
    return "Poor";
  }

  #back() {
    location.href =
      "/umbraco/section/SeoToolkit/workspace/seoToolkit-siteAudit/overview";
  }

  #openPage(page: SiteAuditPageDetailViewModel) {
    this._openedPages.push(page.url!);
    this.requestUpdate();
  }

  #closePage(page: SiteAuditPageDetailViewModel) {
    this._openedPages.splice(this._openedPages.indexOf(page.url!), 1);
    this.requestUpdate();
  }

  #pageIsOpen(page: SiteAuditPageDetailViewModel) {
    return this._openedPages.includes(page.url!);
  }

  #toggleIssue(checkId: number) {
    const item = this._issueItems.find((i) => i.checkId === checkId);
    if (item) {
      item.expanded = !item.expanded;
      this.requestUpdate();
    }
  }

  #setTab(tab: DetailTab) {
    this._activeTab = tab;
    this._pageIndex = 0;
  }

  #setPageFilter(filter: PageFilter) {
    this._pageFilter = filter;
    this._pageIndex = 0;
  }

  handlePageUpdate(event: UUIPaginationEvent) {
    this._pageIndex = event.target.current - 1;
  }

  handleDeleteAudit() {
    this.#context?.deleteAudit();
  }

  handleStopAudit() {
    this.#context?.stopAudit();
  }

  #renderHeader() {
    const score = this._model?.score;
    const errors = this._model?.totalErrors ?? 0;
    const warnings = this._model?.totalWarnings ?? 0;
    const isRunning = this._model?.status === "Running";
    const isScheduled = this._model?.status === "Scheduled";
    const pageCount = this._model?.pagesCrawled?.length ?? 0;
    const totalFound = this._model?.totalPagesFound ?? 0;
    const progress = this._model?.progress ?? 0;

    return html`
      <uui-box class="header-box">
        <div class="header-content">
          <div class="score-section">
            <div
              class="score-circle"
              style="--score-color: ${this.#getScoreColor(score)}"
            >
              <span class="score-value">${score != null ? score : "—"}</span>
              ${when(
                score != null,
                () =>
                  html`<span class="score-label"
                    >${this.#getScoreLabel(score)}</span
                  >`,
                () => html`<span class="score-label">Score</span>`,
              )}
            </div>
          </div>

          <div class="header-info">
            <h2 class="audit-name">${this._model?.name}</h2>
            <div class="status-row">
              <span
                class="status-badge status-${this._model?.status?.toLowerCase()}"
                >${this._model?.status}</span
              >
            </div>

            <div class="stats-row">
              <div class="stat">
                <uui-icon
                  name="icon-delete"
                  style="color: var(--uui-color-danger);"
                ></uui-icon>
                <span class="stat-value">${errors}</span>
                <span class="stat-label">Errors</span>
              </div>
              <div class="stat">
                <uui-icon
                  name="icon-info"
                  style="color: var(--uui-color-warning);"
                ></uui-icon>
                <span class="stat-value">${warnings}</span>
                <span class="stat-label">Warnings</span>
              </div>
              <div class="stat">
                <uui-icon name="icon-document"></uui-icon>
                <span class="stat-value"
                  >${pageCount}${isRunning
                    ? html`<span class="pages-total">/${totalFound}</span>`
                    : nothing}</span
                >
                <span class="stat-label">Pages crawled</span>
              </div>
            </div>

            ${when(
              isRunning,
              () => html`
                <div class="progress-section">
                  <div class="progress-label-row">
                    <span>Crawling in progress...</span>
                    <span>${Math.round(progress)}%</span>
                  </div>
                  <uui-progress-bar .progress=${progress}></uui-progress-bar>
                </div>
              `,
            )}
            ${when(
              isScheduled,
              () => html`
                <div class="progress-section">
                  <div class="progress-label-row">
                    <span>Audit scheduled — will start within a minute</span>
                  </div>
                  <uui-progress-bar .progress=${0}></uui-progress-bar>
                </div>
              `,
            )}
          </div>
        </div>
      </uui-box>
    `;
  }

  #renderTabBar() {
    const errors = this._model?.totalErrors ?? 0;
    const warnings = this._model?.totalWarnings ?? 0;
    const issueCount = errors + warnings;
    const pageCount = this._model?.pagesCrawled?.length ?? 0;

    return html`
      <div class="tab-bar">
        <button
          class="tab-btn ${this._activeTab === DetailTab.Overview
            ? "active"
            : ""}"
          @click="${() => this.#setTab(DetailTab.Overview)}"
        >
          Overview
        </button>
        <button
          class="tab-btn ${this._activeTab === DetailTab.Issues
            ? "active"
            : ""}"
          @click="${() => this.#setTab(DetailTab.Issues)}"
        >
          Issues
          ${when(
            issueCount > 0,
            () =>
              html`<span class="tab-count tab-count--issue"
                >${issueCount}</span
              >`,
          )}
        </button>
        <button
          class="tab-btn ${this._activeTab === DetailTab.Pages ? "active" : ""}"
          @click="${() => this.#setTab(DetailTab.Pages)}"
        >
          Pages
          <span class="tab-count">${pageCount}</span>
        </button>
      </div>
    `;
  }

  #renderOverview() {
    return html`
      <uui-box headline="Issues by check type">
        ${when(
          this._issueItems.length === 0,
          () =>
            html`<p class="empty-state">No issues found. Great job! 🎉</p>`,
          () => html`
            <div class="overview-issues">
              ${repeat(
                this._issueItems,
                (item) => item.checkId,
                (item) => html`
                  <div class="overview-issue-item">
                    ${when(
                      item.isError,
                      () =>
                        html`<uui-icon
                          name="icon-delete"
                          style="color: var(--uui-color-danger);"
                        ></uui-icon>`,
                    )}
                    ${when(
                      item.isWarning && !item.isError,
                      () =>
                        html`<uui-icon
                          name="icon-info"
                          style="color: var(--uui-color-warning);"
                        ></uui-icon>`,
                    )}
                    <span class="overview-issue-name"
                      >${item.errorMessage || item.checkName}</span
                    >
                    <span class="overview-issue-meta">
                      ${item.count}
                      occurrence${item.count !== 1 ? "s" : ""} on
                      ${item.pages.length}
                      page${item.pages.length !== 1 ? "s" : ""}
                    </span>
                  </div>
                `,
              )}
            </div>
          `,
        )}
      </uui-box>
    `;
  }

  #renderIssues() {
    return html`
      <uui-box headline="All issues across the website">
        ${when(
          this._issueItems.length === 0,
          () =>
            html`<p class="empty-state">
              No issues found across the website.
            </p>`,
          () => html`
            <div class="issues-list">
              ${repeat(
                this._issueItems,
                (item) => item.checkId,
                (item) => html`
                  <div class="issue-item">
                    <div
                      class="issue-header"
                      @click="${() => this.#toggleIssue(item.checkId)}"
                    >
                      <div class="issue-header-left">
                        ${when(
                          item.isError,
                          () =>
                            html`<uui-icon
                              name="icon-delete"
                              style="color: var(--uui-color-danger);"
                            ></uui-icon>`,
                        )}
                        ${when(
                          item.isWarning && !item.isError,
                          () =>
                            html`<uui-icon
                              name="icon-info"
                              style="color: var(--uui-color-warning);"
                            ></uui-icon>`,
                        )}
                        <span class="issue-check-name"
                          >${item.errorMessage || item.checkName}</span
                        >
                      </div>
                      <div class="issue-header-right">
                        <span class="issue-page-count"
                          >${item.pages.length}
                          page${item.pages.length !== 1 ? "s" : ""} affected
                          (${item.count}
                          occurrence${item.count !== 1 ? "s" : ""})</span
                        >
                        <span class="issue-toggle"
                          >${item.expanded ? "▲" : "▼"}</span
                        >
                      </div>
                    </div>
                    ${when(
                      item.expanded,
                      () => html`
                        <div class="issue-pages">
                          ${repeat(
                            item.pages,
                            (url) => url,
                            (url) => html`
                              <div class="issue-page-url">
                                <uui-icon name="icon-document"></uui-icon>
                                <span>${url}</span>
                              </div>
                            `,
                          )}
                        </div>
                      `,
                    )}
                  </div>
                `,
              )}
            </div>
          `,
        )}
      </uui-box>
    `;
  }

  #renderPages() {
    const filteredPages = this.#getFilteredPages();
    const totalPages = this._model?.pagesCrawled?.length ?? 0;
    const issuePages = (this._model?.pagesCrawled ?? []).filter((p) =>
      p.results?.some((r) => r.isError || r.isWarning),
    ).length;
    const errorPages = (this._model?.pagesCrawled ?? []).filter((p) =>
      p.results?.some((r) => r.isError),
    ).length;
    const warningPages = (this._model?.pagesCrawled ?? []).filter((p) =>
      p.results?.some((r) => r.isWarning),
    ).length;

    return html`
      <uui-box headline="Crawled pages">
        <div slot="header" class="pages-filter-bar">
          <button
            class="filter-btn ${this._pageFilter === PageFilter.All
              ? "active"
              : ""}"
            @click="${() => this.#setPageFilter(PageFilter.All)}"
          >
            All (${totalPages})
          </button>
          <button
            class="filter-btn ${this._pageFilter === PageFilter.WithIssues
              ? "active"
              : ""}"
            @click="${() => this.#setPageFilter(PageFilter.WithIssues)}"
          >
            With issues (${issuePages})
          </button>
          <button
            class="filter-btn ${this._pageFilter === PageFilter.WithErrors
              ? "active"
              : ""}"
            @click="${() => this.#setPageFilter(PageFilter.WithErrors)}"
          >
            Errors only (${errorPages})
          </button>
          <button
            class="filter-btn ${this._pageFilter === PageFilter.WithWarnings
              ? "active"
              : ""}"
            @click="${() => this.#setPageFilter(PageFilter.WithWarnings)}"
          >
            Warnings only (${warningPages})
          </button>
        </div>

        <div class="pages-list">
          ${when(
            filteredPages.length === 0,
            () =>
              html`<p class="empty-state">
                No pages match the current filter.
              </p>`,
          )}
          ${repeat(
            this.#getPagesPaged(),
            (item) => item.data.url,
            (item) => html`
              <div
                class="page-item ${item.errors > 0
                  ? "has-errors"
                  : item.warnings > 0
                    ? "has-warnings"
                    : "has-no-issues"}"
              >
                <div
                  class="page-main"
                  @click="${() =>
                    item.open
                      ? this.#closePage(item.data)
                      : this.#openPage(item.data)}"
                >
                  <div class="page-url">
                    <span>${item.data.url}</span>
                  </div>
                  <div class="page-meta">
                    <span
                      class="status-code ${item.data.statusCode >= 200 &&
                      item.data.statusCode < 300
                        ? "code-ok"
                        : "code-error"}"
                      >${item.data.statusCode}</span
                    >
                    ${when(
                      item.errors > 0,
                      () => html`
                        <span class="issue-badge error-badge">
                          <uui-icon
                            name="icon-delete"
                            style="font-size: 0.85em;"
                          ></uui-icon>
                          ${item.errors}
                        </span>
                      `,
                    )}
                    ${when(
                      item.warnings > 0,
                      () => html`
                        <span class="issue-badge warning-badge">
                          <uui-icon
                            name="icon-info"
                            style="font-size: 0.85em;"
                          ></uui-icon>
                          ${item.warnings}
                        </span>
                      `,
                    )}
                    ${when(
                      item.errors === 0 && item.warnings === 0,
                      () => html`<span class="page-ok">✓ No issues</span>`,
                    )}
                  </div>
                  <span class="page-toggle">${item.open ? "▲" : "▼"}</span>
                </div>
                ${when(
                  item.open,
                  () => html`
                    <div class="page-results">
                      ${when(
                        (item.data.results?.length ?? 0) === 0,
                        () =>
                          html`<p class="no-issues">
                            No issues found for this page.
                          </p>`,
                        () =>
                          repeat(
                            item.data.results ?? [],
                            (result) => result.checkId,
                            (result) => html`
                              <div
                                class="page-result-item ${result.isError
                                  ? "result-error"
                                  : "result-warning"}"
                              >
                                ${when(
                                  result.isError,
                                  () =>
                                    html`<uui-icon
                                      name="icon-delete"
                                      style="color: var(--uui-color-danger); flex-shrink: 0;"
                                    ></uui-icon>`,
                                )}
                                ${when(
                                  result.isWarning,
                                  () =>
                                    html`<uui-icon
                                      name="icon-info"
                                      style="color: var(--uui-color-warning); flex-shrink: 0;"
                                    ></uui-icon>`,
                                )}
                                <span>${result.message}</span>
                              </div>
                            `,
                          ),
                      )}
                    </div>
                  `,
                )}
              </div>
            `,
          )}
        </div>

        ${when(
          filteredPages.length > 10,
          () => html`
            <div class="pagination">
              <uui-pagination
                total=${Math.ceil(filteredPages.length / 10)}
                current=${this._pageIndex + 1}
                @change=${this.handlePageUpdate}
              ></uui-pagination>
            </div>
          `,
        )}
      </uui-box>
    `;
  }

  override render() {
    return html`
      <div class="site-audit-detail">
        <div class="button-bar">
          <uui-button label="Back" look="outline" @click="${this.#back}"
            >Back</uui-button
          >
          <umb-dropdown>
            <span slot="label">Actions</span>
            <div id="dropdown-layout">
              <uui-button
                label="Delete"
                look="default"
                compact
                @click=${this.handleDeleteAudit}
                >Delete</uui-button
              >
              ${when(
                this._model?.status === "Running",
                () => html`
                  <uui-button
                    label="Stop audit"
                    look="default"
                    compact
                    @click=${this.handleStopAudit}
                    >Stop audit</uui-button
                  >
                `,
              )}
            </div>
          </umb-dropdown>
        </div>

        ${this.#renderHeader()} ${this.#renderTabBar()}

        <div class="tab-content">
          ${when(
            this._activeTab === DetailTab.Overview,
            () => this.#renderOverview(),
          )}
          ${when(
            this._activeTab === DetailTab.Issues,
            () => this.#renderIssues(),
          )}
          ${when(
            this._activeTab === DetailTab.Pages,
            () => this.#renderPages(),
          )}
        </div>
      </div>
    `;
  }

  static override styles = [
    css`
      .site-audit-detail {
        height: 100%;
        overflow-y: scroll;
        padding: 20px;
        box-sizing: border-box;
      }

      #dropdown-layout {
        padding: 10px 6px;
        display: flex;
        flex-direction: column;
        --uui-button-content-align: left;
      }

      /* ---- Button bar ---- */
      .button-bar {
        display: flex;
        justify-content: space-between;
        margin-bottom: 16px;
      }

      /* ---- Header box ---- */
      .header-box {
        margin-bottom: 16px;
      }

      .header-content {
        display: flex;
        gap: 24px;
        align-items: flex-start;
      }

      /* ---- Score circle ---- */
      .score-section {
        flex-shrink: 0;
      }

      .score-circle {
        width: 100px;
        height: 100px;
        border-radius: 50%;
        border: 4px solid var(--score-color, var(--uui-color-text-alt));
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 2px;
      }

      .score-value {
        font-size: 2rem;
        font-weight: 700;
        color: var(--score-color, var(--uui-color-text-alt));
        line-height: 1;
      }

      .score-label {
        font-size: 0.7rem;
        color: var(--score-color, var(--uui-color-text-alt));
        text-transform: uppercase;
        letter-spacing: 0.05em;
      }

      /* ---- Header info ---- */
      .header-info {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .audit-name {
        margin: 0;
        font-size: 1.25rem;
      }

      .status-row {
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .status-badge {
        display: inline-block;
        padding: 2px 10px;
        border-radius: 20px;
        font-size: 0.75rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        background: var(--uui-color-surface-alt);
        color: var(--uui-color-text);
      }

      .status-badge.status-running {
        background: var(--uui-color-focus);
        color: white;
      }

      .status-badge.status-finished {
        background: var(--uui-color-positive);
        color: white;
      }

      .status-badge.status-error {
        background: var(--uui-color-danger);
        color: white;
      }

      .status-badge.status-scheduled {
        background: var(--uui-color-warning);
        color: white;
      }

      /* ---- Stats row ---- */
      .stats-row {
        display: flex;
        gap: 24px;
      }

      .stat {
        display: flex;
        align-items: center;
        gap: 6px;
      }

      .stat-value {
        font-size: 1.1rem;
        font-weight: 600;
      }

      .stat-label {
        font-size: 0.8rem;
        color: var(--uui-color-text-alt);
      }

      .pages-total {
        font-weight: 400;
        color: var(--uui-color-text-alt);
      }

      /* ---- Progress ---- */
      .progress-section {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }

      .progress-label-row {
        display: flex;
        justify-content: space-between;
        font-size: 0.85rem;
        color: var(--uui-color-text-alt);
      }

      /* ---- Tabs ---- */
      .tab-bar {
        display: flex;
        gap: 0;
        border-bottom: 2px solid var(--uui-color-border);
        margin-bottom: 16px;
      }

      .tab-btn {
        display: flex;
        align-items: center;
        gap: 6px;
        padding: 10px 20px;
        border: none;
        background: none;
        cursor: pointer;
        font-size: 0.95rem;
        color: var(--uui-color-text-alt);
        border-bottom: 3px solid transparent;
        margin-bottom: -2px;
        transition: color 0.15s;
      }

      .tab-btn:hover {
        color: var(--uui-color-text);
      }

      .tab-btn.active {
        color: var(--uui-color-interactive);
        border-bottom-color: var(--uui-color-interactive);
        font-weight: 600;
      }

      .tab-count {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 20px;
        height: 20px;
        padding: 0 6px;
        border-radius: 10px;
        background: var(--uui-color-surface-alt);
        color: var(--uui-color-text);
        font-size: 0.75rem;
        font-weight: 600;
      }

      .tab-count--issue {
        background: var(--uui-color-danger);
        color: white;
      }

      /* ---- Tab content ---- */
      .tab-content {
        display: flex;
        flex-direction: column;
        gap: 16px;
        padding-bottom: 80px;
      }

      /* ---- Overview tab ---- */
      .overview-issues {
        display: flex;
        flex-direction: column;
        gap: 2px;
      }

      .overview-issue-item {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 0;
        border-bottom: 1px solid var(--uui-color-border);
      }

      .overview-issue-item:last-child {
        border-bottom: none;
      }

      .overview-issue-name {
        flex: 1;
        font-weight: 500;
      }

      .overview-issue-meta {
        font-size: 0.85rem;
        color: var(--uui-color-text-alt);
        white-space: nowrap;
      }

      /* ---- Issues tab ---- */
      .issues-list {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }

      .issue-item {
        border: 1px solid var(--uui-color-border);
        border-radius: 4px;
        overflow: hidden;
      }

      .issue-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 12px 16px;
        cursor: pointer;
        background: var(--uui-color-surface);
        transition: background 0.15s;
      }

      .issue-header:hover {
        background: var(--uui-color-surface-alt);
      }

      .issue-header-left {
        display: flex;
        align-items: center;
        gap: 8px;
        flex: 1;
        min-width: 0;
      }

      .issue-check-name {
        font-weight: 500;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      .issue-header-right {
        display: flex;
        align-items: center;
        gap: 12px;
        flex-shrink: 0;
        margin-left: 16px;
      }

      .issue-page-count {
        font-size: 0.85rem;
        color: var(--uui-color-text-alt);
      }

      .issue-toggle {
        font-size: 0.75rem;
        color: var(--uui-color-text-alt);
      }

      .issue-pages {
        background: var(--uui-color-surface-alt);
        padding: 8px 16px;
        border-top: 1px solid var(--uui-color-border);
        display: flex;
        flex-direction: column;
        gap: 6px;
      }

      .issue-page-url {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 0.875rem;
        color: var(--uui-color-text-alt);
        padding: 4px 0;
      }

      /* ---- Pages tab ---- */
      .pages-filter-bar {
        display: flex;
        gap: 4px;
        flex-wrap: wrap;
      }

      .filter-btn {
        padding: 4px 12px;
        border: 1px solid var(--uui-color-border);
        border-radius: 4px;
        background: var(--uui-color-surface);
        cursor: pointer;
        font-size: 0.85rem;
        color: var(--uui-color-text-alt);
        transition: all 0.15s;
      }

      .filter-btn:hover {
        border-color: var(--uui-color-interactive);
        color: var(--uui-color-interactive);
      }

      .filter-btn.active {
        background: var(--uui-color-interactive);
        border-color: var(--uui-color-interactive);
        color: white;
      }

      .pages-list {
        display: flex;
        flex-direction: column;
        gap: 4px;
        margin-top: 4px;
      }

      .page-item {
        border: 1px solid var(--uui-color-border);
        border-radius: 4px;
        overflow: hidden;
      }

      .page-item.has-errors {
        border-left: 3px solid var(--uui-color-danger);
      }

      .page-item.has-warnings {
        border-left: 3px solid var(--uui-color-warning);
      }

      .page-main {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 10px 14px;
        cursor: pointer;
        background: var(--uui-color-surface);
        transition: background 0.15s;
      }

      .page-main:hover {
        background: var(--uui-color-surface-alt);
      }

      .page-url {
        flex: 1;
        font-size: 0.875rem;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        min-width: 0;
      }

      .page-meta {
        display: flex;
        align-items: center;
        gap: 8px;
        flex-shrink: 0;
      }

      .status-code {
        font-size: 0.8rem;
        font-weight: 600;
        padding: 2px 6px;
        border-radius: 3px;
      }

      .status-code.code-ok {
        background: var(--uui-color-positive-standalone);
        color: var(--uui-color-positive-emphasis);
      }

      .status-code.code-error {
        background: var(--uui-color-danger-standalone);
        color: var(--uui-color-danger-emphasis);
      }

      .issue-badge {
        display: inline-flex;
        align-items: center;
        gap: 3px;
        padding: 2px 8px;
        border-radius: 10px;
        font-size: 0.8rem;
        font-weight: 600;
      }

      .error-badge {
        background: var(--uui-color-danger-standalone);
        color: var(--uui-color-danger-emphasis);
      }

      .warning-badge {
        background: var(--uui-color-warning-standalone);
        color: var(--uui-color-warning-emphasis);
      }

      .page-ok {
        font-size: 0.8rem;
        color: var(--uui-color-positive);
      }

      .page-toggle {
        font-size: 0.75rem;
        color: var(--uui-color-text-alt);
        flex-shrink: 0;
      }

      .page-results {
        background: var(--uui-color-surface-alt);
        border-top: 1px solid var(--uui-color-border);
        padding: 8px 16px;
        display: flex;
        flex-direction: column;
        gap: 6px;
      }

      .page-result-item {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        font-size: 0.875rem;
        padding: 4px 0;
      }

      .no-issues {
        font-size: 0.875rem;
        color: var(--uui-color-text-alt);
        margin: 4px 0;
      }

      /* ---- Shared ---- */
      .empty-state {
        padding: 20px 0;
        color: var(--uui-color-text-alt);
        text-align: center;
      }

      .pagination {
        display: flex;
        justify-content: center;
        margin-top: 16px;
      }
    `,
  ];
}
