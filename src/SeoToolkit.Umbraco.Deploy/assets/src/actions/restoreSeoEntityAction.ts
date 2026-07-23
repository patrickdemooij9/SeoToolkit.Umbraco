import { UmbEntityActionBase, UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { SeoDeployClient } from "../api/seoDeployClient";
import { buildSeoUdis } from "../api/seoDeployItems";

// SEO-only mirror of Deploy's "Partial Restore": pulls the node's SEO entities down from the
// upstream source, leaving the local document content untouched (ignoreDependencies).
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

		try {
			const client = new SeoDeployClient(token);
			const response = await client.restorePartial(buildSeoUdis(contentKey));
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
