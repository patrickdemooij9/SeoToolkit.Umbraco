import { ManifestWorkspaceAction, ManifestWorkspaceView } from '@umbraco-cms/backoffice/workspace';

const workSpaceView: ManifestWorkspaceView = {
    type: 'workspaceView',
    alias: 'seoToolkit.common.document.edit',
    name: 'SeoToolkit Common Document Edit',
    js: () => import('../workspaces/seoToolkitDocumentView.element'),
    weight: 300,
    meta: {
        label: 'SEO',
        pathname: 'seo',
        icon: 'icon-globe',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'Umb.Workspace.DocumentType'
        }
    ]
}

const overwriteSaveAction: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    overwrites: 'Umb.WorkspaceAction.DocumentType.Save',
    alias: 'seoToolkit.common.document.save',
    name: 'SeoToolkit Common Document Save',
    api: () => import("../actions/saveDocumentAction"),
    meta: {
        label: '#buttons_save',
        look: 'primary',
        color: 'positive',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'Umb.Workspace.DocumentType'
        }
    ]
}

export const Manifests = [workSpaceView, overwriteSaveAction];