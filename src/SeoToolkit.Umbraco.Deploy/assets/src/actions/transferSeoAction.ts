import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { SeoDeployClient } from "../api/seoDeployClient";
import { buildTransferSet } from "../api/seoDeployItems";

export default class TransferSeoAction extends UmbWorkspaceActionBase {

	constructor(host: UmbControllerHost, args: any) {
		super(host, args);
	}

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		const contentKey = workspaceContext?.getUnique?.();
		const token = await authContext?.getLatestToken();
		if (!contentKey || !token) return;

		try {
			const client = new SeoDeployClient(token);
			const items = buildTransferSet(await client.getSeoItems(contentKey));
			if (items.length === 0) {
				notificationContext?.peek("warning", { data: { message: "No SEO data to transfer for this node." } });
				return;
			}
			const response = await client.instantDeploy(items);
			if (response.ok) {
				notificationContext?.peek("positive", { data: { message: "SEO deployed upstream." } });
			} else {
				notificationContext?.peek("danger", { data: { message: `SEO deploy failed (${response.status}).` } });
			}
		} catch (e) {
			notificationContext?.peek("danger", { data: { message: (e as Error).message } });
		}
	}
}
