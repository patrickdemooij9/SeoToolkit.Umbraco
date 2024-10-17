import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { NamedEntityTreeItemResponseModel } from "@umbraco-cms/backoffice/external/backend-api";
import { UmbTreeAncestorsOfRequestArgs, UmbTreeChildrenOfRequestArgs, UmbTreeServerDataSourceBase } from "@umbraco-cms/backoffice/tree";
import { SeoToolkitService } from "../api";
import { SEOTOOLKIT_MODULE_ENTITY, SEOTOOLKIT_ROBOTSTXT_ENTITY, SEOTOOLKIT_SCRIPTMANAGER_ENTITY, SEOTOOLKIT_TREE_ROOT } from "../constants/seoToolkitConstants";
import { SeoToolkitTreeItemModel } from "../trees/types";

export class seoToolkitTreeSource extends UmbTreeServerDataSourceBase<NamedEntityTreeItemResponseModel, SeoToolkitTreeItemModel> {

    constructor(host: UmbControllerHost) {
        super(host, {
            getRootItems,
            getChildrenOf,
            getAncestorsOf,
            mapper
        });
    }
}

const getRootItems = () => {
    return SeoToolkitService.getUmbracoSeoToolkitTreeInfoRoot();
};

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
    if (args.parent.unique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return SeoToolkitService.getUmbracoSeoToolkitTreeInfoChildren({
			parentId: args.parent.unique,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) => {
    return SeoToolkitService.getUmbracoSeoToolkitTreeInfoAncestors({
		descendantId: args.treeItem.unique,
	});
}

const mapper = (item: NamedEntityTreeItemResponseModel): SeoToolkitTreeItemModel => {
	const isInfo = item.id === 'CDF429D1-2380-4AC2-AC3E-22D619EE4529'.toLowerCase();
	let entity = SEOTOOLKIT_MODULE_ENTITY;
	switch(item.id){
		case '20A2086E-7D72-44BA-B97B-5836CAF6E28E'.toLowerCase():
			entity = SEOTOOLKIT_ROBOTSTXT_ENTITY;
			break;
		case '94E95F4A-2ECB-4038-BCFD-8357B7C41F1A'.toLowerCase():
			entity = SEOTOOLKIT_SCRIPTMANAGER_ENTITY;
			break;
		default:
			entity = SEOTOOLKIT_MODULE_ENTITY;
			break;
	}
    return {
		unique: item.id,
		parent: {
			unique: item.parent?.id || null,
			entityType: item.parent ? entity : SEOTOOLKIT_TREE_ROOT,
		},
		name: item.name,
		entityType: entity,
		hasChildren: item.hasChildren,
		isFolder: false,
		icon: isInfo ? 'icon-book' : 'icon-book-alt'
	};
};