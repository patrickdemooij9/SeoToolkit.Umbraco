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
        this.#nodeId = unique.toString();
        this.#loadData();
        instance.getData()?.variants.forEach((variant) => {
          // Get the latest update data from the variants to compare with later when saving
          const updateDate = variant.updateDate;
          if (!this.#lastUpdateDate || (updateDate && this.#lastUpdateDate < updateDate)) {
            this.#lastUpdateDate = updateDate!;
          }
        });
      });

      this.observe(instance.data, (item) => {
        let shouldSave = false;        
        item?.variants.forEach((variant) => {
          const updateDate = variant.updateDate;
          if (this.#lastUpdateDate && updateDate && this.#lastUpdateDate < updateDate) {
            shouldSave = true;
            this.#lastUpdateDate = updateDate!;
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
    this.#repository.getContentSettings(this.#nodeId).then((resp) => {
      if (resp?.data) {
        this.#model.setValue(resp.data);
      }
    });
  }

  update(model: Partial<SitemapContentSettingsViewModel>) {
    this.#model.update(model);
  }

  save() {
    if (!this.#nodeId) return;
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
