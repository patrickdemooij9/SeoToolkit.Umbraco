import { ManifestTreeItem } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_SITEAUDIT_ENTITY } from "../Constants";
import { ManifestWorkspace, ManifestWorkspaceContext, ManifestWorkspaceView } from "@umbraco-cms/backoffice/workspace";
import SiteAuditContentViewContext from "../workspaces/SiteAuditContentViewContext";

const SiteAuditTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.siteAudit',
    name: 'SeoToolkit SiteAudit',
    forEntityTypes: [
        SEOTOOLKIT_SITEAUDIT_ENTITY
    ]
}

const SiteAuditWorkspace: any = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace.siteAudit',
    name: 'SeoToolkit Workspace SiteAudit',
    element: () => import('../workspaces/SiteAuditModuleWorkspace.element'),
    api: () => import('../workspaces/SiteAuditModuleContext'),
    meta: {
        entityType: SEOTOOLKIT_SITEAUDIT_ENTITY,
        repositoryAlias: 'seoToolkit.repositories.siteAudits'
    }
};

const SiteAuditCreateWorkspace: ManifestWorkspace = {
    type: 'workspace',
    kind: 'routable',
    alias: 'seoToolkit.siteAudit.create',
    name: 'SeoToolkit SiteAudit Create',
    api: () => import('../workspaces/SiteAuditCreateContext'),
    meta: {
        entityType: 'st-siteAudit'
    }
}

const SiteAuditDetailWorkspace: ManifestWorkspace = {
    type: 'workspace',
    kind: 'routable',
    alias: 'seoToolkit.siteAudit.detail',
    name: 'SeoToolkit SiteAudit Detail',
    api: () => import('../workspaces/SiteAuditDetailContext'),
    meta: {
        entityType: 'st-siteAudit'
    }
}

const SiteAuditDetailEditView: ManifestWorkspaceView = {
    type: 'workspaceView',
    alias: 'seoToolkit.siteaudit.detail.main',
    name: 'SeoToolkit SiteAudit Detail Main',
    js: () => import('../workspaces/detailViews/SiteAuditDetailMain.element'),
    weight: 300,
    meta: {
        label: 'Edit',
		pathname: 'edit',
		icon: 'icon-document',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.siteAudit.detail'
        }
    ]
}

const SiteAuditContentWorkspaceContext: ManifestWorkspaceContext = {
  type: "workspaceContext",
  alias: "seoToolkit.siteAudit.contentWorkspaceContext",
  name: "SeoToolkit Site Audit content workspace context",
  api: SiteAuditContentViewContext,
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "Umb.Workspace.Document",
    },
  ],
};

const SiteAuditContentView: any = {
  type: "seoToolkitContentView",
  alias: "seoToolkit.siteAudit.contentView",
  name: "SeoToolkit SiteAudit content view",
  js: () => import("../workspaces/SiteAuditContentView.element"),
  weight: 500,
  meta: {
    label: "Page checks",
    pathname: "pageChecks"
  },
};

export const ModuleManifests = [SiteAuditTreeItem, SiteAuditWorkspace, SiteAuditCreateWorkspace, SiteAuditDetailWorkspace, SiteAuditDetailEditView, SiteAuditContentWorkspaceContext, SiteAuditContentView];