import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import RedirectModuleContext from "../workspaces/RedirectModuleContext";
import RedirectRepository from "../dataLayer/RedirectRepository";

export default class ExportRedirectAction extends UmbWorkspaceActionBase<RedirectModuleContext> {
    override async execute() {
        try {
            const repository = new RedirectRepository(this._host);
            const response = await repository.export();

            const blob = response.data as Blob;
            const filename = 'redirects.csv';

            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            a.remove();
            URL.revokeObjectURL(url);
        } catch (err) {
            console.error('Export error', err);
        }
    }
}
