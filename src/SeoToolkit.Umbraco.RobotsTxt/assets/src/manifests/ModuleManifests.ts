import { ManifestTreeItem } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_ROBOTSTXT_ENTITY } from "../Constants";
import { RobotsTxtSaveAction } from "../contexts/RobotsTxtSaveAction";
import { ManifestWorkspace, ManifestWorkspaceAction } from "@umbraco-cms/backoffice/workspace";
import { ManifestModal } from "@umbraco-cms/backoffice/modal";

const RobotsTxtTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.robotsTxt',
    name: 'SeoToolkit RobotsTxt',
    forEntityTypes: [
        SEOTOOLKIT_ROBOTSTXT_ENTITY
    ]
}

const RobotsTxtWorkspace: ManifestWorkspace = {
    type: 'workspace',
    alias: 'seoToolkit.module.workspace.robotsTxt',
    name: 'SeoToolkit RobotsTxt Workspace',
    element: () => import('../workspaces/RobotsTxtModuleWorkspace.element'),
    api: () => import('../contexts/RobotsTxtModuleContext'),
    meta: {
        entityType: SEOTOOLKIT_ROBOTSTXT_ENTITY
    }
};

const RobotsTxtSaveActionManifest: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    alias: 'seoToolkit.module.workspace.actions.save',
    name: 'SeoToolkit RobotsTxt Workspace Save',
    api: RobotsTxtSaveAction,
    meta: {
        look: 'primary',
        color: 'positive',
        label: '#buttons_save',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.module.workspace.robotsTxt',
        },
    ],
}

const RobotsTxtValidationModal: ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.robotstxt.validation',
    name: 'SeoToolkit RobotsTxt Validation Modal',
    js: () => import('../modals/RobotsTxtValidationModal.element')
}

export const Manifests = [RobotsTxtTreeItem, RobotsTxtWorkspace, RobotsTxtSaveActionManifest, RobotsTxtValidationModal];