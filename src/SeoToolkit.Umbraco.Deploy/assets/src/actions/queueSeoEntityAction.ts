import { UmbEntityActionBase, UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { SeoDeployClient } from "../api/seoDeployClient";
import { buildTransferSet } from "../api/seoDeployItems";

// Asks the server which per-node SEO entities exist for the node and queues the document + those;
// queues nothing and informs the editor when the node has no SEO data.
export class QueueSeoEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		const contentKey = this.args.unique;
		if (!contentKey) return;

		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const token = await authContext?.getLatestToken();
		if (!token) return;

		try {
			const client = new SeoDeployClient(token);
			const items = buildTransferSet(contentKey, await client.getSeoItems(contentKey));
			if (items.length === 0) {
				notificationContext?.peek("warning", { data: { message: "No SEO data to transfer for this node." } });
				return;
			}
			await client.queueAdd(items);
			notificationContext?.peek("positive", { data: { message: "SEO added to transfer queue." } });
		} catch (e) {
			notificationContext?.peek("danger", { data: { message: (e as Error).message } });
		}
	}
}

export default QueueSeoEntityAction;
