import { UmbEntryPointOnInit } from "@umbraco-cms/backoffice/extension-api";
import { DeployManifests } from "./manifests/deployManifests";

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
  extensionRegistry.registerMany(DeployManifests);
};
