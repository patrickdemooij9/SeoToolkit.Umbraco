import {
  css,
  customElement,
  html,
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

enum SelectedPageType {
  AllPages,
  AllChecks,
  FilteredCheck,
}

interface PageItem {
  data: SiteAuditPageDetailViewModel;
  open: boolean;
  errors: number;
  warnings: number;
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
  _errors = 0;

  @state()
  _warnings = 0;

  @state()
  _pageType: SelectedPageType = SelectedPageType.AllPages;

  @state()
  _pageIndex = 0;

  @state()
  _openedPages: string[] = [];

  @state()
  _checkResults: SiteAuditCheckResult[] = [];

  constructor() {
    super();

    this.consumeContext(ST_SITEAUDIT_DETAIL_TOKEN_CONTEXT, (instance) => {
      this.#context = instance;

      this.#context.model.subscribe((value) => {
        this._model = value;

        let errors = 0;
        let warnings = 0;
        this._checkResults = [];
        this._model.pagesCrawled
          ?.flatMap((item) => item.results)
          .forEach((result) => {
            if (result?.isError) {
              errors++;
            }
            if (result?.isWarning) {
              warnings++;
            }
            if (result?.isError || result?.isWarning) {
              const existingCheckResult = this._checkResults.find(
                (check) => check.id === result.checkId
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
            }
          });
        this._errors = errors;
        this._warnings = warnings;
      });
    });
  }

  getCheckById(id: number) {
    return this._model?.checks?.find((item) => item.id == id);
  }

  getPagesPaged(): PageItem[] {
    return (
      this._model?.pagesCrawled
        ?.slice(this._pageIndex * 10, this._pageIndex * 10 + 10)
        .map<PageItem>((page) => ({
          data: page,
          open: this.#pageIsOpen(page),
          errors:
            page.results?.reduce(
              (prev, cur) => prev + (cur.isError ? 1 : 0),
              0
            ) ?? 0,
          warnings:
            page.results?.reduce(
              (prev, cur) => prev + (cur.isWarning ? 1 : 0),
              0
            ) ?? 0,
        })) ?? []
    );
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

  handlePageUpdate(event: UUIPaginationEvent) {
    this._pageIndex = event.target.current - 1;
  }

  handleDeleteAudit() {
    this.#context?.deleteAudit();
  }

  handleStopAudit() {
    this.#context?.stopAudit();
  }

  override render() {
    return html`
      <div class="site-audit-detail">
        <div class="button-bar">
          <div>
            <uui-button
              slot="actions"
              id="back"
              label="Back"
              look="outline"
              @click="${this.#back}"
            >
              Back
            </uui-button>
          </div>
          <umb-dropdown>
            <span slot="label">Actions</span>
            <div id="dropdown-layout">
              <uui-button
                label="Delete"
                look="default"
                compact
                @click=${this.handleDeleteAudit}
              >
                Delete
              </uui-button>
              ${when(
                this._model?.status == "Running",
                () => html`
                  <uui-button
                    label="Stop audit"
                    look="default"
                    compact
                    @click=${this.handleStopAudit}
                  >
                    Stop audit
                  </uui-button>
                `
              )}
            </div>
          </umb-dropdown>
        </div>
        <uui-box headline="${this._model?.name!}">
          <div slot="header" class="status-header">
            <div class="flex gap">
              <div class="flex gap align-center">
                <uui-icon
                  name="icon-delete"
                  style="color: var(--uui-color-danger);"
                ></uui-icon>
                ${this._errors}
              </div>
              <div class="flex gap align-center">
                <uui-icon
                  name="icon-info"
                  style="color: var(--uui-color-warning);"
                ></uui-icon>
                ${this._warnings}
              </div>
              <div class="flex gap align-center">
                <uui-icon name="icon-document"></uui-icon>
                <span>${this._model?.pagesCrawled?.length ?? 0}</span>
                ${when(
                  this._model?.progress !== 100,
                  () => `/${this._model?.totalPagesFound}`
                )}
              </div>
            </div>
          </div>
          <uui-progress-bar .progress=${this._model?.progress ?? 0}>
          </uui-progress-bar>

          <h4>Status: ${this._model?.status}</h4>
          ${when(
            this._model?.status === "Scheduled",
            () => html`
              <p ng-if="vm.audit.status === 'Scheduled'">
                Your site audit will begin within a minute
              </p>
            `
          )}

          <hr />
          <h4>Summary</h4>
          <div class="siteaudit-results">
            ${repeat(
              this._checkResults,
              (result) => result.id,
              (result) => html`
                <i
                  class="icon-delete color-red"
                  ng-if="checkResultValue.isError"
                ></i>
                <i
                  class="icon-info color-orange"
                  ng-if="checkResultValue.isWarning"
                ></i>
                <a ng-click="vm.filterResultsOnCheck(checkResultKey)"
                  >${this.getCheckById(result.id)?.errorMessage}
                  (${result.count} times)</a
                >
              `
            )}
          </div>
        </uui-box>

        <!--Todo: Implement these-->
        <!--<div class="navigation-buttons">
          <uui-button label="All pages" look="primary"> All pages </uui-button>
          <uui-button label="All check" look="primary"> All checks </uui-button>
        </div>-->

        ${when(
          this._pageType === SelectedPageType.AllPages,
          () => html`
            <uui-box headline="All pages" class="pages-container">
              <div>
                ${repeat(
                  this.getPagesPaged(),
                  (item) => item.data.url,
                  (item) => html`
                <div
                      class="site-audit-page"
                    >
                      <div>${item.data.url}</div>
                      <div>
                        Status:
                        <span
                          ng-class="{'error-status': page.statusCode < 200 || page.statusCode > 299}"
                          >${item.data.statusCode}</span
                        >
                      </div>
                      <div
                        class="page-health"
                      >
                      ${when(
                        item.errors > 0,
                        () => html`
                          <div class="flex align-center">
                            <uui-icon
                              name="icon-delete"
                              style="color: var(--uui-color-danger);"
                            ></uui-icon>
                            ${item.errors}
                          </div>
                        `
                      )}
                        
                        ${when(
                          item.warnings > 0,
                          () => html`
                            <div class="flex align-center">
                              <uui-icon
                                name="icon-info"
                                style="color: var(--uui-color-warning);"
                              ></uui-icon>
                              ${item.warnings}
                            </div>
                          `
                        )}
                      </div>
                      ${when(
                        item.open,
                        () => html`
                          <a
                            class="clickable"
                            @click="${() => this.#closePage(item.data)}"
                          >
                            Close
                          </a>
                        `,
                        () => html`
                          <a
                            class="clickable"
                            @click="${() => this.#openPage(item.data)}"
                          >
                            Open
                          </a>
                        `
                      )}
                    </div>
                    ${when(
                      item.open,
                      () => html`
                        <div class="result-container">
                          ${repeat(
                            item.data.results ?? [],
                            (result) => result.checkId,
                            (result) => html` <div>${result.message}</div> `
                          )}
                          ${when(
                            item.data.results?.length == 0,
                            () => html` <div>No results for this page!</div> `
                          )}
                        </div>
                      `
                    )}
                    
                  </div>
              `
                )}
              </div>
              ${when(
                (this._model!.pagesCrawled?.length ?? 0) > 10,
                () => html`
                  <div class="pagination">
                    <uui-pagination
                      total=${this._model!.pagesCrawled!.length / 10 + 1}
                      current=${this._pageIndex + 1}
                      @change=${this.handlePageUpdate}
                    >
                    </uui-pagination>
                  </div>
                `
              )}
            </uui-box>
          `
        )}
      </div>
    `;
  }

  static override styles = [
    css`
      .site-audit-detail {
        height: 100%;
        overflow-y: scroll;
        padding: 20px;
      }

      #dropdown-layout {
        padding: 10px 6px;
        display: flex;
        flex-direction: column;
        --uui-button-content-align: left;
      }

      .button-bar {
        display: flex;
        justify-content: space-between;
        margin-bottom: 10px;
      }

      .status-header {
        display: flex;
        justify-content: flex-end;
        width: 100%;
      }

      .navigation-buttons {
        width: 100%;
        display: flex;
        gap: 8px;
        margin: 20px 0 20px 0;

        > * {
          flex: 1;
        }
      }

      .site-audit-page {
        display: flex;
        justify-content: space-between;

        > div {
          flex: 1;
        }
      }

      .pages-container {
        margin-bottom: 100px; //Without this, the page isn't really scrollable and I have no clue why...
      }

      .result-container {
        display: flex;
        flex-direction: column;
        padding-bottom: 20px;
      }

      .page-health {
        display: flex;
      }

      .pagination {
        margin: 20px 0;
      }

      .flex {
        display: flex;
      }

      .gap {
        gap: 4px;
      }

      .align-center {
        align-items: center;
      }

      .clickable {
        cursor: pointer;
      }
    `,
  ];
}
