import { UMB_WORKSPACE_CONTEXT, UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { SEOTOOLKIT_ROBOTSTXT_ENTITY } from "../Constants";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbArrayState, UmbStringState } from "@umbraco-cms/backoffice/observable-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { ValidationError } from "../types/ValidationError";
import { RobotsTxtRepository } from "../dataAccess/RobotsTxtRepository";

export default class RobotsTxtModuleContext extends UmbControllerBase implements UmbWorkspaceContext {
    workspaceAlias = "seoToolkit.context.robotsTxtModule";
    #repository?: RobotsTxtRepository;

    #content = new UmbStringState("");
    public readonly content = this.#content.asObservable();

    #validationErrors = new UmbArrayState<ValidationError>([], (item) => item.error);
    public readonly validationErrors = this.#validationErrors.asObservable();

    constructor(host: UmbControllerHost){
        super(host);

        this.provideContext(ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT, this);
        this.provideContext(UMB_WORKSPACE_CONTEXT, this);

        this.#repository = new RobotsTxtRepository(host);

        this.#repository.getContent().then((result) => {
            this.#content.setValue(result.data!);
        })
    }

    setContent(content: string){
        this.#content.setValue(content);
    }

    submit(){
        this.#repository?.saveContent(this.#content.value);
    }

    getEntityType(): string {
        return SEOTOOLKIT_ROBOTSTXT_ENTITY;
    }
}


export const ST_ROBOTSTXT_MODULE_TOKEN_CONTEXT = new UmbContextToken<RobotsTxtModuleContext>(
	'robotsTxtModuleContext',
);