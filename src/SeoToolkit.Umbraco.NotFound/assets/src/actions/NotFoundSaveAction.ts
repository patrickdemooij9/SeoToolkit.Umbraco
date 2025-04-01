import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import NotFoundModuleWorkspaceContext, {
  ST_NOTFOUND_MODULE_TOKEN_CONTEXT,
} from "../workspaces/NotFoundModuleWorkspaceContext";

export class NotFoundSaveAction extends UmbWorkspaceActionBase<NotFoundModuleWorkspaceContext> {
  override async execute() {
    const workspaceContext = await this.getContext(
      ST_NOTFOUND_MODULE_TOKEN_CONTEXT
    );
    return await workspaceContext.save();
  }
}
