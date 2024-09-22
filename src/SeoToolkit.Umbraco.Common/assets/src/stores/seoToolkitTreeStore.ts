import { UmbControllerHostElement } from "@umbraco-cms/backoffice/controller-api";
import { UmbUniqueTreeStore } from "@umbraco-cms/backoffice/tree";

export class seoToolkitTreeStore extends UmbUniqueTreeStore {
    
    constructor(host: UmbControllerHostElement){
        super(host, "seoToolkitTree");
    }
}