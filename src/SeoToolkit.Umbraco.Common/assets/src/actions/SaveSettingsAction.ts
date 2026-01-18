import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitSettingsContext, { ST_SETTINGS_MODULE_TOKEN_CONTEXT } from "../workspaces/SeoToolkitSettingsContext";

export class SaveSettingsAction extends UmbWorkspaceActionBase<SeoToolkitSettingsContext>{
    override async execute() {
        const workspaceContext = await this.getContext(ST_SETTINGS_MODULE_TOKEN_CONTEXT);
        return await workspaceContext?.save();
    }
}