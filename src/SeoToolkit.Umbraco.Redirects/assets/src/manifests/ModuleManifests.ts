
import { ManifestTreeItem } from '@umbraco-cms/backoffice/tree';
import { SEOTOOLKIT_REDIRECT_ENTITY } from '../Constants';

const RedirectTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.redirects',
    name: 'SeoToolkit Redirect',
    forEntityTypes: [
        SEOTOOLKIT_REDIRECT_ENTITY
    ]
}

const RedirectWorkspace: any = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace.redirects',
    name: 'SeoToolkit Redirects Workspace',
    element: () => import('../workspaces/RedirectModuleWorkspace.element'),
    api: () => import('../workspaces/RedirectModuleContext'),
    meta: {
        entityType: SEOTOOLKIT_REDIRECT_ENTITY,
        repositoryAlias: 'seoToolkit.repositories.redirects'
    }
};

export const Manifests = [RedirectTreeItem, RedirectWorkspace];