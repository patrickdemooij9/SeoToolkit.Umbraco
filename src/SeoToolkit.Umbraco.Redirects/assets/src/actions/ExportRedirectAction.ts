import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import RedirectModuleContext, { ST_REDIRECT_MODULE_TOKEN_CONTEXT } from "../workspaces/RedirectModuleContext";

export default class ExportRedirectAction extends UmbWorkspaceActionBase<RedirectModuleContext> {
    override async execute() {
        // Use the backoffice route to download CSV
        try {
            const response = await fetch('/umbraco/seoToolkitRedirects/export', { credentials: 'same-origin' });
            if (!response.ok) {
                // You may want to display a notification in the future
                console.error('Export failed', response);
                return;
            }

            const blob = await response.blob();
            const contentDisposition = response.headers.get('content-disposition');
            let filename = 'redirects.csv';
            if (contentDisposition) {
                const match = /filename="?(.*?)"?(;|$)/.exec(contentDisposition);
                if (match && match[1]) filename = match[1];
            }

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
