import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { SEOTOOLKIT_SCRIPTMANAGER_ENTITY } from "../Constants";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbDefaultCollectionContext } from "@umbraco-cms/backoffice/collection";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";
import { UMB_MODAL_MANAGER_CONTEXT } from "@umbraco-cms/backoffice/modal";
import ScriptManagerRepository from "../repositories/ScriptManagerRepository";
import { ScriptSortItem } from "../modals/ScriptManagerSortModal.element";

export default class ScriptManagerModuleContext extends UmbDefaultCollectionContext<any, any> {
    workspaceAlias = 'seoToolkit.collections.scripts'

    constructor(host: UmbControllerBase) {
        super(host, UMB_WORKSPACE_CONTEXT.toString());

        this.provideContext(ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT, this);
    }

    async openSortModal() {
        const lastSegment = window.location.href.split("/").pop();
        let domainId: string | undefined = undefined;
        if (lastSegment && lastSegment.includes("~")) {
            const parts = lastSegment.split("~");
            if (parts.length === 2) {
                domainId = parts[1];
            }
        }

        const repository = new ScriptManagerRepository(this._host);
        const resp = await repository.getScripts(domainId);
        if (!resp.data) return;

        const scripts: Array<ScriptSortItem> = [...resp.data]
            .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
            .map(s => ({ id: s.id, name: s.name ?? '' }));

        this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (instance) => {
            if (!instance) return;

            const modal = instance.open(
                this._host,
                'seoToolkit.modal.scriptManager.sort',
                {
                    modal: { type: 'sidebar', size: 'small' },
                    data: { scripts },
                }
            );

            await modal.onSubmit();

            const reorderedScripts = modal.getValue() as Array<ScriptSortItem>;
            const keys = reorderedScripts.map(s => s.id);
            await repository.sortScripts(keys);

            this.requestCollection();
        });
    }

    getEntityType(): string {
        return SEOTOOLKIT_SCRIPTMANAGER_ENTITY;
    }
}

export const ST_SCRIPTMANAGER_MODULE_TOKEN_CONTEXT = new UmbContextToken<ScriptManagerModuleContext>(
	'scriptManagerModuleContext',
);