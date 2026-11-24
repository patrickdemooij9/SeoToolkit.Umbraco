import type {
  ManifestSeoDashboardTile,
  ManifestSeoDashboardTileView,
} from "../../../../SeoToolkit.Umbraco.Common/assets/src/manifests/seoDashboardTileManifest";
import type { SeoModuleEnabledConditionConfig } from "../../../../SeoToolkit.Umbraco.Common/assets/src/conditions/SeoModuleEnabledCondition";
import { SEOTOOLKIT_SITEAUDIT_ENTITY } from "../Constants";

const moduleEnabled: SeoModuleEnabledConditionConfig = { alias: "SeoToolkit.SeoModuleEnabled", moduleAlias: "siteAudit" };

// The tree section id of SiteAuditTreeSection.
const action = {
  label: "Go to site audit",
  href: `/umbraco/section/SeoToolkit/workspace/${SEOTOOLKIT_SITEAUDIT_ENTITY}/edit/b0d1c655-472b-40e7-9ac4-c6328ea9cf32`,
};

const healthView: ManifestSeoDashboardTileView = {
  type: "seoToolkitDashboardTileView",
  alias: "SeoToolkit.DashboardTileView.SiteAuditHealth",
  name: "SeoToolkit Site Audit Health Tile View",
  element: () => import("../dashboard/siteAuditHealthTileView.element"),
};

const healthTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.SiteAuditHealth",
  name: "SeoToolkit SEO Health Dashboard Tile",
  weight: 600,
  meta: {
    title: "SEO health",
    description: "Score from the latest finished site audit, with a breakdown per check.",
    icon: "icon-favorite",
    dataSource: "siteAudit.health",
    view: "SeoToolkit.DashboardTileView.SiteAuditHealth",
    size: { w: 6, h: 4 },
    action,
  },
  conditions: [moduleEnabled],
};

const metaFieldsTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.MetaFields",
  name: "SeoToolkit Meta Fields Dashboard Tile",
  weight: 300,
  meta: {
    title: "Meta fields",
    description: "Pages with a missing title or description, from the latest audit.",
    icon: "icon-tags",
    dataSource: "siteAudit.checks",
    parameters: { checks: "MissingTitleCheck,MissingDescriptionCheck" },
    view: "SeoToolkit.DashboardTileView.Metric",
    settings: { caption: "of {total} pages have meta issues", dateLabel: "Audit of" },
    size: { w: 6, h: 3 },
    action,
  },
  conditions: [moduleEnabled],
};

const brokenLinksTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.BrokenLinks",
  name: "SeoToolkit Broken Links Dashboard Tile",
  weight: 100,
  meta: {
    title: "Broken links",
    description: "Pages linking to something that doesn't resolve, from the latest audit.",
    icon: "icon-link",
    dataSource: "siteAudit.checks",
    parameters: { checks: "BrokenLinkCheck" },
    view: "SeoToolkit.DashboardTileView.Metric",
    settings: { caption: "of {total} pages have broken links", dateLabel: "Audit of" },
    size: { w: 3, h: 3 },
    action,
  },
  conditions: [moduleEnabled],
};

export const DashboardTileManifests = [healthView, healthTile, metaFieldsTile, brokenLinksTile];
