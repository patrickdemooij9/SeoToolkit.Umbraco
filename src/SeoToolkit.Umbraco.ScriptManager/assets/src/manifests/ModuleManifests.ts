import { ManifestTreeItem, ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';
import { SEOTOOLKIT_SCRIPTMANAGER_ENTITY } from '../Constants';
import { ScriptManagerSaveAction } from '../actions/ScriptManagerSaveAction';

const ScriptManagerTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.scriptManager',
    name: 'SeoToolkit ScriptManager',
    forEntityTypes: [
        SEOTOOLKIT_SCRIPTMANAGER_ENTITY
    ]
}

const ScriptManagerWorkspace: any = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace.scriptManager',
    name: 'SeoToolkit RobotsTxt ScriptManager',
    element: () => import('../workspaces/ScriptManagerModuleWorkspace.element'),
    api: () => import('../workspaces/ScriptManagerModuleContext'),
    meta: {
        entityType: SEOTOOLKIT_SCRIPTMANAGER_ENTITY,
        repositoryAlias: 'seoToolkit.repositories.scripts'
    }
};

const ScriptManagerDetailWorkspace: ManifestWorkspace = {
    type: 'workspace',
    kind: 'routable',
    alias: 'seoToolkit.scriptManager.detail',
    name: 'SeoToolkit ScriptManager Detail',
    api: () => import('../workspaces/ScriptManagerDetailContext'),
    meta: {
        entityType: 'st-script'
    }
}

const ScriptManagerDetailEditView: ManifestWorkspaceView = {
    type: 'workspaceView',
    alias: 'seoToolkit.scriptManager.detail.edit',
    name: 'SeoToolkit ScriptManager Detail Edit',
    js: () => import('../workspaces/detailViews/ScriptManagerEdit.element'),
    weight: 300,
    meta: {
        label: 'Edit',
		pathname: 'edit',
		icon: 'icon-document',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.scriptManager.detail'
        }
    ]
}

const ScriptSaveActionManifest: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    alias: 'seoToolkit.scriptManager.detail.save',
    name: 'SeoToolkit ScriptManager Workspace Save',
    api: ScriptManagerSaveAction,
    meta: {
        look: 'primary',
        color: 'positive',
        label: '#buttons_save',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.scriptManager.detail',
        },
    ],
}

export const Manifests = [ScriptManagerTreeItem, ScriptManagerWorkspace, ScriptManagerDetailWorkspace, ScriptManagerDetailEditView, ScriptSaveActionManifest];