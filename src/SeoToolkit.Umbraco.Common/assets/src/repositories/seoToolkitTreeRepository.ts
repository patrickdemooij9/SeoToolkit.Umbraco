import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbApi } from "@umbraco-cms/backoffice/extension-api";
import { UmbTreeItemModel, UmbTreeRepositoryBase, UmbTreeRootModel } from "@umbraco-cms/backoffice/tree";
import { seoToolkitTreeSource } from "../sources/seoToolkitTreeSource";
import { SEOTOOLKIT_TREE_ROOT, SEOTOOLKIT_TREE_STORE_CONTEXT } from "../constants/seoToolkitConstants";

export class seoToolkitTreeRepository extends UmbTreeRepositoryBase<UmbTreeItemModel, UmbTreeRootModel> implements UmbApi {
    constructor(host: UmbControllerHost) {
        super(host, seoToolkitTreeSource, SEOTOOLKIT_TREE_STORE_CONTEXT);
    }

    async requestTreeRoot(){
        const data: UmbTreeRootModel = {
            unique: null,
            entityType: SEOTOOLKIT_TREE_ROOT,
            name: 'SeoToolkit',
            hasChildren: true,
            isFolder: true,
        };

        return { data };
    }
}