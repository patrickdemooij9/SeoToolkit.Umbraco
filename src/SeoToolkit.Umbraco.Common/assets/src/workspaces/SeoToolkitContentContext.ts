import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";
import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { SeoContentSavedEvent } from "../events/seoContentSavedEvent";

const INVARIANT_CULTURE = "invariant";

/**
 * Detects when the document in the workspace has been saved and tells the modules about
 * it, so each of them can persist their own settings without repeating the detection.
 *
 * Umbraco has no client-side event for "this document was saved": UmbEntityUpdatedEvent
 * is only dispatched for updates, never for the first save of a new node, and it does
 * not say which cultures were saved. A variant's update date changing is the signal
 * that does cover both.
 */
export default class SeoToolkitContentContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #actionEventContext?: UmbActionEventContext;
  #nodeId?: string;

  #lastUpdated: { [culture: string]: string | null } = {};

  constructor(host: UmbControllerHost) {
    super(host, ST_SEO_CONTENT_TOKEN_CONTEXT.toString());

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      if (!instance) return;
      this.#actionEventContext = instance;
    });

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      if (!instance) return;

      this.observe(
        instance.unique,
        (unique) => {
          const nodeId = unique?.toString();
          if (!nodeId || nodeId === this.#nodeId) return;

          this.#nodeId = nodeId;
          this.#lastUpdated = {};
        },
        "stSeoContentUnique"
      );

      this.observe(
        instance.data,
        (item) => {
          if (!this.#nodeId) return;
          if (item?.isTrashed) return;

          const savedCultures: string[] = [];
          item?.variants.forEach((variant) => {
            const culture = variant.culture ?? INVARIANT_CULTURE;
            const updateDate = variant.updateDate ?? null;

            const seen = culture in this.#lastUpdated;
            const previous = this.#lastUpdated[culture];
            this.#lastUpdated[culture] = updateDate;

            // First time we see this culture we only learn its current state, it tells
            // us nothing about a save.
            if (!seen) return;
            if (previous === updateDate) return;

            savedCultures.push(culture);
          });

          if (savedCultures.length === 0) return;

          this.#actionEventContext?.dispatchEvent(
            new SeoContentSavedEvent({
              unique: this.#nodeId,
              cultures: savedCultures,
            })
          );
        },
        "stSeoContentData"
      );
    });
  }

  getEntityType(): string {
    return "st-seo-content";
  }
}

export const ST_SEO_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<SeoToolkitContentContext>("ST-SeoContent-Context");
