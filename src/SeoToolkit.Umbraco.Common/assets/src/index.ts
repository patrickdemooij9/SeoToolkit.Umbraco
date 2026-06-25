import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { seoToolkitSection } from './sections/seoToolkitSection';
import { welcomeDashboardManifest } from './dashboards/welcome/welcomeDashboardManifest';
import { seoDashboardManifest } from './dashboards/seo/seoDashboardManifest';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { seoToolkitSidebar } from './sidebar/seoToolkitSidebar';
import { TreeManifests } from './trees/seoToolkitTree';
import { manifest } from './conditions/workspaceEntityIdCondition';
import { Manifests as DocumentManifests } from './manifests/seoToolkitDocumentManifests';
import { ContentViewManifests } from './manifests/seoToolkitContentManifests';
import { client } from './api';
import { SeoDomainsManifest } from './manifests/seoDomainsManifest';
import { SettingsManifests } from './manifests/seoSettingsManifest';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
    host.consumeContext(UMB_AUTH_CONTEXT,(auth)=> {
        if (!auth) {
            return;
        }

        const config = auth.getOpenApiConfiguration();

        client.setConfig({
			auth: config.token,
			baseUrl: config.base,
			credentials: config.credentials,
		});

		client.interceptors.request.use(async (request, _options) => {
			const token = await auth.getLatestToken();
			request.headers.set('Authorization', `Bearer ${token}`);
			return request;
		});

    });

    extensionRegistry.register(seoToolkitSection);
    extensionRegistry.register(welcomeDashboardManifest);
    extensionRegistry.register(seoDashboardManifest);
    extensionRegistry.register(seoToolkitSidebar);
    extensionRegistry.register(manifest);

    extensionRegistry.registerMany(TreeManifests);
    extensionRegistry.registerMany(DocumentManifests);
    extensionRegistry.registerMany(ContentViewManifests);
    extensionRegistry.registerMany(SeoDomainsManifest);
    extensionRegistry.registerMany(SettingsManifests);
};