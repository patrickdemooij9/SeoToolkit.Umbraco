import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbRoutableWorkspaceContext, UmbWorkspaceContext, UmbWorkspaceRouteManager } from "@umbraco-cms/backoffice/workspace";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import ScriptManagerDetailWorkspace from "./ScriptManagerDetailWorkspace.element";

export default class ScriptManagerDetailContext extends UmbControllerBase implements UmbWorkspaceContext, UmbRoutableWorkspaceContext {
    workspaceAlias = 'seoToolkit.scriptManager.detail';

    routes = new UmbWorkspaceRouteManager(this);

    constructor(host: UmbControllerHost){
        super(host);
        console.log("Test");

        this.routes.setRoutes([
			{
				path: 'create',
				component: ScriptManagerDetailWorkspace,
				setup: async () => {
                    console.log("Hello?");
					//this.create();
				},
			},
			{
				path: 'edit/:unique',
				component: ScriptManagerDetailWorkspace,
				setup: (_component, _info) => {
					this.removeUmbControllerByAlias('isNewRedirectController');
					//this.load(info.match.params.unique);
				},
			},
		]);
    }

    getEntityType(): string {
        return 'script';
    }
}