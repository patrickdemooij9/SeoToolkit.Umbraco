import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { NamedEntityTreeItemResponseModel } from "@umbraco-cms/backoffice/external/backend-api";
import { UmbTreeItemModel, UmbTreeServerDataSourceBase } from "@umbraco-cms/backoffice/tree";
import { SeoToolkitService } from "../api";
import { UmbPagedModel } from "@umbraco-cms/backoffice/repository";

export class seoToolkitTreeSource extends UmbTreeServerDataSourceBase<NamedEntityTreeItemResponseModel, UmbTreeItemModel> {

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

const getChildrenOf = () => {
    return new Promise<UmbPagedModel<NamedEntityTreeItemResponseModel>>((resolve) => {
        resolve({
            total: 0,
            items: []
        })
    });
};

const getAncestorsOf = () => {
    return new Promise<NamedEntityTreeItemResponseModel[]>((res) => res([]));
}

const mapper = (item: NamedEntityTreeItemResponseModel): UmbTreeItemModel => {
    return {
        unique: item.id,
        name: item.name,
        parent: {
            unique: null,
            entityType: "root"
        },
        entityType: "seoToolkit",
        hasChildren: item.hasChildren,
        isFolder: false,
        icon: 'icon-alarm-clock'
    };
};