import { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbUniqueTreeStore } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_TREE_STORE_CONTEXT } from "../constants/seoToolkitConstants";

export class seoToolkitTreeStore extends UmbUniqueTreeStore {
    
    constructor(host: UmbControllerHostElement){
        super(host, SEOTOOLKIT_TREE_STORE_CONTEXT);
    }
}