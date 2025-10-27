import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitDomainContext, { ST_DOMAIN_DETAIL_TOKEN_CONTEXT } from "../workspaces/SeoToolkitDomainContext";

export class DeleteDomainAction extends UmbWorkspaceActionBase<SeoToolkitDomainContext>{
    override async execute() {
        const workspaceContext = await this.getContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT);
        return await workspaceContext?.delete();
    }
}