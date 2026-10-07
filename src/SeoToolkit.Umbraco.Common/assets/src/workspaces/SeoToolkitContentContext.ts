import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";
import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UMB_DOCUMENT_ENTITY_TYPE,
  UMB_DOCUMENT_WORKSPACE_CONTEXT,
} from "@umbraco-cms/backoffice/document";
import { UmbEntityUpdatedEvent } from "@umbraco-cms/backoffice/entity-action";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { SeoContentSavedEvent } from "../events/seoContentSavedEvent";

const INVARIANT_CULTURE = "invariant";

/**
 * Detects when the document in the workspace has been saved and tells the modules about
 * it, so each of them can persist their own settings without repeating the detection.
 *
 * Umbraco has no single client-side event for "this document was saved":
 * - Updates dispatch UmbEntityUpdatedEvent after every successful save, but it does not
 *   say which cultures were saved, so we report every culture and let the modules only
 *   persist what the editor changed.
 * - The first save of a new node dispatches no such event. A variant's update date
 *   changing is the signal there. It can't be used for updates: the server leaves the
 *   dates alone when nothing in the document itself changed, which is exactly the case
 *   when the editor only changed SEO settings.
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
      if (this.#actionEventContext || !instance) return;

      this.#actionEventContext = instance;
      instance.addEventListener(UmbEntityUpdatedEvent.TYPE, this.#onEntityUpdated);
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

          // Updates are reported through UmbEntityUpdatedEvent. The workspace stores the
          // created node before it stops being new, so this only lets the create through.
          if (!instance.getIsNew()) return;

          this.#dispatchSaved(savedCultures);
        },
        "stSeoContentData"
      );
    });
  }

  #onEntityUpdated = (event: Event) => {
    if (!(event instanceof UmbEntityUpdatedEvent)) return;
    if (event.getEntityType() !== UMB_DOCUMENT_ENTITY_TYPE) return;
    // The action event context is shared, so events for other documents reach us too.
    if (!this.#nodeId || event.getUnique() !== this.#nodeId) return;

    this.#dispatchSaved(Object.keys(this.#lastUpdated));
  };

  #dispatchSaved(cultures: string[]) {
    if (!this.#nodeId || cultures.length === 0) return;

    this.#actionEventContext?.dispatchEvent(
      new SeoContentSavedEvent({
        unique: this.#nodeId,
        cultures: cultures,
      })
    );
  }

  destroy(): void {
    super.destroy();
    this.#actionEventContext?.removeEventListener(
      UmbEntityUpdatedEvent.TYPE,
      this.#onEntityUpdated
    );
  }

  getEntityType(): string {
    return "st-seo-content";
  }
}

export const ST_SEO_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<SeoToolkitContentContext>("ST-SeoContent-Context");
