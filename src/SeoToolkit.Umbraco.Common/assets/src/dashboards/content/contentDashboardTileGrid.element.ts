// A 12-column grid of SEO tiles. "Customize" slides in a sidebar with the tiles that aren't on the dashboard:
// drag them onto the grid (or back to remove them), drag tiles to reorder, and drag the bottom-right corner to resize.
import { LitElement, css, html, customElement, property, state, repeat, when } from "@umbraco-cms/backoffice/external/lit";
import { ManifestSeoDashboardTile, SeoDashboardTileSize } from "../../manifests/seoDashboardTileManifest";
import { ContentDashboardLayout, GRID_COLUMNS, isDefaultLayout, tileSize } from "./contentDashboardLayout";
import "./contentDashboardTile.element";

const ROW_HEIGHT = 64;
const GAP = 16;
const MAX_ROWS = 10;
const MIN_COLUMN_WIDTH = 70;
const ACTION_FROM_HEIGHT = 4;

type DragPayload = { kind: "new" | "move"; alias: string };

@customElement("seo-content-dashboard-tile-grid")
export class ContentDashboardTileGridElement extends LitElement {
  @property({ attribute: false }) tiles: ManifestSeoDashboardTile[] = [];
  @property({ attribute: false }) layout!: ContentDashboardLayout;

  @state() private editing = false;
  @state() private dropTarget?: string;
  @state() private resizing?: { alias: string; size: SeoDashboardTileSize };
  // Narrow screens (or an open sidebar) get fewer columns so tiles don't collapse into slivers.
  @state() private columns = GRID_COLUMNS;
  #drag?: DragPayload;
  #observer = new ResizeObserver(([entry]) => {
    const fit = Math.floor((entry.contentRect.width + GAP) / (MIN_COLUMN_WIDTH + GAP));
    this.columns = Math.max(4, Math.min(GRID_COLUMNS, fit));
  });

  firstUpdated() {
    this.#observer.observe(this.renderRoot.querySelector(".grid")!);
  }

  disconnectedCallback() {
    this.#observer.disconnect();
    super.disconnectedCallback();
  }

  #emit(layout: ContentDashboardLayout) {
    this.dispatchEvent(new CustomEvent("layout-change", { detail: layout }));
  }

  #sizeOf(tile: ManifestSeoDashboardTile) {
    if (this.resizing?.alias === tile.alias) return this.resizing.size;
    return tileSize(this.layout, tile);
  }

  // ---- drag & drop (sidebar → grid, grid → grid, grid → sidebar) ----

  #dragStart(e: DragEvent, payload: DragPayload) {
    this.#drag = payload;
    e.dataTransfer!.effectAllowed = "move";
    e.dataTransfer!.setData("text/plain", payload.alias);
  }

  // `target` is a tile alias, "end" (the bottom of the grid) or "sidebar".
  #dragOver(e: DragEvent, target: string) {
    if (!this.#drag) return;
    if (target === "sidebar" && this.#drag.kind === "new") return;
    e.preventDefault();
    e.stopPropagation();
    this.dropTarget = target;
  }

  #drop(e: DragEvent) {
    e.preventDefault();
    e.stopPropagation();
    const drag = this.#drag;
    const target = this.dropTarget;
    this.#dragEnd();
    if (!drag || !target || drag.alias === target) return;

    if (target === "sidebar") {
      this.#remove(drag.alias);
      return;
    }
    const order = this.layout.order.filter((alias) => alias !== drag.alias);
    const index = target === "end" ? order.length : order.indexOf(target);
    order.splice(index, 0, drag.alias);
    this.#emit({ ...this.layout, order, hidden: this.layout.hidden.filter((alias) => alias !== drag.alias) });
  }

  #dragEnd() {
    this.#drag = undefined;
    this.dropTarget = undefined;
  }

  #add(alias: string) {
    const order = [...this.layout.order.filter((o) => o !== alias), alias];
    this.#emit({ ...this.layout, order, hidden: this.layout.hidden.filter((h) => h !== alias) });
  }

  #remove(alias: string) {
    this.#emit({ ...this.layout, hidden: [...this.layout.hidden, alias] });
  }

  // ---- free resize via the corner handle ----

  #resizeStart(e: PointerEvent, tile: ManifestSeoDashboardTile) {
    e.preventDefault();
    e.stopPropagation();
    const handle = e.currentTarget as HTMLElement;
    handle.setPointerCapture(e.pointerId);

    const grid = this.renderRoot.querySelector(".grid") as HTMLElement;
    const columnWidth = (grid.clientWidth - GAP * (this.columns - 1)) / this.columns;
    const start = { x: e.clientX, y: e.clientY, ...this.#sizeOf(tile) };
    this.resizing = { alias: tile.alias, size: { w: start.w, h: start.h } };

    const move = (ev: PointerEvent) => {
      const w = Math.round(start.w + (ev.clientX - start.x) / (columnWidth + GAP));
      const h = Math.round(start.h + (ev.clientY - start.y) / (ROW_HEIGHT + GAP));
      this.resizing = {
        alias: tile.alias,
        size: { w: Math.min(GRID_COLUMNS, Math.max(2, w)), h: Math.min(MAX_ROWS, Math.max(1, h)) },
      };
    };
    const up = () => {
      handle.removeEventListener("pointermove", move);
      handle.removeEventListener("pointerup", up);
      const size = this.resizing!.size;
      this.resizing = undefined;
      this.#emit({ ...this.layout, sizes: { ...this.layout.sizes, [tile.alias]: size } });
    };
    handle.addEventListener("pointermove", move);
    handle.addEventListener("pointerup", up);
  }

  render() {
    const byAlias = new Map(this.tiles.map((tile) => [tile.alias, tile]));
    const visible = this.layout.order.filter((alias) => !this.layout.hidden.includes(alias) && byAlias.has(alias));
    const available = this.tiles.filter((tile) => !visible.includes(tile.alias));

    return html`
      <div class="toolbar">
        <h2>SEO overview</h2>
        <div class="toolbar-actions">
          ${when(
            this.editing && !isDefaultLayout(this.layout, this.tiles),
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
        <div
          class="grid"
          style="--columns:${this.columns};--row:${ROW_HEIGHT}px;--gap:${GAP}px"
          @dragover=${(e: DragEvent) => this.#dragOver(e, "end")}
          @drop=${this.#drop}>
          ${repeat(visible, (alias) => alias, (alias) => this.#renderTile(byAlias.get(alias)!))}
          ${when(
            this.editing,
            () => html`<div class="drop-end ${this.dropTarget === "end" ? "active" : ""}">Drop here to add at the end</div>`
          )}
        </div>

        <aside
          class="sidebar ${this.dropTarget === "sidebar" ? "active" : ""}"
          @dragover=${(e: DragEvent) => this.#dragOver(e, "sidebar")}
          @drop=${this.#drop}>
          <h3>Tiles</h3>
          <p class="hint">Drag a tile onto the dashboard. Drag a dashboard tile here to remove it.</p>
          ${when(!available.length, () => html`<p class="hint">All tiles are on the dashboard.</p>`)}
          ${available.map(
            (tile) => html`<div
              class="catalog-item"
              draggable="true"
              @dragstart=${(e: DragEvent) => this.#dragStart(e, { kind: "new", alias: tile.alias })}
              @dragend=${this.#dragEnd}>
              <uui-icon name=${tile.meta.icon}></uui-icon>
              <div>
                <strong>${tile.meta.title}</strong>
                <p>${tile.meta.description}</p>
              </div>
              <uui-button compact label="Add" @click=${() => this.#add(tile.alias)}><uui-icon name="icon-add"></uui-icon></uui-button>
            </div>`
          )}
        </aside>
      </div>
    `;
  }

  #renderTile(tile: ManifestSeoDashboardTile) {
    const size = this.#sizeOf(tile);
    const action = tile.meta.action;
    return html`
      <div
        class="tile ${this.dropTarget === tile.alias ? "drop-before" : ""} ${this.resizing?.alias === tile.alias ? "resizing" : ""}"
        style="grid-column: span ${Math.min(size.w, this.columns)}; grid-row: span ${size.h}"
        draggable=${this.editing ? "true" : "false"}
        @dragstart=${(e: DragEvent) => this.#dragStart(e, { kind: "move", alias: tile.alias })}
        @dragend=${this.#dragEnd}
        @dragover=${(e: DragEvent) => this.#dragOver(e, tile.alias)}
        @drop=${this.#drop}>
        <div class="headline">
          <uui-icon name=${tile.meta.icon}></uui-icon>
          <span class="title">${tile.meta.title}</span>
          ${when(this.editing, () => html`<button class="remove" title="Remove" @click=${() => this.#remove(tile.alias)}>✕</button>`)}
        </div>
        <seo-content-dashboard-tile .manifest=${tile} .size=${size}></seo-content-dashboard-tile>
        ${when(
          action && size.h >= ACTION_FROM_HEIGHT,
          () => html`<uui-button class="action" look="secondary" compact href=${action!.href}>${action!.label}</uui-button>`
        )}
        ${when(
          this.editing,
          () => html`<div class="size-label">${size.w} × ${size.h}</div>
            <div class="resize" title="Drag to resize" @pointerdown=${(e: PointerEvent) => this.#resizeStart(e, tile)}></div>`
        )}
      </div>
    `;
  }

  static styles = css`
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
    .grid {
      flex: 1;
      min-width: 0;
      display: grid;
      grid-template-columns: repeat(var(--columns), minmax(0, 1fr));
      grid-auto-rows: var(--row);
      grid-auto-flow: dense;
      gap: var(--gap);
    }
    .tile {
      position: relative;
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-3);
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
    .tile.drop-before {
      box-shadow: -6px 0 0 0 var(--uui-color-interactive-emphasis);
    }
    .tile.resizing {
      outline: 2px solid var(--uui-color-interactive-emphasis);
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
      right: 24px;
      bottom: 6px;
      font-size: 11px;
      color: var(--uui-color-text-alt);
    }
    .resize {
      position: absolute;
      right: 0;
      bottom: 0;
      width: 18px;
      height: 18px;
      cursor: nwse-resize;
      touch-action: none;
      background: linear-gradient(135deg, transparent 50%, var(--uui-color-border-emphasis) 50%);
      border-bottom-right-radius: var(--uui-border-radius);
    }
    .drop-end {
      grid-column: 1 / -1;
      display: grid;
      place-items: center;
      border: 2px dashed var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      color: var(--uui-color-text-alt);
    }
    .drop-end.active {
      border-color: var(--uui-color-interactive-emphasis);
      color: var(--uui-color-interactive-emphasis);
    }
    .sidebar {
      position: sticky;
      top: 0;
      flex: none;
      width: 0;
      max-height: calc(100vh - 200px);
      overflow: hidden auto;
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
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    "seo-content-dashboard-tile-grid": ContentDashboardTileGridElement;
  }
}
