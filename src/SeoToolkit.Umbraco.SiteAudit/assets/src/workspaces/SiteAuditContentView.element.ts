import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import SiteAuditContentViewContext, {
  ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT,
} from "./SiteAuditContentViewContext";
import {
  SiteAuditCheckCatalogueEntry,
  SiteAuditIssue,
} from "../dataAccess/SiteAuditApi";

@customElement("st-siteaudit-content-view")
export default class SiteAuditContentViewElement extends UmbElementMixin(
  LitElement,
) {
  #context?: SiteAuditContentViewContext;
  #contentChecks: SiteAuditCheckCatalogueEntry[] = [];
  #issues: SiteAuditIssue[] = [];

  #isRunning = false;
  #hasRan = false;

  constructor() {
    super();

    this.consumeContext(ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT, (context) => {
      this.#context = context;

      context?.contentChecks.subscribe((item) => (this.#contentChecks = item));
      context?.issues.subscribe((item) => {
        this.#issues = item;
        this.requestUpdate();
      });
    });
  }

  /** Findings for one check. A check with none passed. */
  #issuesFor(alias: string) {
    return this.#issues.filter((issue) => issue.checkAlias === alias);
  }

  async runChecks() {
    this.#isRunning = true;
    this.requestUpdate();

    await this.#context?.runChecks();

    this.#isRunning = false;
    this.#hasRan = true;
    this.requestUpdate();
  }

  /**
   * A check only reports when it fails, so "no issues" means it passed. Errors and warnings are
   * shown differently - an editor should be able to tell a blocking problem from a suggestion.
   */
  #renderStatusIcon(alias: string) {
    const issues = this.#issuesFor(alias);

    if (issues.length === 0) {
      return html`<uui-icon
        name="icon-check"
        title="Passed"
        style="color: var(--uui-color-success);"
      ></uui-icon>`;
    }

    const hasError = issues.some((issue) => issue.isError);

    return html`<uui-icon
      name=${hasError ? "icon-delete" : "icon-alert"}
      title=${hasError ? "Error" : "Warning"}
      style="color: var(--uui-color-${hasError ? "danger" : "warning"});"
    ></uui-icon>`;
  }

  render() {
    return html`
      <div class="content-checks-header">
        <h3>Page checks</h3>
        <uui-button
          look="primary"
          @click=${this.runChecks}
          .state=${this.#isRunning ? "waiting" : undefined}
        >
          Start checks
        </uui-button>
      </div>
      <div class="content-checks">
        ${repeat(
          this.#contentChecks,
          (item) => item.alias,
          (item) => html`
            <div class="content-check">
              <div class="content" style="align-items: center;gap: 12px">
                ${when(this.#hasRan, () => this.#renderStatusIcon(item.alias))}
                <div class="content-info">
                  <p class="content-name">${item.name}</p>
                  <p>${item.description}</p>
                </div>
              </div>
              ${when(this.#hasRan, () =>
                repeat(
                  this.#issuesFor(item.alias),
                  (issue) => issue.id,
                  (issue) => html`
                    <div
                      class=${issue.isError
                        ? "result-message error"
                        : "result-message warning"}
                    >
                      ${issue.message}
                    </div>
                  `,
                ),
              )}
            </div>
          `,
        )}
      </div>
    `;
  }

  static styles = css`
    .content-checks-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .content-checks {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .content-check {
      padding: 16px;
      border: 1px solid var(--uui-color-border);
      border-radius: 4px;
      background-color: var(--uui-color-surface);
    }

    .content {
      display: flex;
      gap: 8px;
      align-items: center;
    }

    .content-info {
        > p {
            margin: 0;
        }

        .content-name {
            font-weight: bold;
        }
    }

    .result-message {
      margin-top: 8px;
      padding: 12px;
    }

    .result-message.error {
      color: var(--uui-color-danger);
    }

    .result-message.warning {
      color: var(--uui-color-warning-emphasis, var(--uui-color-warning));
    }
  `;
}
