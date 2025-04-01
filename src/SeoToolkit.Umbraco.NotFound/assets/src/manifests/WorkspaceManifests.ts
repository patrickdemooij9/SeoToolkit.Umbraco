import { ManifestTreeItem } from '@umbraco-cms/backoffice/tree';
import { SEOTOOLKIT_NOTFOUND_ENTITY } from '../NotFoundConstants';
import { ManifestWorkspace, ManifestWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import { NotFoundSaveAction } from '../actions/NotFoundSaveAction';

const NotFoundTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.notFound',
    name: 'SeoToolkit NotFound',
    forEntityTypes: [
        SEOTOOLKIT_NOTFOUND_ENTITY
    ]
}

const NotFoundWorkspace: ManifestWorkspace = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace.notFound',
    name: 'SeoToolkit NotFound Workspace',
    element: () => import('../workspaces/NotFoundModuleWorkspace.element'),
    api: () => import('../workspaces/NotFoundModuleWorkspaceContext'),
    meta: {
        entityType: SEOTOOLKIT_NOTFOUND_ENTITY
    }
};

const NotFoundSaveActionManifest: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    alias: 'seoToolkit.notFound.workspace.actions.save',
    name: 'SeoToolkit NotFound Workspace Save',
    api: NotFoundSaveAction,
    meta: {
        look: 'primary',
        color: 'positive',
        label: '#buttons_save',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.module.workspace.notFound',
        },
    ],
}

export const WorkspaceManifests = [ NotFoundTreeItem, NotFoundWorkspace, NotFoundSaveActionManifest ];