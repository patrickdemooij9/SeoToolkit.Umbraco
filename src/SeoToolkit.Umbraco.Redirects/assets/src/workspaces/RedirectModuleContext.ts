import { UmbDefaultCollectionContext } from "@umbraco-cms/backoffice/collection";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";
import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { Redirect } from "../models/Redirect";
import { SEOTOOLKIT_REDIRECT_ENTITY } from "../Constants";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import RedirectRepository from "../dataLayer/RedirectRepository";
import { RedirectModalData } from "../models/RedirectModalData";
import { RedirectOverviewItem } from "../models/RedirectOverviewItem";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export default class RedirectModuleContext extends UmbDefaultCollectionContext<
  RedirectOverviewItem,
  any
> {
  workspaceAlias = "seoToolkit.collections.redirects";

  constructor(host: UmbControllerBase) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());

    this.provideContext(ST_REDIRECT_MODULE_TOKEN_CONTEXT, this);
  }

  async openCreateModal(unique?: string) {
    let redirect: Partial<Redirect> = {
      redirectCode: 302,
      isEnabled: true,
    };
    if (unique) {
      redirect = await new RedirectRepository(this).get(
        Number.parseInt(unique)
      );
    }

    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (instance) => {
      if (!instance) {
        return;
      }
      const modal = instance.open(
        this._host,
        "seoToolkit.modal.redirect.create",
        {
          modal: { type: "sidebar", size: "medium" },
          data: {
            redirect: redirect,
          },
        }
      );

      await modal.onSubmit();

      const data = (modal.getValue() as RedirectModalData).redirect;
      let domain = data.domain;
      if (!domain || domain <= 0) {
        domain = null;
      }

      await new RedirectRepository(this).save({
        id: unique ? Number.parseInt(unique) : 0,
        domain: domain,
        customDomain: data.customDomain,
        isEnabled: data.isEnabled,
        isRegex: data.isRegex,
        oldUrl: data.oldUrl,
        newUrl: data.newUrl,
        newNodeId: data.newNodeId,
        newCultureId: data.newCultureIso,
        redirectCode: data.redirectCode,
      });

      this.requestCollection();
    });
  }

  async openImportModal() {
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (instance) => {
      if (!instance) {
        return;
      }
      const modal = instance.open(
        this._host,
        "seoToolkit.modal.redirect.import",
        {
          modal: { type: "sidebar", size: "medium" },
        }
      );

      await modal.onSubmit();

      this.requestCollection();
    });
  }

  async openStatusCodeModal(redirectIds: number[]) {
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (instance) => {
      if (!instance) {
        return;
      }
      const modal = instance.open(
        this._host,
        "seoToolkit.modal.redirect.statusCode",
        {
          modal: { type: "sidebar", size: "medium" },
        }
      );
      await modal.onSubmit();

      const redirectCode = modal.getValue() as number;
      await new RedirectRepository(this).updateStatusCodes({
        redirectIds: redirectIds,
        redirectCode: redirectCode,
      });

      this.requestCollection();

      this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
        instance?.peek("positive", {
          data: {
            headline: "Updated",
            message: redirectIds.length + " redirects have been updated.",
          },
        });
      });
    });
  }

  getEntityType(): string {
    return SEOTOOLKIT_REDIRECT_ENTITY;
  }
}

export const ST_REDIRECT_MODULE_TOKEN_CONTEXT =
  new UmbContextToken<RedirectModuleContext>("redirectsModuleContext");
