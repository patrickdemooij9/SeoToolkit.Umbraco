import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, repeat, when } from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";
import { SiteAuditCheckViewModel } from "../api";
import SiteAuditContentViewContext, { SiteAuditContentCheckResult, ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT } from "./SiteAuditContentViewContext";

@customElement("st-siteaudit-content-view")
export default class SiteAuditContentViewElement extends UmbElementMixin(LitElement) {
    #context?: SiteAuditContentViewContext;
    #contentChecks: SiteAuditCheckViewModel[] = [];
    #contentCheckResult: SiteAuditContentCheckResult[] = [];

    //#isRunning = false;
    #hasRan = false;

    constructor() {
        super();
        
        this.consumeContext(ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT, (context) => {
            this.#context = context;

            context?.contentChecks.subscribe((item) => this.#contentChecks = item);
            context?.contentCheckResults.subscribe((item) => this.#contentCheckResult = item);
        })
    }

    getItemCheck(checkId: number) {
        return this.#contentCheckResult.find((item) => item.checkId == checkId);
    }

    async runChecks(){
        //this.#isRunning = true;
        await this.#context?.runChecks();
        //this.#isRunning = false;
        this.#hasRan = true;
    }

    render() {
        return html`
        <div style="display: flex; justify-content: space-between; align-items: center;">
        <h3>Page checks</h3>
                  <uui-button look="primary" click=${this.runChecks}>
            Start checks
          </uui-button>
    </div>
    <div class="umb-panel-group__details-checks">
    ${repeat(this.#contentChecks, (item) => item.id, (item) => html`
<div class="umb-panel-group__details-check-title flex" style="flex-direction: column; gap: 4px">
            <div class="flex" style="align-items: center;gap: 12px">
                ${when(this.#hasRan, () => this.getItemCheck(item.id)?.hasError ?? false ? html`<uui-icon
                  name="icon-delete"
                  style="color: var(--uui-color-danger);"
                ></uui-icon>` : html`<uui-icon
                  name="icon-check"
                  style="color: var(--uui-color-success);"
                ></uui-icon>`)}
                <div>
                    <div class="umb-panel-group__details-check-name ng-binding">${item.name}</div>
                    <div class="umb-panel-group__details-check-description ng-binding">${item.description}</div>
                </div>
            </div>
            ${when(this.#hasRan && this.getItemCheck(item.id)?.errorMessage, () => html`
                <div class="color-red">
                ${this.getItemCheck(item.id)?.errorMessage}
            </div>
                `)}
        </div>
        `)}
        
    </div>
        `
    }
}