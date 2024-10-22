import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UMB_WORKSPACE_CONTEXT, UmbRoutableWorkspaceContext, UmbWorkspaceContext, UmbWorkspaceRouteManager } from "@umbraco-cms/backoffice/workspace";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import ScriptManagerDetailWorkspace from "./ScriptManagerDetailWorkspace.element";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbArrayState, UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { ScriptDefinitionViewModel, ScriptDetailViewModel } from "../api";
import ScriptManagerRepository from "../repositories/ScriptManagerRepository";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export default class ScriptManagerDetailContext extends UmbContextBase<ScriptManagerDetailContext> implements UmbWorkspaceContext, UmbRoutableWorkspaceContext {
    workspaceAlias = 'seoToolkit.scriptManager.detail';

    routes = new UmbWorkspaceRouteManager(this);
	#repository: ScriptManagerRepository;

	#definitions = new UmbArrayState<ScriptDefinitionViewModel>([], (item) => item.alias);
	public readonly definitions = this.#definitions.asObservable();

	#script = new UmbObjectState<ScriptDetailViewModel>({
		id: 0,
		name: ''
	});
	public readonly script = this.#script.asObservable();

    constructor(host: UmbControllerHost){
        super(host, UMB_WORKSPACE_CONTEXT.toString());
		this.provideContext(ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT, this);

		this.#repository = new ScriptManagerRepository(host);

        this.routes.setRoutes([
			{
				path: 'create',
				component: ScriptManagerDetailWorkspace,
				setup: async () => {
					this.load();
				},
			},
			{
				path: 'edit/:unique',
				component: ScriptManagerDetailWorkspace,
				setup: (_component, info) => {
					this.load(Number.parseInt(info.match.params.unique));
				},
			},
		]);
    }

	updateScript(script: Partial<ScriptDetailViewModel>){
		this.#script.update(script);
	}

	load(scriptId?: number) {
		this.#repository.getScriptDefinitions().then((resp) => {
			this.#definitions.setValue(resp.data!);
		});

		if (scriptId){
			this.#repository.getScript(scriptId).then((resp) => {
				this.#script.update(resp.data!);
			})
		}
	}

	async save() {
		const isNew = this.#script.value.id === 0;

		const response = await this.#repository.saveScript(this.#script.value);
		this.#script.setValue(response.data!);
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			instance.peek('positive', {
				data: {
					headline: 'Saved',
					message: 'Script successfully saved!'
				}
			})
		});
		if (isNew){
			history.replaceState(null, '', location.href + '/' + this.#script.value.id);
		}
	}

    getEntityType(): string {
        return 'script';
    }
}

export const ST_SCRIPTMANAGER_DETAIL_TOKEN_CONTEXT = new UmbContextToken<ScriptManagerDetailContext>(
	'scriptManagerDetailContext',
);