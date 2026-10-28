import type { ManifestSeoDashboardTile } from "../../../../SeoToolkit.Umbraco.Common/assets/src/manifests/seoDashboardTileManifest";
import type { SeoModuleEnabledConditionConfig } from "../../../../SeoToolkit.Umbraco.Common/assets/src/conditions/SeoModuleEnabledCondition";
import { SEOTOOLKIT_REDIRECT_ENTITY } from "../Constants";

const moduleEnabled: SeoModuleEnabledConditionConfig = { alias: "SeoToolkit.SeoModuleEnabled", moduleAlias: "redirects" };

// The tree section id of RedirectsTreeSection.
const action = {
  label: "Manage redirects",
  href: `/umbraco/section/SeoToolkit/workspace/${SEOTOOLKIT_REDIRECT_ENTITY}/edit/1147f58d-d2d5-425b-aede-db537bdac9ef`,
};

const redirectsTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.Redirects",
  name: "SeoToolkit Redirects Dashboard Tile",
  weight: 400,
  meta: {
    title: "Redirects",
    description: "How many redirects exist, split by type.",
    icon: "icon-trafic",
    dataSource: "redirects.count",
    view: "SeoToolkit.DashboardTileView.Metric",
    size: { w: 3, h: 2 },
    action,
  },
  conditions: [moduleEnabled],
};

const redirectUsageTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.RedirectUsage",
  name: "SeoToolkit Redirect Usage Dashboard Tile",
  weight: 50,
  meta: {
    title: "Redirect usage",
    description: "Which redirects are still being used, based on when each was last hit.",
    icon: "icon-chart-curve",
    dataSource: "redirects.usage",
    parameters: { recentDays: "30", staleDays: "90", take: "5" },
    view: "SeoToolkit.DashboardTileView.Metric",
    settings: { caption: "of {total} enabled redirects hit in the last 30 days" },
    size: { w: 6, h: 3 },
    action,
  },
  conditions: [moduleEnabled],
};

export const DashboardTileManifests = [redirectsTile, redirectUsageTile];
