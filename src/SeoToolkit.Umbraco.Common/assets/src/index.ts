import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { seoToolkitSection } from './sections/seoToolkitSection';
import { welcomeDashboardManifest } from './dashboards/welcome/welcomeDashboardManifest';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { OpenAPI } from './api';
import { seoToolkitSidebar } from './sidebar/seoToolkitSidebar';
import { TreeManifests } from './trees/seoToolkitTree';
import { manifest } from './conditions/workspaceEntityIdCondition';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {

    host.consumeContext(UMB_AUTH_CONTEXT,(auth)=> {

        const config = auth.getOpenApiConfiguration();
  
        OpenAPI.BASE = config.base;
        OpenAPI.WITH_CREDENTIALS = config.withCredentials;
        OpenAPI.CREDENTIALS = config.credentials;
        OpenAPI.TOKEN = config.token;
  
    });

    extensionRegistry.register(seoToolkitSection);
    extensionRegistry.register(welcomeDashboardManifest);
    extensionRegistry.register(seoToolkitSidebar);
    extensionRegistry.register(manifest);

    extensionRegistry.registerMany(TreeManifests);
};