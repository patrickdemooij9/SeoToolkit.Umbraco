import { UmbEntityBulkActionBase } from "@umbraco-cms/backoffice/entity-bulk-action";
import ScriptManagerRepository from "../repositories/ScriptManagerRepository";
import { ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT } from "../workspaces/ScriptManagerModuleContext";

export default class ScriptManagerDeleteAction extends UmbEntityBulkActionBase<object>{
    async execute() {
        const repository = new ScriptManagerRepository(this._host);
        await repository.deleteScripts(this.selection.map((item => Number.parseInt(item))));

        const context = await this.getContext(ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT);
        context.requestCollection();
    }
}