import { ManifestTreeItem, ManifestWorkspace } from "@umbraco-cms/backoffice/extension-registry";
import { SEOTOOLKIT_ROBOTSTXT_ENTITY } from "../Constants";

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
    meta: {
        entityType: SEOTOOLKIT_ROBOTSTXT_ENTITY
    }
};

export const Manifests = [RobotsTxtTreeItem, RobotsTxtWorkspace];