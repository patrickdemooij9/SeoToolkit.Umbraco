import type { ManifestSeoDashboardTile } from "../../../../SeoToolkit.Umbraco.Common/assets/src/manifests/seoDashboardTileManifest";
import type { SeoModuleEnabledConditionConfig } from "../../../../SeoToolkit.Umbraco.Common/assets/src/conditions/SeoModuleEnabledCondition";
import { SEOTOOLKIT_SCRIPTMANAGER_ENTITY } from "../Constants";

const moduleEnabled: SeoModuleEnabledConditionConfig = { alias: "SeoToolkit.SeoModuleEnabled", moduleAlias: "scriptManager" };

const scriptsTile: ManifestSeoDashboardTile = {
  type: "seoToolkitDashboardTile",
  alias: "SeoToolkit.DashboardTile.Scripts",
  name: "SeoToolkit Scripts Dashboard Tile",
  weight: 200,
  meta: {
    title: "Scripts",
    description: "Scripts the script manager adds to every page.",
    icon: "icon-script",
    dataSource: "scriptManager.scripts",
    view: "SeoToolkit.DashboardTileView.Metric",
    settings: { caption: "scripts active" },
    size: { w: 3, h: 2 },
    // The tree section id of ScriptManagerTreeSection.
    action: {
      label: "Manage scripts",
      href: `/umbraco/section/SeoToolkit/workspace/${SEOTOOLKIT_SCRIPTMANAGER_ENTITY}/edit/94e95f4a-2ecb-4038-bcfd-8357b7c41f1a`,
    },
  },
  conditions: [moduleEnabled],
};

export const DashboardTileManifests = [scriptsTile];
