import { ManifestWorkspaceContext } from "@umbraco-cms/backoffice/workspace";


const documentView: any = {
  type: "seoToolkitDocumentView",
  alias: "seoToolkit.metaFields.documentView",
  name: "SeoToolkit MetaFields document view",
  js: () => import("../workspaces/MetaFieldsDocumentView.element"),
  weight: 500,
  meta: {
    label: "Meta Fields",
    pathname: "metaFields",
    icon: "icon-globe",
  },
};

const documentWorkspaceContext: ManifestWorkspaceContext = {
  type: "workspaceContext",
  alias: "seoToolkit.metaFields.documentWorkspaceContext",
  name: "SeoToolkit Meta Fields workspace context",
  api: () => import("../workspaces/MetaFieldsDocumentContext"),
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "Umb.Workspace.DocumentType",
    },
  ],
};

const contentView: any = {
  type: "seoToolkitContentView",
  alias: "seoToolkit.metaFields.contentView",
  name: "SeoToolkit MetaFields content view",
  js: () => import("../workspaces/MetaFieldsContentView.element"),
  weight: 500,
  meta: {
    label: "Meta Fields",
    pathname: "metaFields",
    icon: "icon-globe",
  },
};

const contentWorkspaceContext: ManifestWorkspaceContext = {
    type: "workspaceContext",
    alias: "seoToolkit.metaFields.contentWorkspaceContext",
    name: "SeoToolkit Meta Fields content workspace context",
    api: () => import("../workspaces/MetaFieldsContentContext"),
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

export const DocumentManifests = [
  documentView,
  documentWorkspaceContext,
  contentView,
  contentWorkspaceContext,
];

