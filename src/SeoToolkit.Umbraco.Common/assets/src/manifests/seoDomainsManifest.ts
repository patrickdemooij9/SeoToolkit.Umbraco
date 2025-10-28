import { ManifestTreeItem } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_DOMAIN_ENTITY, SEOTOOLKIT_DOMAIN_ROOT_ENTITY } from "../constants/seoToolkitConstants";
import { CreateDomainTreeAction } from "../actions/createDomainTreeAction";
import { ManifestWorkspace, ManifestWorkspaceAction, ManifestWorkspaceView } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitDomainContext from "../workspaces/SeoToolkitDomainContext";
import { SeoToolkitDomainEditViewElement } from "../workspaces/SeoToolkitDomainEditView.element";
import { SaveDomainAction } from "../actions/saveDomainAction";
import { SeoDomainDeletableCondition } from "../conditions/SeoDomainDeleteableCondition";
import { ManifestCondition } from "@umbraco-cms/backoffice/extension-api";
import { DeleteDomainAction } from "../actions/deleteDomainAction";
import SeoToolkitTreeItemElement from "../trees/SeoToolkitTreeItem.element";

const DomainsTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.domains',
    name: 'SeoToolkit Domains',
    element: SeoToolkitTreeItemElement,
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

const DomainDeleteAction: ManifestWorkspaceAction = {
    type: 'workspaceAction',
    kind: 'default',
    alias: 'seoToolkit.domain.detail.delete',
    name: 'SeoToolkit Domain Workspace Delete',
    api: DeleteDomainAction,
    meta: {
        look: 'secondary',
        color: 'danger',
        label: 'Delete'
    },
    conditions: [
        {
            alias: 'SeoToolkit.DomainDeleteCondition'
        }
    ]
}

const DomainsDeleteCondition: ManifestCondition = {
  type: "condition",
  name: "Seo Delete Domain Condition",
  alias: "SeoToolkit.DomainDeleteCondition",
  api: SeoDomainDeletableCondition,
};

export const SeoDomainsManifest = [ DomainsTreeItem, CreateDomainTreeActionManifest, DomainDetailWorkspace, DomainDetailEditView, DomainSaveActionManifest, DomainDeleteAction, DomainsDeleteCondition ];