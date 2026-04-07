import { ManifestCollection, ManifestCollectionAction, ManifestCollectionView, UMB_COLLECTION_ALIAS_CONDITION } from "@umbraco-cms/backoffice/collection";
import { SEOTOOLKIT_REDIRECT_ENTITY } from "../Constants";
import { ManifestEntityBulkAction, ManifestRepository } from "@umbraco-cms/backoffice/extension-registry";

const RedirectCollection: ManifestCollection = {
    type: 'collection',
    kind: 'default',
    alias: 'seoToolkit.collections.redirects',
    name: 'Redirects Collection',
    js: () => import("../collections/RedirectCollection.element"),
    api: () => import('../workspaces/RedirectModuleContext'),
    meta: {
        repositoryAlias: 'seoToolkit.repositories.redirects'
    }
}

const RedirectCollectionView: ManifestCollectionView = {
    type: 'collectionView',
    alias: 'seoToolkit.collections.redirects.overview',
    name: 'Redirects overview',
    js: () => import("../collections/RedirectCollectionView.element"),
    meta: {
        label: 'Overview',
        icon: 'icon-list',
        pathName: 'overview',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.redirects',
        }
    ]
}

const RedirectCollectionCreateAction: ManifestCollectionAction = {
    type: 'collectionAction',
    kind: 'button',
    name: 'Redirect Collection Overview Create',
    alias: 'seoToolkit.collections.redirects.createAction',
    api: () => import('../actions/CreateRedirectAction'),
    meta: {
        label: '#general_create',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.redirects',
        },
    ],
}

const RedirectCollectionImportAction: ManifestCollectionAction = {
    type: 'collectionAction',
    kind: 'button',
    name: 'Redirect Collection Overview Import',
    alias: 'seoToolkit.collections.redirects.importAction',
    api: () => import('../actions/ImportRedirectAction'),
    meta: {
        label: 'Import',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.redirects',
        },
    ],
}

const RedirectCollectionExportAction: ManifestCollectionAction = {
    type: 'collectionAction',
    kind: 'button',
    name: 'Redirect Collection Overview Export',
    alias: 'seoToolkit.collections.redirects.exportAction',
    api: () => import('../actions/ExportRedirectAction'),
    meta: {
        label: 'Export',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.redirects',
        },
    ],
} 

const RedirectCollectionTrashBulkAction: ManifestEntityBulkAction = {
    type: 'entityBulkAction',
	alias: 'seoToolkit.collections.redirects.trashAction',
	name: 'Redirect Collection Overview Trash',
	weight: 10,
    api: () => import('../actions/DeleteRedirectAction'),
	forEntityTypes: [SEOTOOLKIT_REDIRECT_ENTITY],
	meta: {
		label: 'Delete'
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'seoToolkit.collections.redirects',
		}
	],
}

const RedirectUpdateStatusCodeBulkAction: ManifestEntityBulkAction = {
    type: 'entityBulkAction',
	alias: 'seoToolkit.collections.redirects.updateStatusCode',
	name: 'Redirect Collection Overview Update Status Code',
	weight: 20,
    api: () => import('../actions/UpdateStatusCodeRedirectAction'),
	forEntityTypes: [SEOTOOLKIT_REDIRECT_ENTITY],
	meta: {
		label: 'Update status code'
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'seoToolkit.collections.redirects',
		}
	],
}

const RedirectRepository: ManifestRepository = {
    type: 'repository',
    alias: 'seoToolkit.repositories.redirects',
    name: 'Redirects Repository',
    api: () => import('../dataLayer/RedirectRepository')
}

export const CollectionManifests = [RedirectCollection, RedirectCollectionView, RedirectCollectionCreateAction, RedirectCollectionTrashBulkAction, RedirectUpdateStatusCodeBulkAction, RedirectCollectionImportAction, RedirectCollectionExportAction, RedirectRepository];