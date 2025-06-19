import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { Manifests } from './manifests/ModuleManifests';
import { CollectionManifests } from './manifests/CollectionManifests';
import { ModalManifest } from './manifests/ModalManifests';
import { client } from './api';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {

    host.consumeContext(UMB_AUTH_CONTEXT, (auth) => {
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
      request.headers.set("Authorization", `Bearer ${token}`);
      return request;
    });
  });

    extensionRegistry.registerMany(Manifests);
    extensionRegistry.registerMany(CollectionManifests);
    extensionRegistry.registerMany(ModalManifest);
};