import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import RobotsTxtModuleContext, { ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT } from "./RobotsTxtModuleContext";

export class RobotsTxtSaveAction extends UmbWorkspaceActionBase<RobotsTxtModuleContext> {
    override async execute() {
        const workspaceContext = await this.getContext(ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT);
        return await workspaceContext.submit(false);
    }
}