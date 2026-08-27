import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbArrayState } from "@umbraco-cms/backoffice/observable-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import SiteAuditContentCheckSource from "../dataAccess/SiteAuditContentCheckSource";
import {
  SiteAuditCheckCatalogueEntry,
  SiteAuditIssue,
} from "../dataAccess/SiteAuditApi";

export default class SiteAuditContentViewContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #source: SiteAuditContentCheckSource;
  #nodeId?: string;

  #contentChecks = new UmbArrayState<SiteAuditCheckCatalogueEntry>([], (item) => item.alias);
  public readonly contentChecks = this.#contentChecks.asObservable();

  /**
   * Only failures come back from the server. A check with no issue against it passed, which is
   * what lets the tick be shown without the server reporting every non-finding.
   */
  #issues = new UmbArrayState<SiteAuditIssue>([], (item) => item.id);
  public readonly issues = this.#issues.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT.toString());

    this.#source = new SiteAuditContentCheckSource(this);
    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      instance?.unique.subscribe((unique) => {
        this.#nodeId = unique?.toString();
      });
    });
    this.#source.getPageChecks().then((response) => {
      this.#contentChecks.setValue(response.data ?? []);
    });
  }

  async runChecks() {
    const result = await this.#source.runPageChecks(this.#nodeId!);

    if (!result?.data) {
      this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
        instance?.peek("danger", {
          data: {
            headline: "Error",
            message: "Could not check this page.",
          },
        });
      });
      return;
    }

    this.#issues.setValue(result.data.issues ?? []);
  }

  getEntityType(): string {
    return "st-siteaudit";
  }
}

export const ST_SITEAUDIT_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<SiteAuditContentViewContext>(
    "ST-SiteAuditContent-Context",
  );
