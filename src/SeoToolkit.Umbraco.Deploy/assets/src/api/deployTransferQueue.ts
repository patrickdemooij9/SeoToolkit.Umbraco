import { UmbContextToken, type UmbContextMinimal } from "@umbraco-cms/backoffice/context-api";

// Re-declares Deploy's transfer-queue manager by its registered alias so we can add SEO entities
// through the same context the native "Add to Transfer Queue" action uses. Its add() posts to the
// server AND refreshes the queue state (reload + BroadcastChannel + entity signs), so the transfer
// queue widget updates immediately — a raw POST to /queue/add does not.
export interface DeployAddToQueueModel {
  id: string;
  entityType: string;
  culture?: string | null;
  includeDescendants?: boolean;
  releaseDate?: string | null;
}

export interface DeployTransferQueueManager extends UmbContextMinimal {
  add(item: DeployAddToQueueModel): Promise<string | undefined>;
  // Reloads queue state and refreshes tree signs once; called after server-side queuing.
  refresh(): Promise<void>;
}

export const DEPLOY_TRANSFER_QUEUE_MANAGER = new UmbContextToken<DeployTransferQueueManager>(
  "DeployTransferQueueManager",
);
