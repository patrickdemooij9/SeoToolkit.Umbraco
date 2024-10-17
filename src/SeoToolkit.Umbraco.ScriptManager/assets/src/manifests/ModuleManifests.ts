import { ManifestTreeItem, ManifestWorkspace, ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';
import { SEOTOOLKIT_SCRIPTMANAGER_ENTITY } from '../Constants';

const ScriptManagerTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.scriptManager',
    name: 'SeoToolkit ScriptManager',
    forEntityTypes: [
        SEOTOOLKIT_SCRIPTMANAGER_ENTITY
    ]
}

const ScriptManagerWorkspace: ManifestWorkspace = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace.scriptManager',
    name: 'SeoToolkit RobotsTxt ScriptManager',
    element: () => import('../workspaces/ScriptManagerModuleWorkspace.element'),
    api: () => import('../workspaces/ScriptManagerModuleContext'),
    meta: {
        entityType: SEOTOOLKIT_SCRIPTMANAGER_ENTITY
    }
};

const ScriptManagerDetailWorkspace: ManifestWorkspace = {
    type: 'workspace',
    kind: 'routable',
    alias: 'seoToolkit.scriptManager.detail',
    name: 'SeoToolkit ScriptManager Detail',
    element: () => import('../workspaces/ScriptManagerDetailWorkspace.element'),
    api: () => import('../workspaces/ScriptManagerDetailContext'),
    meta: {
        entityType: 'script'
    }
}

const ScriptManagerDetailEditView: ManifestWorkspaceView = {
    type: 'workspaceView',
    alias: 'seoToolkit.scriptManager.detail.edit',
    name: 'SeoToolkit ScriptManager Detail Edit',
    js: () => import('../workspaces/detailViews/ScriptManagerEdit.element'),
    weight: 300,
    meta: {
        label: 'Overview',
		pathname: 'overview',
		icon: 'icon-webhook',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.scriptManager.detail'
        }
    ]
}

export const Manifests = [ScriptManagerTreeItem, ScriptManagerWorkspace, ScriptManagerDetailWorkspace, ScriptManagerDetailEditView];