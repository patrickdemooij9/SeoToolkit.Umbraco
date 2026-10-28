import { ManifestDashboard } from "@umbraco-cms/backoffice/dashboard";
import { ManifestSeoDashboardTileView } from "../../manifests/seoDashboardTileManifest";

const contentDashboard: ManifestDashboard = {
  type: "dashboard",
  alias: "seoToolkitContentDashboard",
  name: "SEO",
  weight: 100,
  meta: {
    label: "SEO",
    pathname: "seo",
  },
  element: () => import("./contentDashboard.element"),
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "Umb.Section.Content",
    },
  ],
};

// The generic view for numbers. Tiles of any module can use it through `meta.view`.
const metricTileView: ManifestSeoDashboardTileView = {
  type: "seoToolkitDashboardTileView",
  alias: "SeoToolkit.DashboardTileView.Metric",
  name: "SeoToolkit Dashboard Metric Tile View",
  element: () => import("./views/metricTileView.element"),
};

export const ContentDashboardManifests = [contentDashboard, metricTileView];
