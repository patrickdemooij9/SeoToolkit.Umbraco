import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { SiteAuditCheckViewModel } from "../api";
import { UmbArrayState } from "@umbraco-cms/backoffice/observable-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import SiteAuditContentCheckSource from "../dataAccess/SiteAuditContentCheckSource";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export interface SiteAuditContentCheckResult {
  checkId: number;
  hasError: boolean;
  errorMessage?: string;
}

export default class SiteAuditContentViewContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #source: SiteAuditContentCheckSource;
  #nodeId?: string;

  #contentChecks = new UmbArrayState<SiteAuditCheckViewModel>(
    [],
    (item) => item.id,
  );
  public readonly contentChecks = this.#contentChecks.asObservable();

  #contentCheckResults = new UmbArrayState<SiteAuditContentCheckResult>(
    [],
    (item) => item.checkId,
  );
  public readonly contentCheckResults =
    this.#contentCheckResults.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT.toString());

    this.#source = new SiteAuditContentCheckSource(this);
    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      instance?.unique.subscribe((unique) => {
        this.#nodeId = unique?.toString();
      });
    });
    this.#source.getPageChecks().then((response) => {
      this.#contentChecks.setValue(response.data);
    });
  }

  async runChecks() {
    const result = await this.#source.runPageChecks(this.#nodeId!);
    const pageCrawled = result.data.pagesCrawled![0];
    if (!pageCrawled || pageCrawled.statusCode !== 200) {
      this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
        instance?.peek("danger", {
          data: {
            headline: "Error",
            message: "Could not render the page to perform the checks!",
          },
        });
      });
      return;
    }

    this.#contentCheckResults.setValue(
      pageCrawled.results?.map<SiteAuditContentCheckResult>((check) => ({
        checkId: check.checkId as number,
        hasError: true,
        errorMessage: check.message ?? "",
      })) ?? [],
    );
  }

  getEntityType(): string {
    return "st-siteaudit";
  }
}

export const ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<SiteAuditContentViewContext>(
    "ST-SiteAuditContent-Context",
  );
