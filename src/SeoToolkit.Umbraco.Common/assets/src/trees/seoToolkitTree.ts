import { ManifestMenu, ManifestRepository, ManifestTree, ManifestTreeItem, ManifestTreeStore, ManifestTypes, ManifestWorkspace, ManifestWorkspaceContext, ManifestWorkspaceView } from "@umbraco-cms/backoffice/extension-registry";
import { seoToolkitTreeRepository } from "../repositories/seoToolkitTreeRepository";
import { seoToolkitTreeStore } from "../stores/seoToolkitTreeStore";
import { SEOTOOLKIT_MODULE_ENTITY, SEOTOOLKIT_TREE_ROOT } from "../constants/seoToolkitConstants";

export const treeRepository: ManifestRepository = {
    type: 'repository',
    alias: "SeoToolkitTreeRepository",
    name: 'SeoToolkit Tree repository',
    api: seoToolkitTreeRepository
}

export const treeStore: ManifestTreeStore = {
    type: 'treeStore',
    alias: "SeoToolkitTreeStore",
    name: 'SeoToolkit tree Store',
    api: seoToolkitTreeStore
};

export const tree: ManifestTree = {
    type: 'tree',
    kind: "default",
    alias: "SeoToolkitTree",
    name: 'SeoToolkit tree',
    meta: {
        repositoryAlias: "SeoToolkitTreeRepository"
    }
};

export const treeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'SeoToolkitTreeItem',
    name: 'SeoToolkit Tree Item',
    forEntityTypes: [
        SEOTOOLKIT_MODULE_ENTITY, SEOTOOLKIT_TREE_ROOT
    ]
};

export const menu: ManifestMenu = {
    type: 'menu',
    alias: "SeoToolkitMenu",
    name: 'SeoToolkit Menu'
}

export const menuItem: ManifestTypes = {
    type: 'menuItem',
    kind: 'tree',
    alias: 'SeoToolkitMenuItem',
    name: 'SeoToolkit Menu Item',
    weight: 400,
    meta: {
        label: 'Times',
        icon: 'icon-alarm-clock',
        entityType: SEOTOOLKIT_TREE_ROOT,
        menus: ["SeoToolkitMenu"],
        treeAlias: "SeoToolkitTree",
        hideTreeRoot: true
    }
};

export const workspaceManifest: ManifestWorkspace = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace',
    name: 'SeoToolkit Module Workspace',
    element: () => import('../workspaces/seoToolkitModuleWorkspace.element'),
    meta: {
        entityType: SEOTOOLKIT_MODULE_ENTITY
    }
};

export const workspaceContext: ManifestWorkspaceContext = {
	type: 'workspaceContext',
	alias: 'seoToolkit.module.context',
	name: 'SeoTOolkit workspace context',
    js: () => import('../workspaces/seoToolkitModule.context'),
	conditions: [
		{
			alias: 'Umb.Condition.WorkspaceAlias',
			match: 'seoToolkit.module.workspace',
		},
	],
};

export const infoWorkspaceView: ManifestWorkspaceView =
{
    type: 'workspaceView',
    alias: 'seotoolkit.workspace.info',
    name: 'default view',
    js: () => import('../dashboards/welcome/welcomeDashboard.element'),
    weight: 300,
    meta: {
        icon: 'icon-alarm-clock',
        pathname: 'default',
        label: 'time'
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.module.workspace'
        },
    ],
};

export const TreeManifests = [treeRepository, treeStore, tree, treeItem, menu, menuItem, workspaceManifest, workspaceContext, infoWorkspaceView];