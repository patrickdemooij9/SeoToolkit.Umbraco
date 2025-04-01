import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { OpenAPI } from './api';
import { WorkspaceManifests } from './manifests/WorkspaceManifests';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {

    host.consumeContext(UMB_AUTH_CONTEXT,(auth)=> {

        const config = auth.getOpenApiConfiguration();
  
        OpenAPI.BASE = config.base;
        OpenAPI.WITH_CREDENTIALS = config.withCredentials;
        OpenAPI.CREDENTIALS = config.credentials;
        OpenAPI.TOKEN = config.token;
  
    });

    extensionRegistry.registerMany(WorkspaceManifests);
};