import { UmbEntityActionBase, UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { umbOpenModal } from "@umbraco-cms/backoffice/modal";
import { SeoDeployClient } from "../api/seoDeployClient";
import { buildSeoUdis, SEO_DEPLOY_ENTITY_TYPES } from "../api/seoDeployItems";
import { DEPLOY_PARTIALRESTORE_MODAL } from "../api/deployRestoreModal";

// SEO-only mirror of Deploy's "Partial Restore": opens Deploy's own restore dialog so the user
// picks the source environment, then restores just the node's SEO entities. The SEO artifacts
// depend on the document only in Exist mode, so a normal restore leaves the document content
// untouched — no ignoreDependencies needed (forcing it 500s when the environment disallows it).
export class RestoreSeoEntityAction extends UmbEntityActionBase<never> {
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

		const restoreModel = await umbOpenModal(this, DEPLOY_PARTIALRESTORE_MODAL, {
			data: {
				isTreeRestore: false,
				document: { unique: contentKey, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue },
			},
		}).catch(() => undefined);

		const sourceUrl = restoreModel?.environment?.url;
		if (!sourceUrl) return;

		try {
			const client = new SeoDeployClient(token);
			const response = await client.restorePartial(
				buildSeoUdis(contentKey),
				sourceUrl,
				restoreModel.ignoreDependencies ?? false,
			);
			if (response.ok) {
				notificationContext?.peek("positive", { data: { message: "SEO restore started from the server." } });
			} else {
				notificationContext?.peek("danger", { data: { message: `SEO restore failed (${response.status}).` } });
			}
		} catch (e) {
			notificationContext?.peek("danger", { data: { message: (e as Error).message } });
		}
	}
}

export default RestoreSeoEntityAction;
