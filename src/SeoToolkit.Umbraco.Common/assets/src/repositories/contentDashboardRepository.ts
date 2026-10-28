import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { ContentDashboardSource } from "../sources/contentDashboardSource";
import { ContentDashboardLayout } from "../dashboards/content/contentDashboardLayout";

export class ContentDashboardRepository extends UmbControllerBase {
  #source: ContentDashboardSource;
  // Key of the stored layout. Unset while the user is on the default layout.
  #layoutKey?: string;

  constructor(host: UmbControllerHost) {
    super(host);
    this.#source = new ContentDashboardSource(this);
  }

  // Returns undefined when the user hasn't customized the dashboard.
  async getLayout(): Promise<ContentDashboardLayout | undefined> {
    const { data } = await this.#source.getLayout();
    const stored = data?.items[0];
    this.#layoutKey = stored?.key;
    if (!stored) return undefined;

    try {
      return JSON.parse(stored.value);
    } catch {
      return undefined;
    }
  }

  // Pass undefined when the user is back on the default layout: the stored copy is removed, so they keep following the default.
  async saveLayout(layout: ContentDashboardLayout | undefined) {
    if (!layout) {
      if (!this.#layoutKey) return;
      const { error } = await this.#source.deleteLayout(this.#layoutKey);
      if (!error) this.#layoutKey = undefined;
      return;
    }

    const value = JSON.stringify(layout);
    if (this.#layoutKey) {
      await this.#source.updateLayout(this.#layoutKey, value);
      return;
    }

    const key = crypto.randomUUID();
    const { error } = await this.#source.createLayout(key, value);
    if (!error) this.#layoutKey = key;
  }
}
