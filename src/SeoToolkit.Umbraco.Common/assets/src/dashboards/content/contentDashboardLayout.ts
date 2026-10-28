import { ManifestSeoDashboardTile, SeoDashboardTileSize } from "../../manifests/seoDashboardTileManifest";

/**
 * How a user arranged the dashboard, keyed by tile alias.
 */
export interface ContentDashboardLayout {
  order: string[];
  hidden: string[];
  sizes: Record<string, SeoDashboardTileSize>;
}

export const GRID_COLUMNS = 12;
const DEFAULT_TILE_SIZE: SeoDashboardTileSize = { w: 3, h: 2 };

export const defaultTileSize = (tile?: ManifestSeoDashboardTile) => tile?.meta.size ?? DEFAULT_TILE_SIZE;

export const tileSize = (layout: ContentDashboardLayout, tile: ManifestSeoDashboardTile) =>
  layout.sizes[tile.alias] ?? defaultTileSize(tile);

// The tiles in weight order (as the extension registry returns them), all visible, at their own size.
export const defaultLayout = (tiles: ManifestSeoDashboardTile[]): ContentDashboardLayout => ({
  order: tiles.map((tile) => tile.alias),
  hidden: [],
  sizes: {},
});

/**
 * The user's layout, or the default when there is none. Tiles the layout doesn't know yet (from a new version
 * or a newly installed module) are added at the end, unless the user removed them.
 */
export function resolveLayout(layout: ContentDashboardLayout | undefined, tiles: ManifestSeoDashboardTile[]) {
  if (!layout) return defaultLayout(tiles);
  const unknown = tiles.map((tile) => tile.alias).filter((alias) => !layout.order.includes(alias) && !layout.hidden.includes(alias));
  return { ...layout, order: [...layout.order, ...unknown] };
}

// Drops sizes that match the tile's default and sorts `hidden`, so an unchanged layout equals the default.
// Aliases of tiles that aren't available right now (e.g. a disabled module) are kept, so their place isn't lost.
export function normalizeLayout(layout: ContentDashboardLayout, tiles: ManifestSeoDashboardTile[]): ContentDashboardLayout {
  const sizes: ContentDashboardLayout["sizes"] = {};
  for (const [alias, size] of Object.entries(layout.sizes ?? {})) {
    const fallback = defaultTileSize(tiles.find((tile) => tile.alias === alias));
    if (size.w !== fallback.w || size.h !== fallback.h) sizes[alias] = size;
  }
  return { order: [...(layout.order ?? [])], hidden: [...(layout.hidden ?? [])].sort(), sizes };
}

export const isDefaultLayout = (layout: ContentDashboardLayout, tiles: ManifestSeoDashboardTile[]) =>
  JSON.stringify(normalizeLayout(layout, tiles)) === JSON.stringify(normalizeLayout(defaultLayout(tiles), tiles));
