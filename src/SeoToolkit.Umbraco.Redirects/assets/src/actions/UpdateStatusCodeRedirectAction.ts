import { UmbEntityBulkActionBase } from "@umbraco-cms/backoffice/entity-bulk-action";
import { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

export default class UpdateStatusCodeRedirectAction extends UmbEntityBulkActionBase<object> {
  override async execute() {
    const workspaceContext = await this.getContext(
      ST_REDIRECT_MODULE_TOKEN_CONTEXT
    );
    await workspaceContext?.openStatusCodeModal(this.selection.map((item => Number.parseInt(item))));
  }
}
