import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UMB_WORKSPACE_CONTEXT, UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { SEOTOOLKIT_SCRIPTMANAGER_ENTITY } from "../Constants";
import { UmbArrayState } from "@umbraco-cms/backoffice/observable-api";
import { ScriptListViewModel } from "../api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { ScriptManagerRepository } from "../repositories/ScriptManagerRepository";

export default class ScriptManagerModuleContext extends UmbControllerBase implements UmbWorkspaceContext {
    workspaceAlias = 'seoToolkit.module.workspace.scriptManager';

    #repository: ScriptManagerRepository;

    #scripts = new UmbArrayState<ScriptListViewModel>([], (item) => item.id);
    public readonly scripts = this.#scripts.asObservable();

    constructor(host: UmbControllerBase) {
        super(host);

        this.provideContext(ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT, this);
        this.provideContext(UMB_WORKSPACE_CONTEXT, this);

        this.#repository = new ScriptManagerRepository(host);  
    }

    async load(){
        this.#scripts.setValue((await this.#repository.getScripts()).data!);
    }

    getEntityType(): string {
        return SEOTOOLKIT_SCRIPTMANAGER_ENTITY;
    }
}

export const ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT = new UmbContextToken<ScriptManagerModuleContext>(
	'scriptManagerModuleContext',
);