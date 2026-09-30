import { UmbEntityActionBase, UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { umbOpenModal } from "@umbraco-cms/backoffice/modal";
import { BackofficeSeoToolkitDeploy } from "../api/generated";
import { DEPLOY_TRANSFER_QUEUE_MANAGER } from "../api/deployTransferQueue";
import { DEPLOY_QUEUE_MODAL } from "../api/deployQueueModal";

// Tree action: opens Deploy's queue dialog for the "include descendants" choice, then has the
// server queue the node's (and optionally descendants') SEO entities — never the content node.
export class QueueSeoEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		const contentKey = this.args.unique;
		if (!contentKey) return;

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		const queueManager = await this.getContext(DEPLOY_TRANSFER_QUEUE_MANAGER);
		if (!queueManager) {
			notificationContext?.peek("danger", { data: { message: "Deploy transfer queue is unavailable." } });
			return;
		}

		// Only includeDescendants and releaseDate are used; the dialog's culture/publish options don't apply.
		const options = await umbOpenModal(this, DEPLOY_QUEUE_MODAL, {
			data: {
				document: { unique: contentKey, entityType: "document", isRoot: false, hasChildren: true },
				supportsTransferDescendants: true,
			},
		}).catch(() => undefined);
		if (!options) return; // dialog cancelled

		try {
			// One server call queues everything; then refresh the widget once.
			const { data } = await BackofficeSeoToolkitDeploy.postUmbracoSeoToolkitDeploySeoQueueAdd({
				query: {
					contentKey,
					includeDescendants: options.includeDescendants,
					releaseDate: options.releaseDate ?? undefined,
				},
			});
			const { added } = data;
			if (added === 0) {
				notificationContext?.peek("warning", { data: { message: "No SEO data to transfer for this node." } });
				return;
			}
			await queueManager.refresh();
			notificationContext?.peek("positive", { data: { message: "SEO added to transfer queue." } });
		} catch (e) {
			notificationContext?.peek("danger", { data: { message: (e as Error).message } });
		}
	}
}

export default QueueSeoEntityAction;
