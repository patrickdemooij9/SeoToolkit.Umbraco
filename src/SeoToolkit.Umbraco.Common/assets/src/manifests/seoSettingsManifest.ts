import { ManifestTreeItem } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_SETTINGS_ENTITY } from "../constants/seoToolkitConstants";
import { ManifestWorkspace, ManifestWorkspaceAction } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitSettingsContext from "../workspaces/SeoToolkitSettingsContext";
import { SaveSettingsAction } from "../actions/SaveSettingsAction";

const SettingsTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.settings',
    name: 'SeoToolkit Settings',
    forEntityTypes: [
        SEOTOOLKIT_SETTINGS_ENTITY
    ]
}

const SettingsWorkspace: ManifestWorkspace = {
    type: 'workspace',
    kind: 'routable',
    alias: 'seoToolkit.domain.settings.workspace',
    name: 'SeoToolkit Settings Workspace',
    api: SeoToolkitSettingsContext,
    meta: {
        entityType: SEOTOOLKIT_SETTINGS_ENTITY
    }
} 

const SettingsSaveActionManifest: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    alias: 'seoToolkit.settings.detail.save',
    name: 'SeoToolkit Settings Workspace Save',
    api: SaveSettingsAction,
    meta: {
        look: 'primary',
        color: 'positive',
        label: '#buttons_save',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.domain.settings.workspace',
        },
    ],
}

export const SettingsManifests = [SettingsTreeItem, SettingsWorkspace, SettingsSaveActionManifest];