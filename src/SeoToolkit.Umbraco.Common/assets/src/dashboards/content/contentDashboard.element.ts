import { LitElement, css, html, customElement, state, when } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UmbExtensionsManifestInitializer } from "@umbraco-cms/backoffice/extension-api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import { ContentDashboardRepository } from "../../repositories/contentDashboardRepository";
import { ManifestSeoDashboardTile } from "../../manifests/seoDashboardTileManifest";
import { ContentDashboardLayout, isDefaultLayout, normalizeLayout, resolveLayout } from "./contentDashboardLayout";
import "./contentDashboardTileGrid.element";

const SAVE_DELAY = 500;

@customElement("seo-content-dashboard")
export class ContentDashboardElement extends UmbElementMixin(LitElement) {
  // Tiles whose conditions are met, e.g. only the tiles of enabled modules.
  @state() private tiles: ManifestSeoDashboardTile[] = [];
  // The user's own layout; undefined while they follow the default.
  @state() private userLayout?: ContentDashboardLayout;
  @state() private loaded = false;

  #repository = new ContentDashboardRepository(this);
  #saveTimer?: number;

  constructor() {
    super();
    new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, "seoToolkitDashboardTile", null, (tiles) => {
      this.tiles = tiles.map((tile) => tile.manifest as unknown as ManifestSeoDashboardTile);
    });
  }

  connectedCallback() {
    super.connectedCallback();
    this.#repository.getLayout().then((layout) => {
      this.userLayout = layout;
      this.loaded = true;
    });
  }

  disconnectedCallback() {
    // Save a pending change right away instead of dropping it.
    if (this.#saveTimer) {
      window.clearTimeout(this.#saveTimer);
      this.#save();
    }
    super.disconnectedCallback();
  }

  #onLayoutChange(layout: ContentDashboardLayout | undefined) {
    this.userLayout = layout && !isDefaultLayout(layout, this.tiles) ? normalizeLayout(layout, this.tiles) : undefined;
    window.clearTimeout(this.#saveTimer);
    this.#saveTimer = window.setTimeout(() => this.#save(), SAVE_DELAY);
  }

  #save() {
    this.#saveTimer = undefined;
    this.#repository.saveLayout(this.userLayout);
  }

  render() {
    return html`
      <div class="dashboard">
        ${when(
          this.loaded,
          () => html`<seo-content-dashboard-tile-grid
            .tiles=${this.tiles}
            .layout=${resolveLayout(this.userLayout, this.tiles)}
            @layout-change=${(e: CustomEvent<ContentDashboardLayout>) => this.#onLayoutChange(e.detail)}
            @layout-reset=${() => this.#onLayoutChange(undefined)}></seo-content-dashboard-tile-grid>`,
          () => html`<uui-loader></uui-loader>`
        )}
      </div>
    `;
  }

  static styles = css`
    .dashboard {
      padding: var(--uui-size-layout-1);
    }
  `;
}

export default ContentDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    "seo-content-dashboard": ContentDashboardElement;
  }
}
