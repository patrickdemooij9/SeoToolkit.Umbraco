import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { SitemapContentSettingsViewModel } from "../api";
import { ContentSettingsRepository } from "../repositories/contentSettingsRepository";

export default class SitemapContentViewContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #repository: ContentSettingsRepository;
  #nodeId?: string;
  #lastUpdateDate?: string;
  // Guards against posting before the current node's settings have loaded.
  // Without this, a save triggered right after navigation would persist the
  // previous page's model (overwriting the new node) or post empty defaults
  // (which the backend treats as "default" and deletes the existing row).
  #loaded = false;

  #model = new UmbObjectState<SitemapContentSettingsViewModel>({
    excludeFromSitemap: false,
  });
  public readonly model = this.#model.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_SITEMAP_CONTENT_TOKEN_CONTEXT.toString());

    this.#repository = new ContentSettingsRepository(host);

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      if (!instance) return;

      this.observe(instance.unique, (unique) => {
        if (!unique) return;
        // The document workspace context is reused across navigation, so reset
        // all per-node state before loading the new node's settings.
        this.#nodeId = unique.toString();
        this.#lastUpdateDate = undefined;
        this.#loaded = false;
        this.#model.setValue({ excludeFromSitemap: false });
        this.#loadData();
      });

      this.observe(instance.data, (item) => {
        let shouldSave = false;
        item?.variants.forEach((variant) => {
          const updateDate = variant.updateDate;
          // The document was saved when a variant's update date changes.
          // Track the latest value each time (not a monotonic max) so the
          // detection keeps working after navigating between pages.
          if (this.#lastUpdateDate && updateDate && this.#lastUpdateDate !== updateDate) {
            shouldSave = true;
          }
          if (updateDate) {
            this.#lastUpdateDate = updateDate;
          }
        });
        if (shouldSave) {
          this.save();
        }
      });
    });
  }

  #loadData() {
    if (!this.#nodeId) return;
    const node = this.#nodeId;
    this.#repository.getContentSettings(node).then((resp) => {
      // Ignore late responses if the user already navigated to another node.
      if (this.#nodeId !== node) return;
      if (resp?.data) {
        this.#model.setValue(resp.data);
      }
      this.#loaded = true;
    });
  }

  update(model: Partial<SitemapContentSettingsViewModel>) {
    this.#model.update(model);
  }

  save() {
    // Don't persist until this node's settings have loaded, otherwise we'd
    // write stale/default values over the node's real settings.
    if (!this.#nodeId || !this.#loaded) return;
    const value = this.#model.getValue();
    this.#repository.setContentSettings({
      nodeKey: this.#nodeId,
      excludeFromSitemap: value.excludeFromSitemap ?? false,
      changeFrequency: value.changeFrequency,
      priority: value.priority,
    });
  }

  getEntityType(): string {
    return "st-sitemap-content";
  }
}

export const ST_SITEMAP_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<SitemapContentViewContext>("ST-SitemapContent-Context");
