import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

// Re-declares Deploy's "Add to Transfer Queue" dialog (alias "Deploy.Modal.Queue") so we can open
// it and read back the "include descendants" choice. For SEO we use only includeDescendants and
// releaseDate; the dialog's culture/publish options don't apply.
export interface DeployQueueModalData {
  document: {
    unique: string;
    entityType: string;
    isRoot: boolean;
    hasChildren: boolean;
  };
  supportsTransferDescendants: boolean;
}

export interface DeployQueueModalValue {
  selectedCultures: string[];
  allCulturesSelected: boolean;
  includeDescendants: boolean;
  releaseDate: string | null;
}

export const DEPLOY_QUEUE_MODAL = new UmbModalToken<
  DeployQueueModalData,
  DeployQueueModalValue
>("Deploy.Modal.Queue", { modal: { type: "dialog", size: "medium" } });
