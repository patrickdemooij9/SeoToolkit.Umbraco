import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbApi } from "@umbraco-cms/backoffice/extension-api";
import { UmbTreeItemModel, UmbTreeRepositoryBase, UmbTreeRootModel } from "@umbraco-cms/backoffice/tree";
import { seoToolkitTreeSource } from "../sources/seoToolkitTreeSource";

export class seoToolkitTreeRepository extends UmbTreeRepositoryBase<UmbTreeItemModel, UmbTreeRootModel> implements UmbApi {
    constructor(host: UmbControllerHost) {
        super(host, seoToolkitTreeSource, "seoToolkitTree");
    }

    async requestTreeRoot(){
        const data: UmbTreeRootModel = {
            unique: null,
            entityType: "root",
            name: 'time',
            hasChildren: true,
            isFolder: true,
        };

        return { data };
    }
}