import { UMB_WORKSPACE_CONTEXT, UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { SEOTOOLKIT_MODULE_ENTITY } from "../constants/seoToolkitConstants";
import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export default class SeoToolkitModuleContext extends UmbControllerBase implements UmbWorkspaceContext {
    workspaceAlias: string = "seoToolkit.module.workspace";

    constructor(host: UmbControllerHost){
        super(host);

        this.provideContext(SEOTOOLKIT_MODULES_CONTEXT_TOKEN, this);
        this.provideContext(UMB_WORKSPACE_CONTEXT, this);
    }

    getEntityType(): string {
        return SEOTOOLKIT_MODULE_ENTITY;
    }
}

export const SEOTOOLKIT_MODULES_CONTEXT_TOKEN = new UmbContextToken<SeoToolkitModuleContext>(
	'SeoToolkitModuleContext',
);