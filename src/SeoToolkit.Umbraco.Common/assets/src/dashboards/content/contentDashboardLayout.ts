import { ManifestSeoDashboardTile, SeoDashboardTileSize } from "../../manifests/seoDashboardTileManifest";

export interface TilePosition extends SeoDashboardTileSize {
  x: number;
  y: number;
}

/**
 * Where a user placed the tiles on the dashboard (in a 12-column grid), keyed by tile alias.
 * Tiles without a position (e.g. new tiles) are placed in the first free spot; removed tiles are listed in `hidden`.
 */
export interface ContentDashboardLayout {
  positions: Record<string, TilePosition>;
  hidden: string[];
}

export const GRID_COLUMNS = 12;
const DEFAULT_TILE_SIZE: SeoDashboardTileSize = { w: 3, h: 2 };

export const defaultTileSize = (tile?: ManifestSeoDashboardTile) => tile?.meta.size ?? DEFAULT_TILE_SIZE;

// Stored layouts come from user data, so anything that isn't a valid layout counts as "not customized".
export function parseLayout(value: unknown): ContentDashboardLayout | undefined {
  const layout = value as Partial<ContentDashboardLayout> | undefined;
  if (!layout || typeof layout.positions !== "object" || !Array.isArray(layout.hidden)) return undefined;
  return { positions: layout.positions ?? {}, hidden: layout.hidden };
}
