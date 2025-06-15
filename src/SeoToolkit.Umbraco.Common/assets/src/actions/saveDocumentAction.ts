import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbSubmittableWorkspaceContext, UmbSubmitWorkspaceAction, UmbWorkspaceActionArgs, UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import { SaveDocumentEvent } from "../events/saveDocumentEvent";
import { UMB_ACTION_EVENT_CONTEXT } from "@umbraco-cms/backoffice/action";

export default class SaveDocumentAction extends UmbWorkspaceActionBase<UmbSubmittableWorkspaceContext> {

    #submitAction: UmbSubmitWorkspaceAction;

    constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<UmbSubmittableWorkspaceContext>) {
		super(host, args);

        this.#submitAction = new UmbSubmitWorkspaceAction(host, args);
    }

    override async execute() {
        await this.#submitAction.execute();

        const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
        eventContext?.dispatchEvent(new SaveDocumentEvent("document.save"));
    }
}