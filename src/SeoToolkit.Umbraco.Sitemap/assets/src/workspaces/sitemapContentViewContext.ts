import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { SitemapContentSettingsViewModel } from "../api";
import { ContentSettingsRepository } from "../repositories/contentSettingsRepository";

const SEO_CONTENT_SAVED_EVENT_TYPE = "seo-content-saved";

interface SeoContentSavedEventDetail {
  unique: string;
  cultures: string[];
}

export default class SitemapContentViewContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #repository: ContentSettingsRepository;
  #actionEventContext?: UmbActionEventContext;
  #nodeId?: string;

  #loaded = false;
  #isDirty = false;
  #editVersion = 0;

  #model = new UmbObjectState<SitemapContentSettingsViewModel>({
    excludeFromSitemap: false,
  });
  public readonly model = this.#model.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_SITEMAP_CONTENT_TOKEN_CONTEXT.toString());

    this.#repository = new ContentSettingsRepository(host);

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      if (this.#actionEventContext || !instance) return;

      this.#actionEventContext = instance;
      instance.addEventListener(
        SEO_CONTENT_SAVED_EVENT_TYPE,
        this.#onContentSaved
      );
    });

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      if (!instance) return;

      this.observe(
        instance.unique,
        (unique) => {
          const nodeId = unique?.toString();
          if (!nodeId || nodeId === this.#nodeId) return;

          this.#nodeId = nodeId;
          this.#loaded = false;
          this.#isDirty = false;
          this.#editVersion++;
          this.#model.setValue({ excludeFromSitemap: false });
          this.#loadData(nodeId);
        },
        "stSitemapContentUnique"
      );
    });
  }

  #loadData(node: string) {
    this.#repository.getContentSettings(node).then((resp) => {
      // Ignore late responses if the user already navigated to another node.
      if (this.#nodeId !== node) return;
      if (!resp || resp.error) return;

      const data = resp.data ?? { excludeFromSitemap: false };
      this.#model.setValue(data);
      this.#loaded = true;
      this.#isDirty = false;
    });
  }

  #onContentSaved = (event: Event) => {
    const detail = (event as CustomEvent<SeoContentSavedEventDetail>).detail;
    // The action event context is shared, so events for other documents reach us too.
    if (!detail || detail.unique !== this.#nodeId) return;

    this.save();
  };

  update(model: Partial<SitemapContentSettingsViewModel>) {
    this.#isDirty = true;
    this.#editVersion++;
    this.#model.update(model);
  }

  async save() {
    const node = this.#nodeId;
    if (!node || !this.#loaded || !this.#isDirty) return;

    const settings = this.#model.getValue();
    const editVersionAtSave = this.#editVersion;

    const resp = await this.#repository.setContentSettings({
      nodeKey: node,
      excludeFromSitemap: settings.excludeFromSitemap,
      changeFrequency: settings.changeFrequency,
      priority: settings.priority,
    });

    if (!resp || resp.error) return;
    if (this.#nodeId !== node) return;
    if (this.#editVersion !== editVersionAtSave) return;

    this.#isDirty = false;
  }

  destroy(): void {
    super.destroy();
    this.#actionEventContext?.removeEventListener(
      SEO_CONTENT_SAVED_EVENT_TYPE,
      this.#onContentSaved
    );
  }

  getEntityType(): string {
    return "st-sitemap-content";
  }
}

export const ST_SITEMAP_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<SitemapContentViewContext>("ST-SitemapContent-Context");
