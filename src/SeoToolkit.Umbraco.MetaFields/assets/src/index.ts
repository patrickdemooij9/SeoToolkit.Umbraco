import { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { DocumentManifests } from "./manifests/DocumentManifests";
import { PropertyEditorManifests } from "./manifests/PropertyEditorManifests";
import { ModalManifests } from "./manifests/ModalManifests";
import { PreviewerManifests } from "./manifests/PreviewerManifests";
import { client } from "./api";

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

  extensionRegistry.registerMany(DocumentManifests);
  extensionRegistry.registerMany(PropertyEditorManifests);
  extensionRegistry.registerMany(ModalManifests);
  extensionRegistry.registerMany(PreviewerManifests);
};

