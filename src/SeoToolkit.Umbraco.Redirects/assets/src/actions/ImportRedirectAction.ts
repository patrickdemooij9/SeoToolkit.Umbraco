import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import RedirectModuleContext, { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

export default class ImportRedirectAction extends UmbWorkspaceActionBase<RedirectModuleContext>{
    override async execute() {
        const workspaceContext = await this.getContext(ST_REDIRECT_MODULE_TOKEN_CONTEXT);
        workspaceContext.openImportModal();
    }
}