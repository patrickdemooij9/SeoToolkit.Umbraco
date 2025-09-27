import { ManifestTreeItem } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_DOMAIN_ENTITY, SEOTOOLKIT_DOMAIN_ROOT_ENTITY } from "../constants/seoToolkitConstants";
import { CreateDomainTreeAction } from "../actions/createDomainTreeAction";
import { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitDomainContext from "../workspaces/SeoToolkitDomainContext";
import { SeoToolkitDomainEditViewElement } from "../workspaces/SeoToolkitDomainEditView.element";
import { SaveDomainAction } from "../actions/saveDomainAction";

const DomainsTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.domains',
    name: 'SeoToolkit Domains',
    forEntityTypes: [
        SEOTOOLKIT_DOMAIN_ENTITY,
        SEOTOOLKIT_DOMAIN_ROOT_ENTITY
    ]
}

const CreateDomainTreeActionManifest = {
	type: 'entityAction',
	alias: 'seoToolkit.domains.createDomain',
	name: 'SeoToolkit Create Domain',
	weight: 10,
	api: CreateDomainTreeAction,
	forEntityTypes: [SEOTOOLKIT_DOMAIN_ROOT_ENTITY],
	meta: {
		icon: 'icon-add',
		label: 'Create domain',
	},
};

const DomainDetailWorkspace: ManifestWorkspace = {
    type: 'workspace',
    kind: 'routable',
    alias: 'seoToolkit.domain.detail',
    name: 'SeoToolkit Domain Detail',
    api: SeoToolkitDomainContext,
    meta: {
        entityType: 'seoToolkit-domain'
    }
} 

const DomainDetailEditView: ManifestWorkspaceView = {
    type: 'workspaceView',
    alias: 'seoToolkit.domain.detail.edit',
    name: 'SeoToolkit Domain Detail Edit',
    js: SeoToolkitDomainEditViewElement,
    weight: 100,
    meta: {
        label: 'Edit',
		pathname: 'edit',
		icon: 'icon-document',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.domain.detail'
        }
    ]
}

const DomainSaveActionManifest: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    alias: 'seoToolkit.domain.detail.save',
    name: 'SeoToolkit Domain Workspace Save',
    api: SaveDomainAction,
    meta: {
        look: 'primary',
        color: 'positive',
        label: '#buttons_save',
    },
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'seoToolkit.domain.detail',
        },
    ],
}

export const SeoDomainsManifest = [ DomainsTreeItem, CreateDomainTreeActionManifest, DomainDetailWorkspace, DomainDetailEditView, DomainSaveActionManifest ];