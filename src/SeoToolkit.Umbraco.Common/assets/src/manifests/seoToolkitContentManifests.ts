import { ManifestCondition } from "@umbraco-cms/backoffice/extension-api";
import { SeoEnabledCondition } from "../conditions/SeoEnabledCondition";
import {
  ManifestWorkspaceContext,
  ManifestWorkspaceView,
} from "@umbraco-cms/backoffice/workspace";
import { SeoModuleEnabledCondition } from "../conditions/SeoModuleEnabledCondition";

const workSpaceView: ManifestWorkspaceView = {
  type: "workspaceView",
  alias: "seoToolkit.common.content.edit",
  name: "SeoToolkit Common Content Edit",
  js: () => import("../workspaces/seoToolkitContentView.element"),
  weight: -100,
  meta: {
    label: "SEO",
    pathname: "seo",
    icon: "icon-globe",
  },
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "Umb.Workspace.Document",
    },
    {
      alias: "SeoToolkit.SeoEnabled",
    }
  ],
};

const contentContext: ManifestWorkspaceContext = {
  type: "workspaceContext",
  alias: "seoToolkit.common.content.context",
  name: "SeoToolkit Common Content Context",
  api: () => import("../workspaces/SeoToolkitContentContext"),
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "Umb.Workspace.Document",
    },
  ],
};

const seoConditionManifest: ManifestCondition = {
  type: "condition",
  name: "Seo Enabled Condition",
  alias: "SeoToolkit.SeoEnabled",
  api: SeoEnabledCondition,
};

const seoModuleEnabledConditionManifest: ManifestCondition = {
  type: "condition",
  name: "Seo Module Enabled Condition",
  alias: "SeoToolkit.SeoModuleEnabled",
  api: SeoModuleEnabledCondition,
};

export const ContentViewManifests = [
  workSpaceView,
  contentContext,
  seoConditionManifest,
  seoModuleEnabledConditionManifest,
];
