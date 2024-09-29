import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { OpenAPI } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
//import { robotsTxtWorkspaceView } from "../contentApps/RobotsTxtModuleContentApp.element";

export const onInit: UmbEntryPointOnInit = (host, _extensionRegistry) => {

    host.consumeContext(UMB_AUTH_CONTEXT, (auth) => {

        const config = auth.getOpenApiConfiguration();

        OpenAPI.BASE = config.base;
        OpenAPI.WITH_CREDENTIALS = config.withCredentials;
        OpenAPI.CREDENTIALS = config.credentials;
        OpenAPI.TOKEN = config.token;

    });

    //extensionRegistry.register(robotsTxtWorkspaceView);
};