import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UMB_ACTION_EVENT_CONTEXT } from "@umbraco-cms/backoffice/action";
import { SaveDocumentEvent } from "../events/saveDocumentEvent";

export default class SaveAndPublishDocumentAction extends UmbWorkspaceActionBase {

    constructor(host: UmbControllerHost, args: any) {
		super(host, args);
	}

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT);
		workspaceContext?.saveAndPublish();

        const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
        eventContext?.dispatchEvent(new SaveDocumentEvent("document.save"));
	}
}