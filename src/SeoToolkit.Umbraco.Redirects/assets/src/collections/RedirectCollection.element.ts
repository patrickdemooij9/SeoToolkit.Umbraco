import { UmbCollectionDefaultElement } from "@umbraco-cms/backoffice/collection";
import { customElement, html } from "@umbraco-cms/backoffice/external/lit";

@customElement("st-redirects-collection")
export default class RedirectCollection extends UmbCollectionDefaultElement {
    protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
		`;
	}
} 