import { ManifestCollection, ManifestCollectionAction, ManifestCollectionView, UMB_COLLECTION_ALIAS_CONDITION } from "@umbraco-cms/backoffice/collection";
import { SEOTOOLKIT_SITEAUDIT_ENTITY } from "../Constants";
import { ManifestEntityBulkAction, ManifestRepository } from "@umbraco-cms/backoffice/extension-registry";

const SiteAuditCollection: ManifestCollection = {
    type: 'collection',
    kind: 'default',
    alias: 'seoToolkit.collections.siteAudits',
    name: 'SiteAudits Collection',
    api: () => import('../workspaces/SiteAuditModuleContext'),
    meta: {
        repositoryAlias: 'seoToolkit.repositories.siteAudits'
    }
}

const SiteAuditCollectionView: ManifestCollectionView = {
    type: 'collectionView',
    alias: 'seoToolkit.collections.siteAudits.overview',
    name: 'SiteAudits Collection overview',
    js: () => import("../collections/SiteAuditCollection.element"),
    meta: {
        label: 'Overview',
        icon: 'icon-list',
        pathName: 'overview',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.siteAudits',
        }
    ]
}

const SiteAuditCollectionCreateAction: ManifestCollectionAction = {
    type: 'collectionAction',
    kind: 'button',
    name: 'SiteAudit Collection Overview Create',
    alias: 'seoToolkit.collections.siteAudits.createAction',
    meta: {
        label: '#general_create',
        href: '/umbraco/section/SeoToolkit/workspace/st-siteAudit/create',
    },
    conditions: [
        {
            alias: UMB_COLLECTION_ALIAS_CONDITION,
            match: 'seoToolkit.collections.siteAudits',
        },
    ],
}

const SiteAuditCollectionTrashBulkAction: ManifestEntityBulkAction = {
    type: 'entityBulkAction',
	alias: 'seoToolkit.collections.siteAudits.trashAction',
	name: 'SiteAudits Collection Overview Trash',
	weight: 10,
    api: () => import('../actions/SiteAuditDeleteAction'),
	forEntityTypes: [SEOTOOLKIT_SITEAUDIT_ENTITY],
	meta: {
		label: 'Delete'
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: 'seoToolkit.collections.siteAudits',
		}
	],
}

const SiteAuditRepository: ManifestRepository = {
    type: 'repository',
    alias: 'seoToolkit.repositories.siteAudits',
    name: 'SiteAudits Repository',
    api: () => import('../dataAccess/SiteAuditRepository')
}

export const CollectionManifests = [SiteAuditCollection, SiteAuditCollectionView, SiteAuditCollectionCreateAction, SiteAuditCollectionTrashBulkAction, SiteAuditRepository];