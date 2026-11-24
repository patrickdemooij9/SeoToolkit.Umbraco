import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { DashboardData } from '../api';

export interface SeoDashboardTileSize {
    w: number;
    h: number;
}

export interface MetaSeoDashboardTile {
    title: string;
    description?: string;
    icon: string;
    /** Alias of the C# IDashboardDataSource that provides the data. */
    dataSource: string;
    /** Passed to the data source, so one data source can serve several tiles. */
    parameters?: Record<string, string>;
    /** Alias of the seoToolkitDashboardTileView that renders the data. */
    view: string;
    /** Settings for the view, e.g. a caption. Each view documents its own settings. */
    settings?: Record<string, unknown>;
    /** Default size in grid columns (of 12) and rows. */
    size?: SeoDashboardTileSize;
    action?: { label: string; href: string };
}

/**
 * A tile on the SEO dashboard of the content section.
 */
export interface ManifestSeoDashboardTile extends ManifestWithDynamicConditions {
    type: 'seoToolkitDashboardTile';
    meta: MetaSeoDashboardTile;
}

/**
 * The properties the dashboard sets on a tile view element.
 */
export interface SeoDashboardTileViewElement extends HTMLElement {
    data?: DashboardData;
    settings?: Record<string, unknown>;
    size?: SeoDashboardTileSize;
}

/**
 * Renders the data of a tile. One view can be used by many tiles, configured through the tile's `settings`.
 */
export interface ManifestSeoDashboardTileView extends ManifestElement<SeoDashboardTileViewElement> {
    type: 'seoToolkitDashboardTileView';
}

declare global {
    interface UmbExtensionManifestMap {
        SeoToolkitDashboardTile: ManifestSeoDashboardTile;
        SeoToolkitDashboardTileView: ManifestSeoDashboardTileView;
    }
}
