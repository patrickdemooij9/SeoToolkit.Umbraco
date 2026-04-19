import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import ScriptManagerModuleContext, { ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT } from "../workspaces/ScriptManagerModuleContext";

export class ScriptManagerSortAction extends UmbWorkspaceActionBase<ScriptManagerModuleContext> {
    override async execute() {
        const context = await this.getContext(ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT);
        context?.openSortModal();
    }
}
