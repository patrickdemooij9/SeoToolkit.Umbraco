import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { NamedEntityTreeItemResponseModel } from "@umbraco-cms/backoffice/external/backend-api";
import { UmbTreeAncestorsOfRequestArgs, UmbTreeChildrenOfRequestArgs, UmbTreeServerDataSourceBase } from "@umbraco-cms/backoffice/tree";
import { SeoToolkitService } from "../api";
import { SEOTOOLKIT_TREE_ENTITY, SEOTOOLKIT_TREE_ROOT } from "../constants/seoToolkitConstants";
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
    return {
		unique: item.id,
		parent: {
			unique: item.parent?.id || null,
			entityType: item.parent ? SEOTOOLKIT_TREE_ENTITY : SEOTOOLKIT_TREE_ROOT,
		},
		name: item.name,
		entityType: SEOTOOLKIT_TREE_ENTITY,
		hasChildren: item.hasChildren,
		isFolder: false,
		icon: 'icon-book-alt',
	};
};