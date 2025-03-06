import { ManifestCondition } from "@umbraco-cms/backoffice/extension-api";
import { SeoEnabledCondition } from "../conditions/SeoEnabledCondition";
import {
  ManifestWorkspaceAction,
  ManifestWorkspaceView,
} from "@umbraco-cms/backoffice/workspace";

const workSpaceView: ManifestWorkspaceView = {
  type: "workspaceView",
  alias: "seoToolkit.common.content.edit",
  name: "SeoToolkit Common Content Edit",
  js: () => import("../workspaces/seoToolkitContentView.element"),
  weight: 200,
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
    },
  ],
};

const overwriteSaveAction: ManifestWorkspaceAction = {
  type: "workspaceAction",
  kind: "default",
  overwrites: "Umb.WorkspaceAction.Document.Save",
  alias: "seoToolkit.common.content.save",
  name: "SeoToolkit Common Content Save",
  api: () => import("../actions/saveDocumentAction"),
  meta: {
    label: "#buttons_save",
    look: "primary",
    color: "positive",
  },
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

export const ContentViewManifests = [
  workSpaceView,
  overwriteSaveAction,
  seoConditionManifest,
];
