import { UmbEntityBulkActionBase } from "@umbraco-cms/backoffice/entity-bulk-action";
import RedirectRepository from "../dataLayer/RedirectRepository";
import { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

export default class DeleteRedirectAction extends UmbEntityBulkActionBase<object>{
    async execute() {
        const repository = new RedirectRepository(this._host);
        await repository.delete(this.selection.map((item => item)));

        const context = await this.getContext(ST_REDIRECT_MODULE_TOKEN_CONTEXT);
        context?.requestCollection();
    }
}