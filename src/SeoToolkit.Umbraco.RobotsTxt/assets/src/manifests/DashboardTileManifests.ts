import type { ManifestSeoDashboardTile } from "../../../../SeoToolkit.Umbraco.Common/assets/src/manifests/seoDashboardTileManifest";
import type { SeoModuleEnabledConditionConfig } from "../../../../SeoToolkit.Umbraco.Common/assets/src/conditions/SeoModuleEnabledCondition";
import { SEOTOOLKIT_ROBOTSTXT_ENTITY } from "../Constants";

const moduleEnabled: SeoModuleEnabledConditionConfig = { alias: "SeoToolkit.SeoModuleEnabled", moduleAlias: "robotsTxt" };

const indexableTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.Indexable",
  name: "SeoToolkit Indexable Dashboard Tile",
  weight: 500,
  meta: {
    title: "Indexable",
    description: "Checks robots.txt for a rule that blocks all search engines.",
    icon: "icon-eye",
    dataSource: "robotsTxt.indexable",
    view: "SeoToolkit.DashboardTileView.Metric",
    size: { w: 3, h: 2 },
    // The tree section id of RobotsTxtTreeSection.
    action: {
      label: "Edit robots.txt",
      href: `/umbraco/section/SeoToolkit/workspace/${SEOTOOLKIT_ROBOTSTXT_ENTITY}/edit/20a2086e-7d72-44ba-b97b-5836caf6e28e`,
    },
  },
  conditions: [moduleEnabled],
};

export const DashboardTileManifests = [indexableTile];
