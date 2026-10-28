// The SEO tiles on a GridStack grid (12 columns). "Customize" makes the grid editable and slides in a sidebar with
// the tiles that aren't on the dashboard: drag them anywhere onto the grid, drag tiles back onto the sidebar to
// remove them, move tiles freely and resize them from their edges.
import { LitElement, PropertyValues, css, html, customElement, property, state, when, render, unsafeCSS } from "@umbraco-cms/backoffice/external/lit";
import { GridStack, GridItemHTMLElement, GridStackWidget } from "gridstack";
import gridstackStyles from "gridstack/dist/gridstack.min.css?inline";
import { ManifestSeoDashboardTile, SeoDashboardTileSize } from "../../manifests/seoDashboardTileManifest";
import { ContentDashboardLayout, GRID_COLUMNS, TilePosition, defaultTileSize } from "./contentDashboardLayout";
import "./contentDashboardTile.element";

const ROW_HEIGHT = 80;
const MARGIN = 8;
const MIN_COLUMN_WIDTH = 70;
const ACTION_FROM_HEIGHT = 4;
const SYNC_DELAY = 100;

@customElement("seo-content-dashboard-tile-grid")
export class ContentDashboardTileGridElement extends LitElement {
  @property({ attribute: false }) tiles: ManifestSeoDashboardTile[] = [];
  /** The user's layout; undefined means the default (all tiles, placed in weight order). */
  @property({ attribute: false }) layout?: ContentDashboardLayout;

  @state() private editing = false;
  @state() private overSidebar = false;

  #grid?: GridStack;
  // The grid items by tile alias. GridStack owns these elements; Lit only renders their content.
  #items = new Map<string, GridItemHTMLElement>();
  // The last layout this grid reported, so it doesn't rebuild when that layout comes back in.
  // Starts as null (never a valid layout), so a reset to the default (undefined) always rebuilds.
  #emitted: ContentDashboardLayout | null = null;
  #syncTimer?: number;
  // The grid at the start of a drag or resize.
  #before?: string;

  #trackPointer = (e: PointerEvent) => {
    const rect = this.renderRoot.querySelector(".sidebar")!.getBoundingClientRect();
    this.overSidebar =
      this.editing && e.clientX >= rect.left && e.clientX <= rect.right && e.clientY >= rect.top && e.clientY <= rect.bottom;
  };

  get #gridEl() {
    return this.renderRoot.querySelector<HTMLElement>(".grid-stack")!;
  }

  disconnectedCallback() {
    document.removeEventListener("pointermove", this.#trackPointer);
    window.clearTimeout(this.#syncTimer);
    this.#grid?.destroy(false);
    this.#grid = undefined;
    this.#items.clear();
    this.#gridEl?.replaceChildren();
    super.disconnectedCallback();
  }

  protected updated(changed: PropertyValues) {
    if (!this.#grid) {
      this.#init();
      this.#scheduleSync();
    } else if (changed.has("layout") && this.layout !== this.#emitted) {
      this.#sync(true);
    } else if (changed.has("tiles")) {
      this.#scheduleSync();
    }

    if (changed.has("editing")) this.#grid!.setStatic(!this.editing);
    if (this.editing) this.#setupDragIn();
    this.#renderItems();
  }

  #init() {
    const grid = GridStack.init(
      {
        column: GRID_COLUMNS,
        cellHeight: ROW_HEIGHT,
        margin: MARGIN,
        mode: "float",
        staticGrid: !this.editing,
        acceptWidgets: (el) => el.classList.contains("catalog-item"),
        resizable: { handles: "e,se,s,sw,w" },
        // Fewer columns on narrow screens; positions are still saved for 12 columns.
        columnOpts: { columnWidth: MIN_COLUMN_WIDTH, columnMax: GRID_COLUMNS },
      },
      this.#gridEl
    )!;

    // Only user actions are saved, not GridStack reflowing the grid by itself (e.g. on a column change),
    // and only when the drag or resize actually changed something.
    grid.on("dragstart", () => {
      this.#before = this.#snapshot();
      document.addEventListener("pointermove", this.#trackPointer);
    });
    grid.on("dragstop", (_event, el) => {
      document.removeEventListener("pointermove", this.#trackPointer);
      if (this.overSidebar) this.#removeTile(String(el.gridstackNode?.id));
      this.overSidebar = false;
      setTimeout(() => this.#emitLayoutIfChanged());
    });
    grid.on("resizestart", () => (this.#before = this.#snapshot()));
    grid.on("resize", () => this.#renderItems());
    grid.on("resizestop", () => setTimeout(() => this.#emitLayoutIfChanged()));
    grid.on("change", () => this.#renderItems());
    // A tile dragged in from the sidebar arrives as the drag helper; swap it for a real tile at the same spot.
    grid.on("dropped", (_event, _previous, node) => {
      const tile = this.tiles.find((t) => t.alias === String(node.id));
      grid.removeWidget(node.el!, true, false);
      if (tile) this.#addTile(tile, { x: node.x ?? 0, y: node.y ?? 0, w: node.w ?? 1, h: node.h ?? 1 });
      this.#emitLayout();
    });

    this.#grid = grid;
  }

  // Tiles arrive in batches (as each module's "enabled" condition resolves), so wait for them to settle. On the
  // default layout the grid is rebuilt, so the tiles are always placed in weight order whatever loaded first.
  #scheduleSync() {
    window.clearTimeout(this.#syncTimer);
    this.#syncTimer = window.setTimeout(() => this.#sync(!this.layout), SYNC_DELAY);
  }

  // Puts the visible tiles on the grid. `reset` rebuilds it, e.g. after "Reset to default".
  #sync(reset: boolean) {
    const grid = this.#grid;
    if (!grid) return;
    const hidden = this.layout?.hidden ?? [];
    const positions = this.layout?.positions ?? {};
    const visible = this.tiles.filter((tile) => !hidden.includes(tile.alias));

    if (reset) {
      grid.removeAll(true, false);
      this.#items.clear();
    }
    for (const [alias, el] of this.#items) {
      if (!visible.some((tile) => tile.alias === alias)) {
        grid.removeWidget(el, true, false);
        this.#items.delete(alias);
      }
    }
    // Tiles with a position go first, so tiles without one (new tiles, or all of them by default) fill the free space.
    // Added one by one (not batched), so each auto-positioned tile takes the first free spot in weight order.
    visible
      .filter((tile) => !this.#items.has(tile.alias))
      .sort((a, b) => Number(!positions[a.alias]) - Number(!positions[b.alias]))
      .forEach((tile) => this.#addTile(tile, positions[tile.alias]));
    this.requestUpdate();
  }

  #addTile(tile: ManifestSeoDashboardTile, position?: TilePosition) {
    const el = document.createElement("div");
    el.className = "grid-stack-item";
    el.innerHTML = `<div class="grid-stack-item-content"></div>`;
    this.#gridEl.append(el);

    const widget: GridStackWidget = position
      ? { id: tile.alias, ...position }
      : { id: tile.alias, ...defaultTileSize(tile), autoPosition: true };
    this.#items.set(tile.alias, this.#grid!.makeWidget(el, widget));
  }

  #removeTile(alias: string) {
    const el = this.#items.get(alias);
    if (!el) return;
    this.#grid!.removeWidget(el, true, false);
    this.#items.delete(alias);
  }

  #add(tile: ManifestSeoDashboardTile) {
    this.#addTile(tile);
    this.#emitLayout();
  }

  #snapshot() {
    return JSON.stringify(this.#grid?.save(false, false, undefined, GRID_COLUMNS));
  }

  #emitLayoutIfChanged() {
    if (this.#snapshot() !== this.#before) this.#emitLayout();
    this.#renderItems();
  }

  #emitLayout() {
    if (!this.#grid) return;
    const known = new Set(this.tiles.map((tile) => tile.alias));
    const positions: Record<string, TilePosition> = {};
    // Keep the place of tiles that aren't available right now (e.g. of a disabled module).
    for (const [alias, position] of Object.entries(this.layout?.positions ?? {})) {
      if (!known.has(alias)) positions[alias] = position;
    }
    for (const widget of this.#grid.save(false, false, undefined, GRID_COLUMNS) as GridStackWidget[]) {
      if (widget.id == null) continue;
      positions[String(widget.id)] = { x: widget.x ?? 0, y: widget.y ?? 0, w: widget.w ?? 1, h: widget.h ?? 1 };
    }
    const hidden = [
      ...this.tiles.map((tile) => tile.alias).filter((alias) => !this.#items.has(alias)),
      ...(this.layout?.hidden ?? []).filter((alias) => !known.has(alias)),
    ];

    this.#emitted = { positions, hidden };
    this.dispatchEvent(new CustomEvent("layout-change", { detail: this.#emitted }));
    this.requestUpdate();
  }

  // ---- sidebar → grid ----

  #setupDragIn() {
    const items = [...this.renderRoot.querySelectorAll<HTMLElement>(".catalog-item")];
    if (items.length) GridStack.setupDragIn(items, { appendTo: "body", helper: (el) => this.#dragHelper(el) });
  }

  // The helper lives in the document body (outside this element's styles), so it is styled inline.
  #dragHelper(el: HTMLElement) {
    const helper = document.createElement("div");
    for (const attribute of ["gs-id", "gs-w", "gs-h"]) helper.setAttribute(attribute, el.getAttribute(attribute) ?? "");
    helper.textContent = el.dataset.title ?? "";
    Object.assign(helper.style, {
      padding: "8px 12px",
      background: "#fff",
      border: "1px solid #d8d7d9",
      borderRadius: "6px",
      boxShadow: "0 6px 16px rgba(0, 0, 0, 0.2)",
      font: "600 13px/1.2 sans-serif",
      zIndex: "10000",
    });
    return helper;
  }

  // ---- rendering ----

  #renderItems() {
    for (const [alias, el] of this.#items) {
      const tile = this.tiles.find((t) => t.alias === alias);
      const content = el.querySelector<HTMLElement>(".grid-stack-item-content");
      if (!tile || !content) continue;
      const size = { w: el.gridstackNode?.w ?? 1, h: el.gridstackNode?.h ?? 1 };
      render(this.#renderTile(tile, size), content, { host: this });
    }
  }

  #renderTile(tile: ManifestSeoDashboardTile, size: SeoDashboardTileSize) {
    const action = tile.meta.action;
    return html`
      <div class="tile">
        <div class="headline">
          <uui-icon name=${tile.meta.icon}></uui-icon>
          <span class="title">${tile.meta.title}</span>
          ${when(
            this.editing,
            () => html`<button
              class="remove"
              title="Remove"
              @click=${() => {
                this.#removeTile(tile.alias);
                this.#emitLayout();
              }}>
              ✕
            </button>`
          )}
        </div>
        <seo-content-dashboard-tile .manifest=${tile} .size=${size}></seo-content-dashboard-tile>
        ${when(
          action && size.h >= ACTION_FROM_HEIGHT,
          () => html`<uui-button class="action" look="secondary" compact href=${action!.href}>${action!.label}</uui-button>`
        )}
        ${when(this.editing, () => html`<div class="size-label">${size.w} × ${size.h}</div>`)}
      </div>
    `;
  }

  render() {
    const available = this.tiles.filter((tile) => !this.#items.has(tile.alias));

    return html`
      <div class="toolbar">
        <h2>SEO overview</h2>
        <div class="toolbar-actions">
          ${when(
            this.editing && this.layout,
            () => html`<uui-button look="secondary" @click=${() => this.dispatchEvent(new CustomEvent("layout-reset"))}>
              Reset to default
            </uui-button>`
          )}
          <uui-button look=${this.editing ? "primary" : "secondary"} @click=${() => (this.editing = !this.editing)}>
            <uui-icon name="icon-settings"></uui-icon> ${this.editing ? "Done" : "Customize"}
          </uui-button>
        </div>
      </div>

      <div class="workspace ${this.editing ? "editing" : ""}">
        <div class="grid-stack"></div>

        <aside class="sidebar ${this.overSidebar ? "active" : ""}">
          <h3>Tiles</h3>
          <p class="hint">Drag a tile onto the dashboard. Drag a dashboard tile here to remove it.</p>
          ${when(!available.length, () => html`<p class="hint">All tiles are on the dashboard.</p>`)}
          ${available.map((tile) => {
            const size = defaultTileSize(tile);
            return html`<div class="catalog-item" gs-id=${tile.alias} gs-w=${size.w} gs-h=${size.h} data-title=${tile.meta.title}>
              <uui-icon name=${tile.meta.icon}></uui-icon>
              <div>
                <strong>${tile.meta.title}</strong>
                <p>${tile.meta.description}</p>
              </div>
              <uui-button compact label="Add" @click=${() => this.#add(tile)}><uui-icon name="icon-add"></uui-icon></uui-button>
            </div>`;
          })}
        </aside>
      </div>
    `;
  }

  static styles = [
    unsafeCSS(gridstackStyles),
    css`
      .toolbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--uui-size-space-4);
      }
      h2 {
        margin: 0;
      }
      .toolbar-actions {
        display: flex;
        gap: var(--uui-size-space-2);
      }
      .workspace {
        display: flex;
        align-items: flex-start;
      }
      .grid-stack {
        flex: 1;
        min-width: 0;
        margin: calc(-1 * ${MARGIN}px);
      }
      .grid-stack-item-content {
        display: flex;
      }
      .tile {
        position: relative;
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-3);
        min-width: 0;
        overflow: hidden;
        padding: var(--uui-size-space-4) var(--uui-size-space-5);
        background: var(--uui-color-surface);
        border-radius: var(--uui-border-radius);
        box-shadow: var(--uui-shadow-depth-1);
      }
      .editing .tile {
        cursor: grab;
        outline: 2px dashed var(--uui-color-border-emphasis);
      }
      .editing .ui-draggable-dragging .tile,
      .editing .ui-resizable-resizing .tile {
        outline: 2px solid var(--uui-color-interactive-emphasis);
      }
      .grid-stack-placeholder > .placeholder-content {
        border: 2px dashed var(--uui-color-interactive-emphasis);
        border-radius: var(--uui-border-radius);
        background: transparent;
      }
      .headline {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-2);
        font-weight: 700;
      }
      .headline .title {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      .remove {
        all: unset;
        cursor: pointer;
        margin-left: auto;
        padding: 0 4px;
        color: var(--uui-color-text-alt);
      }
      .action {
        align-self: flex-start;
      }
      .size-label {
        position: absolute;
        right: 16px;
        bottom: 6px;
        font-size: 11px;
        color: var(--uui-color-text-alt);
      }
      .sidebar {
        position: sticky;
        top: 0;
        flex: none;
        width: 0;
        max-height: calc(100vh - 200px);
        overflow: hidden auto;
        scrollbar-width: thin;
        scrollbar-color: var(--uui-color-divider-emphasis) transparent;
        opacity: 0;
        margin-left: 0;
        padding: 0;
        background: var(--uui-color-surface);
        border-radius: var(--uui-border-radius);
        box-shadow: var(--uui-shadow-depth-3);
        transition: width 200ms ease, opacity 200ms ease, margin 200ms ease;
      }
      .editing .sidebar {
        width: 320px;
        opacity: 1;
        margin-left: var(--uui-size-space-5);
        padding: var(--uui-size-space-4);
        box-sizing: border-box;
      }
      .sidebar.active {
        outline: 2px dashed var(--uui-color-danger);
      }
      .sidebar h3 {
        margin: 0 0 var(--uui-size-space-2);
      }
      .hint {
        color: var(--uui-color-text-alt);
        font-size: 12px;
      }
      .catalog-item {
        display: flex;
        gap: var(--uui-size-space-3);
        align-items: flex-start;
        padding: var(--uui-size-space-3);
        margin-bottom: var(--uui-size-space-2);
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-border-radius);
        cursor: grab;
        background: var(--uui-color-surface);
      }
      .catalog-item:hover {
        border-color: var(--uui-color-border-emphasis);
      }
      .catalog-item uui-icon {
        margin-top: 3px;
      }
      .catalog-item p {
        margin: 4px 0 0;
        font-size: 12px;
        color: var(--uui-color-text-alt);
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "seo-content-dashboard-tile-grid": ContentDashboardTileGridElement;
  }
}
