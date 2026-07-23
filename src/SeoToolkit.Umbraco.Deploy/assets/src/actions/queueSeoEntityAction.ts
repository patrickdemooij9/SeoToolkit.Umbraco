import { UmbEntityActionBase, UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { SeoDeployClient } from "../api/seoDeployClient";
import { buildTransferSet } from "../api/seoDeployItems";
import { DEPLOY_TRANSFER_QUEUE_MANAGER } from "../api/deployTransferQueue";

// This action runs in a tree/entity context (no document workspace context), so it can't use the
// `SeoToolkit.SeoEnabled` condition. Instead it asks the server which per-node SEO entities exist
// for the node and only queues the document + those; when the node has no SEO data it queues
// nothing and informs the editor. Items are added through Deploy's own transfer-queue manager so
// the transfer queue widget refreshes, matching the native "Add to Transfer Queue" action.
export class QueueSeoEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		const contentKey = this.args.unique;
		if (!contentKey) return;

		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const queueManager = await this.getContext(DEPLOY_TRANSFER_QUEUE_MANAGER);
		const token = await authContext?.getLatestToken();
		if (!token) return;
		if (!queueManager) {
			notificationContext?.peek("danger", { data: { message: "Deploy transfer queue is unavailable." } });
			return;
		}

		try {
			const client = new SeoDeployClient(token);
			const items = buildTransferSet(contentKey, await client.getSeoItems(contentKey));
			if (items.length === 0) {
				notificationContext?.peek("warning", { data: { message: "No SEO data to transfer for this node." } });
				return;
			}
			for (const item of items) {
				await queueManager.add({
					id: item.id,
					entityType: item.entityType,
					culture: "*",
					includeDescendants: false,
					releaseDate: null,
				});
			}
			notificationContext?.peek("positive", { data: { message: "SEO added to transfer queue." } });
		} catch (e) {
			notificationContext?.peek("danger", { data: { message: (e as Error).message } });
		}
	}
}

export default QueueSeoEntityAction;
