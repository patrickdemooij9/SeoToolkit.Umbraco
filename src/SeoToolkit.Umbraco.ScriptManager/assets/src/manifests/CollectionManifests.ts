import { UMB_COLLECTION_ALIAS_CONDITION } from "@umbraco-cms/backoffice/collection";
import { ManifestCollection, ManifestCollectionAction, ManifestCollectionView, ManifestEntityBulkAction, ManifestRepository } from "@umbraco-cms/backoffice/extension-registry";
import { SEOTOOLKIT_SCRIPTMANAGER_ENTITY } from "../Constants";

const ScriptManagerCollection: ManifestCollection = {
    type: 'collection',
    kind: 'default',
    alias: 'seoToolkit.collections.scripts',
    name: 'ScriptManager Collection',
    api: () => import('../workspaces/ScriptManagerModuleContext'),
    meta: {
        repositoryAlias: 'seoToolkit.repositories.scripts'
    }
}

const ScriptManagerCollectionView: ManifestCollectionView = {
    type: 'collectionView',
    alias: 'seoToolkit.collections.script.overview',
    name: 'ScriptManager Collection overview',
    js: () => import("../collections/ScriptManagerCollection.element"),
    meta: {
        label: 'Overview',
        icon: 'icon-list',
        pathName: 'overview',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.scripts',
        }
    ]
}

const ScriptManagerCollectionCreateAction: ManifestCollectionAction = {
    type: 'collectionAction',
    kind: 'button',
    name: 'ScriptManager Collection Overview Create',
    alias: 'seoToolkit.collections.script.createAction',
    meta: {
        label: '#general_create',
        href: '/umbraco/section/SeoToolkit/workspace/st-script/create',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.scripts',
        },
    ],
}

const ScriptManagerCollectionTrashBulkAction: ManifestEntityBulkAction = {
    type: 'entityBulkAction',
	alias: 'seoToolkit.collections.script.trashAction',
	name: 'ScriptManager Collection Overview Trash',
	weight: 10,
    api: () => import('../actions/ScriptManagerDeleteAction'),
	forEntityTypes: [SEOTOOLKIT_SCRIPTMANAGER_ENTITY],
	meta: {
		label: 'Delete'
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'seoToolkit.collections.scripts',
		}
	],
}

const ScriptManagerRepository: ManifestRepository = {
    type: 'repository',
    alias: 'seoToolkit.repositories.scripts',
    name: 'ScriptManager Repository',
    api: () => import('../repositories/ScriptManagerRepository')
}

export const CollectionManifests = [ScriptManagerCollection, ScriptManagerCollectionView, ScriptManagerCollectionCreateAction, ScriptManagerCollectionTrashBulkAction, ScriptManagerRepository];