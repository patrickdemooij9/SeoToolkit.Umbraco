import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

// Re-declares Deploy's partial-restore dialog by its registered alias so we can open it for
// environment selection without taking a dependency on the Deploy client package.
export interface DeployEnvironmentInfoModel {
  url: string;
  name?: string;
}

export interface DeployRestoreModalData {
  isTreeRestore: boolean;
  document?: { unique?: string | null; entityType?: string | null };
}

export interface DeployRestoreModalValue {
  environment?: DeployEnvironmentInfoModel;
  ignoreDependencies?: boolean;
}

export const DEPLOY_PARTIALRESTORE_MODAL = new UmbModalToken<
  DeployRestoreModalData,
  DeployRestoreModalValue
>("Deploy.Modal.PartialRestore", { modal: { type: "dialog" } });
