import { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { Manifests } from "./manifests/ModuleManifests";

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
  extensionRegistry.registerMany(Manifests);
};
