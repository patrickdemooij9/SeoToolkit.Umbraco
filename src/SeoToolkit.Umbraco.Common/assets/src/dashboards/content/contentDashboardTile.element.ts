import { LitElement, PropertyValues, css, html, customElement, property, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import { createExtensionElement } from "@umbraco-cms/backoffice/extension-api";
import { DashboardData } from "../../api";
import { DashboardTileRepository } from "../../repositories/dashboardTileRepository";
import {
  ManifestSeoDashboardTile,
  ManifestSeoDashboardTileView,
  SeoDashboardTileSize,
  SeoDashboardTileViewElement,
} from "../../manifests/seoDashboardTileManifest";

/**
 * Loads the data of one tile from its data source and renders it with the tile's view.
 */
@customElement("seo-content-dashboard-tile")
export class ContentDashboardTileElement extends UmbElementMixin(LitElement) {
  @property({ attribute: false }) manifest?: ManifestSeoDashboardTile;
  @property({ attribute: false }) size?: SeoDashboardTileSize;

  @state() private data?: DashboardData;
  @state() private failed = false;
  @state() private view?: SeoDashboardTileViewElement;

  #repository = new DashboardTileRepository(this);

  protected willUpdate(changed: PropertyValues<this>) {
    if (changed.has("manifest") && this.manifest) {
      this.#loadData(this.manifest);
      this.#loadView(this.manifest.meta.view);
    }
  }

  protected updated() {
    if (!this.view || !this.data) return;
    this.view.data = this.data;
    this.view.settings = this.manifest?.meta.settings;
    this.view.size = this.size;
  }

  async #loadData(manifest: ManifestSeoDashboardTile) {
    const { data, error } = await this.#repository.getData(manifest.meta.dataSource, manifest.meta.parameters);
    this.failed = !!error;
    this.data = data;
  }

  #loadView(alias: string) {
    this.observe(
      umbExtensionsRegistry.byAlias<ManifestSeoDashboardTileView>(alias),
      async (viewManifest) => {
        this.view = viewManifest ? await createExtensionElement(viewManifest) : undefined;
      },
      "_observeView"
    );
  }

  render() {
    if (this.failed) return html`<p class="message">Could not load this tile.</p>`;
    if (!this.data || !this.view) return html`<uui-loader-bar></uui-loader-bar>`;
    return this.view;
  }

  static styles = css`
    :host {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-height: 0;
    }
    .message {
      margin: 0;
      color: var(--uui-color-text-alt);
    }
  `;
}

export default ContentDashboardTileElement;

declare global {
  interface HTMLElementTagNameMap {
    "seo-content-dashboard-tile": ContentDashboardTileElement;
  }
}
