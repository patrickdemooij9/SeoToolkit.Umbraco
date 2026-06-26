import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  repeat,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import { SiteAuditCheckViewModel } from "../api";
import SiteAuditContentViewContext, {
  SiteAuditContentCheckResult,
  ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT,
} from "./SiteAuditContentViewContext";

@customElement("st-siteaudit-content-view")
export default class SiteAuditContentViewElement extends UmbElementMixin(
  LitElement,
) {
  #context?: SiteAuditContentViewContext;
  #contentChecks: SiteAuditCheckViewModel[] = [];
  #contentCheckResult: SiteAuditContentCheckResult[] = [];

  #isRunning = false;
  #hasRan = false;

  constructor() {
    super();

    this.consumeContext(ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT, (context) => {
      this.#context = context;

      context?.contentChecks.subscribe((item) => (this.#contentChecks = item));
      context?.contentCheckResults.subscribe(
        (item) => (this.#contentCheckResult = item),
      );
    });
  }

  getItemCheck(checkId: number) {
    return this.#contentCheckResult.find((item) => item.checkId == checkId);
  }

  async runChecks() {
    this.#isRunning = true;
    await this.#context?.runChecks();
    this.#isRunning = false;
    this.#hasRan = true;
    this.requestUpdate();
  }

  render() {
    return html`
      <div class="content-checks-header">
        <h3>Page checks</h3>
        <uui-button look="primary" @click=${this.runChecks} .state=${this.#isRunning ? "waiting" : undefined}>
          Start checks
        </uui-button>
      </div>
      <div class="content-checks">
        ${repeat(
          this.#contentChecks,
          (item) => item.id,
          (item) => html`
            <div class="content-check">
              <div class="content" style="align-items: center;gap: 12px">
                ${when(this.#hasRan, () =>
                  (this.getItemCheck(item.id as number)?.hasError ?? false)
                    ? html`<uui-icon
                        name="icon-delete"
                        style="color: var(--uui-color-danger);"
                      ></uui-icon>`
                    : html`<uui-icon
                        name="icon-check"
                        style="color: var(--uui-color-success);"
                      ></uui-icon>`,
                )}
                <div class="content-info">
                  <p class="content-name">${item.name}</p>
                  <p>${item.description}</p>
                </div>
              </div>
              ${when(
                this.#hasRan && this.getItemCheck(item.id as number)?.errorMessage,
                () => html`
                  <div class="error-message">
                    ${this.getItemCheck(item.id as number)?.errorMessage}
                  </div>
                `,
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

    .error-message {
      margin-top: 8px;
      padding: 12px;
      color: var(--uui-color-danger);
    }
  `;
}
