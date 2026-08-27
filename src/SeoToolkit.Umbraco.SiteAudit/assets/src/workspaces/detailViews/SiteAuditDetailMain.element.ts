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
import SiteAuditDetailContext, {
  RESOURCE_PAGE_SIZE,
  ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT,
} from "../SiteAuditDetailContext";
import {
  SiteAuditCategorySummary,
  SiteAuditIssue,
  SiteAuditResource,
  SiteAuditResourceDetail,
  SiteAuditRunDetail,
} from "../../dataAccess/SiteAuditApi";

enum SelectedView {
  Summary,
  Pages,
  Issues,
}

@customElement("st-siteaudit-detail-main")
export default class SiteAuditDetailMain
  extends UmbLitElement
  implements UmbWorkspaceViewElement
{
  #context?: SiteAuditDetailContext;

  @state() private _model?: SiteAuditRunDetail;
  @state() private _categories: SiteAuditCategorySummary[] = [];
  @state() private _resources: SiteAuditResource[] = [];
  @state() private _issues: SiteAuditIssue[] = [];
  @state() private _resourceTotal = 0;
  @state() private _view = SelectedView.Summary;
  @state() private _pageIndex = 0;
  @state() private _checkFilter?: string;
  @state() private _selectedResource?: SiteAuditResourceDetail;
  @state() private _loadingResource?: number;

  constructor() {
    super();

    this.consumeContext(ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT, (context) => {
      this.#context = context;
      if (!context) return;

      this.observe(context.model, (model) => (this._model = model));
      this.observe(context.categories, (items) => (this._categories = items));
      this.observe(context.resources, (items) => (this._resources = items));
      this.observe(context.issues, (items) => (this._issues = items));
      this.observe(context.resourceTotal, (total) => (this._resourceTotal = total));
      this.observe(context.selectedResource, (item) => (this._selectedResource = item));
      this.observe(context.loadingResource, (id) => (this._loadingResource = id));
    });
  }

  #show(view: SelectedView) {
    this._view = view;
    if (view !== SelectedView.Issues) this._checkFilter = undefined;
  }

  /** Server-side paging: the client asks for the slice it needs rather than filtering a full list. */
  #onPageChange(event: UUIPaginationEvent) {
    this._pageIndex = event.target.current - 1;
    this.#context?.loadResources({
      skip: this._pageIndex * RESOURCE_PAGE_SIZE,
      take: RESOURCE_PAGE_SIZE,
    });
  }

  /** Jumps to the issues for one check. Replaces the dead AngularJS click handler this had. */
  #filterByCheck(alias: string) {
    this._checkFilter = alias;
    this._view = SelectedView.Issues;
    this.#context?.loadIssues(alias);
  }

  #clearCheckFilter() {
    this._checkFilter = undefined;
    this.#context?.loadIssues();
  }

  render() {
    if (!this._model) return html`<uui-loader></uui-loader>`;

    return html`
      <uui-box>
        <div class="header">
          <div>
            <h3>${this._model.name}</h3>
            <p class="muted">${this._model.startingUrl}</p>
          </div>
          ${this.#renderScore()}
        </div>

        <div class="stats">
          ${this.#renderStat("Pages crawled", this.#renderCrawledCount())}
          ${this.#renderStat("Errors", this._model.errorCount + this._model.criticalCount, "danger")}
          ${this.#renderStat("Warnings", this._model.warningCount, "warning")}
          ${this.#renderStat("Status", this._model.status)}
          ${this.#renderStat("Page limit", this._model.maxPages ?? "None")}
        </div>

        ${when(
          !this._model.isFinished,
          () => html`<uui-progress-bar .progress=${this._model!.progress}></uui-progress-bar>`
        )}
        ${when(this.#hitPageLimit(), () =>
          html`<p class="muted">
            The crawl stopped at its limit of ${this._model!.maxPages} pages, so parts of the
            site were not visited.
          </p>`
        )}
        ${when(
          this._model.status === "Scheduled",
          () => html`<p class="muted">Waiting to start. This usually begins within a minute.</p>`
        )}
        ${when(
          this._model.status === "Interrupted",
          () => html`<p class="muted">
            This audit stopped reporting and was not finished. Its partial results are kept.
          </p>`
        )}
      </uui-box>

      <div class="navigation-buttons">
        <uui-button
          label="Summary"
          look=${this._view === SelectedView.Summary ? "primary" : "outline"}
          @click=${() => this.#show(SelectedView.Summary)}
        >
          Summary
        </uui-button>
        <uui-button
          label="All pages"
          look=${this._view === SelectedView.Pages ? "primary" : "outline"}
          @click=${() => this.#show(SelectedView.Pages)}
        >
          All pages (${this._resourceTotal})
        </uui-button>
        <uui-button
          label="All issues"
          look=${this._view === SelectedView.Issues ? "primary" : "outline"}
          @click=${() => this.#show(SelectedView.Issues)}
        >
          All issues
        </uui-button>
        ${when(
          this.#context?.exportUrl,
          () => html`<a class="export" href=${this.#context!.exportUrl!} download>
            <uui-button label="Export CSV" look="outline">Export CSV</uui-button>
          </a>`
        )}
      </div>

      ${when(this._view === SelectedView.Summary, () => this.#renderSummary())}
      ${when(this._view === SelectedView.Pages, () => this.#renderPages())}
      ${when(this._view === SelectedView.Issues, () => this.#renderIssues())}
    `;
  }

  #renderCrawledCount() {
    const model = this._model!;
    return model.isFinished
      ? `${model.totalCrawled}`
      : `${model.totalCrawled} / ${model.totalDiscovered}`;
  }

  /** Whether the crawl was cut short rather than simply running out of pages to visit. */
  #hitPageLimit() {
    const model = this._model;
    if (!model?.isFinished || !model.maxPages) return false;

    return model.totalCrawled >= model.maxPages;
  }

  #renderScore() {
    // The score is only meaningful once a run has finished, and is not calculated yet.
    if (this._model?.score === null || this._model?.score === undefined) return null;

    return html`<div class="score">
      <span class="score-value">${this._model.score}</span>
      <span class="muted">Health score</span>
    </div>`;
  }

  #renderStat(label: string, value: unknown, tone?: string) {
    return html`<div class="stat">
      <span class="stat-value ${tone ?? ""}">${value}</span>
      <span class="muted">${label}</span>
    </div>`;
  }

  #renderSummary() {
    if (this._categories.length === 0) {
      return html`<uui-box><p class="muted">No results were recorded for this audit.</p></uui-box>`;
    }

    return html`
      ${repeat(
        this._categories,
        (category) => category.category,
        (category) => html`
          <uui-box headline=${category.category}>
            <div class="checks">
              ${repeat(
                category.checks,
                (check) => check.alias,
                (check) => html`
                  <div class="check">
                    <div class="check-info">
                      <button
                        class="link"
                        ?disabled=${check.failedCount === 0}
                        @click=${() => this.#filterByCheck(check.alias)}
                      >
                        ${check.name}
                      </button>
                      ${when(
                        check.description,
                        () => html`<p class="muted">${check.description}</p>`
                      )}
                    </div>
                    <div class="check-result">
                      ${when(
                        !check.didRun,
                        () => html`<span class="muted">Not run</span>`,
                        () =>
                          check.failedCount === 0
                            ? html`<uui-icon
                                name="icon-check"
                                style="color: var(--uui-color-success);"
                              ></uui-icon>`
                            : html`<span class=${check.severity === "Warning" ? "warning" : "danger"}>
                                ${check.failedCount} of ${check.applicableCount}
                              </span>`
                      )}
                    </div>
                  </div>
                `
              )}
            </div>
          </uui-box>
        `
      )}
    `;
  }

  #renderPages() {
    return html`
      <uui-box headline="All pages">
        <p class="muted table-hint">Select a page to see what was found on it.</p>
        <div class="grid">
          <div class="grid-row grid-head">
            <div>Url</div>
            <div>Status</div>
            <div>Issues</div>
            <div>Time</div>
          </div>
          ${repeat(
            this._resources,
            (resource) => resource.id,
            (resource) => this.#renderPageRow(resource)
          )}
        </div>
        ${when(
          this._resourceTotal > RESOURCE_PAGE_SIZE,
          () => html`<uui-pagination
            .current=${this._pageIndex + 1}
            .total=${Math.ceil(this._resourceTotal / RESOURCE_PAGE_SIZE)}
            @change=${this.#onPageChange}
          ></uui-pagination>`
        )}
      </uui-box>
    `;
  }

  #renderPageRow(resource: SiteAuditResource) {
    const isOpen = this._selectedResource?.id === resource.id;
    const isLoading = this._loadingResource === resource.id;

    return html`
      <div
        class="grid-row selectable ${isOpen ? "open" : ""}"
        role="button"
        tabindex="0"
        aria-expanded=${isOpen ? "true" : "false"}
        @click=${() => this.#context?.selectResource(resource.id)}
        @keydown=${(event: KeyboardEvent) => this.#onRowKey(event, resource.id)}
      >
        <div class="url" title=${resource.url}>
          <uui-icon
            class="chevron"
            name=${isOpen ? "icon-navigation-down" : "icon-navigation-right"}
          ></uui-icon>
          ${resource.path}
          ${when(
            !resource.isIndexable,
            () => html`<span class="badge" title=${resource.indexabilityReasons.join(", ")}>
              ${resource.indexabilityReasons[0] ?? "Not indexable"}
            </span>`
          )}
        </div>
        <div class=${this.#statusClass(resource.statusCode)}>
          ${resource.statusCode === 0 ? (resource.failure ?? "Failed") : resource.statusCode}
        </div>
        <div>
          ${when(
            resource.errorCount > 0,
            () => html`<span class="danger">${resource.errorCount}</span> `
          )}
          ${when(
            resource.warningCount > 0,
            () => html`<span class="warning">${resource.warningCount}</span>`
          )}
          ${when(resource.issueCount === 0, () => html`<span class="muted">-</span>`)}
        </div>
        <div class="muted">${resource.responseTimeMs} ms</div>
      </div>
      ${when(isLoading, () => html`<div class="page-detail"><uui-loader></uui-loader></div>`)}
      ${when(isOpen, () => this.#renderPageDetail(this._selectedResource!))}
    `;
  }

  /** Space and Enter open a row, since it behaves as a button rather than being one. */
  #onRowKey(event: KeyboardEvent, resourceId: number) {
    if (event.key !== "Enter" && event.key !== " ") return;

    event.preventDefault();
    this.#context?.selectResource(resourceId);
  }

  #renderPageDetail(resource: SiteAuditResourceDetail) {
    return html`
      <div class="page-detail">
        <div class="facts">
          ${this.#renderFact("Url", html`<a href=${resource.url} target="_blank" rel="noopener">
            ${resource.url}
          </a>`)}
          ${when(
            resource.finalUrl && resource.finalUrl !== resource.url,
            () => this.#renderFact("Redirected to", resource.finalUrl)
          )}
          ${this.#renderFact("Status", resource.statusCode === 0
            ? (resource.failure ?? "Failed")
            : `${resource.statusCode}`)}
          ${this.#renderFact("Indexable", resource.isIndexable
            ? "Yes"
            : resource.indexabilityReasons.join(", ") || "No")}
          ${this.#renderFact("Title", resource.title || "-")}
          ${this.#renderFact("Meta description", resource.metaDescription || "-")}
          ${this.#renderFact("H1", resource.h1 || "-")}
          ${this.#renderFact("Words", `${resource.wordCount}`)}
          ${this.#renderFact("Depth", `${resource.depth}`)}
          ${this.#renderFact("Response", `${resource.responseTimeMs} ms`)}
          ${this.#renderFact("Size", `${Math.round(resource.sizeBytes / 1024)} kB`)}
        </div>

        ${when(
          resource.issues.length === 0,
          () => html`<p class="muted">Nothing was found on this page.</p>`,
          () => html`<div class="issues">
            ${repeat(
              resource.issues,
              (issue) => issue.id,
              (issue) => this.#renderIssue(issue, false)
            )}
          </div>`
        )}
      </div>
    `;
  }

  #renderFact(label: string, value: unknown) {
    if (value === undefined || value === null) return nothing;

    return html`<div class="fact">
      <span class="muted">${label}</span>
      <span class="fact-value">${value}</span>
    </div>`;
  }

  #renderIssues() {
    return html`
      <uui-box headline="All issues">
        ${when(
          this._checkFilter,
          () => html`<div class="filter">
            Showing ${this._checkFilter}
            <uui-button label="Clear" look="outline" compact @click=${this.#clearCheckFilter}>
              Clear
            </uui-button>
          </div>`
        )}
        ${when(
          this._issues.length === 0,
          () => html`<p class="muted">Nothing to report.</p>`,
          () => html`
            <div class="issues">
              ${repeat(
                this._issues,
                (issue) => issue.id,
                (issue) => this.#renderIssue(issue, true)
              )}
            </div>
          `
        )}
      </uui-box>
    `;
  }

  /**
   * One finding. The url is only worth repeating in the run-wide list - inside a page it is
   * already known, and the check that raised it is the useful part.
   */
  #renderIssue(issue: SiteAuditIssue, withUrl: boolean) {
    return html`
      <div class="issue">
        <uui-icon
          name=${issue.isError ? "icon-delete" : "icon-alert"}
          style="color: var(--uui-color-${issue.isError ? "danger" : "warning"});"
        ></uui-icon>
        <div>
          <p class="issue-message">${issue.message}</p>
          <p class="muted">
            ${withUrl ? html`${issue.url ?? "Site-wide"} &middot; ` : nothing}${issue.checkName}
          </p>
          ${when(issue.evidence, () => html`<p class="evidence">${issue.evidence}</p>`)}
        </div>
      </div>
    `;
  }

  #statusClass(statusCode: number) {
    if (statusCode === 0) return "danger";
    if (statusCode >= 400) return "danger";
    if (statusCode >= 300) return "warning";
    return "";
  }

  static styles = css`
    :host {
      display: flex;
      flex-direction: column;
      gap: 16px;
      /* The workspace gives its views no padding of their own, so without this the boxes sit
         flush against the edges of the window. */
      padding: var(--uui-size-layout-1);
      padding-bottom: 100px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 16px;
    }

    .header h3 {
      margin: 0;
    }

    .score {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .score-value {
      font-size: 32px;
      font-weight: bold;
    }

    .stats {
      display: flex;
      flex-wrap: wrap;
      gap: 32px;
      margin: 16px 0;
    }

    .stat {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 20px;
      font-weight: bold;
    }

    .muted {
      color: var(--uui-color-text-alt);
      margin: 0;
      font-size: 12px;
    }

    .danger {
      color: var(--uui-color-danger);
      font-weight: bold;
    }

    .warning {
      color: var(--uui-color-warning-emphasis, var(--uui-color-warning));
      font-weight: bold;
    }

    /* Pinned so the view can be switched without scrolling back to the top of a long report.
       The negative margin lets the bar span the full width while the host keeps its padding;
       without it the content would be visible scrolling through the gap at either side. */
    .navigation-buttons {
      position: sticky;
      top: 0;
      z-index: 2;
      display: flex;
      gap: 8px;
      align-items: center;
      margin: 0 calc(var(--uui-size-layout-1) * -1);
      padding: 8px var(--uui-size-layout-1);
      background-color: var(--uui-color-surface);
      border-bottom: 1px solid var(--uui-color-divider);
    }

    .export {
      text-decoration: none;
    }

    .checks,
    .issues {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .check {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 16px;
      padding: 8px 0;
      border-bottom: 1px solid var(--uui-color-divider);
    }

    .link {
      background: none;
      border: none;
      padding: 0;
      font: inherit;
      color: var(--uui-color-interactive);
      cursor: pointer;
      text-align: left;
    }

    .link[disabled] {
      color: inherit;
      cursor: default;
    }

    .table-hint {
      margin-bottom: 8px;
    }

    .grid {
      display: flex;
      flex-direction: column;
    }

    .grid-row {
      display: grid;
      grid-template-columns: 1fr 100px 100px 100px;
      gap: 8px;
      padding: 6px 0;
      border-bottom: 1px solid var(--uui-color-divider);
      align-items: center;
    }

    .grid-head {
      font-weight: bold;
    }

    .selectable {
      cursor: pointer;
    }

    .selectable:hover,
    .selectable:focus-visible {
      background-color: var(--uui-color-surface-alt);
    }

    .selectable.open {
      background-color: var(--uui-color-surface-alt);
      border-bottom: none;
      font-weight: bold;
    }

    .chevron {
      margin-right: 4px;
      vertical-align: middle;
      font-size: 12px;
    }

    .url {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .badge {
      display: inline-block;
      margin-left: 8px;
      padding: 1px 6px;
      border-radius: 3px;
      font-size: 11px;
      background-color: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .page-detail {
      padding: 12px 12px 16px 12px;
      background-color: var(--uui-color-surface-alt);
      border-bottom: 1px solid var(--uui-color-divider);
    }

    .facts {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 8px 16px;
      margin-bottom: 12px;
    }

    .fact {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .fact-value {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .issue {
      display: flex;
      gap: 12px;
      align-items: flex-start;
      padding: 8px 0;
      border-bottom: 1px solid var(--uui-color-divider);
    }

    .issue-message {
      margin: 0;
    }

    .evidence {
      margin: 4px 0 0 0;
      padding: 2px 6px;
      border-radius: 3px;
      background-color: var(--uui-color-surface);
      font-family: monospace;
      font-size: 12px;
      overflow-wrap: anywhere;
    }

    .filter {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 12px;
    }
  `;
}
