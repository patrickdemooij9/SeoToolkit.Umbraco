import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import ScriptManagerDetailContext, { ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT } from "../workspaces/ScriptManagerDetailContext";

export class ScriptManagerSaveAction extends UmbWorkspaceActionBase<ScriptManagerDetailContext>{
    override async execute() {
        const workspaceContext = await this.getContext(ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT);
        return await workspaceContext.save();
    }
}