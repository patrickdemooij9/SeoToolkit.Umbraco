import type { ManifestWithView } from '@umbraco-cms/backoffice/extension-api';
import { MetaWorkspaceView, UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

export interface SeoDocumentViewManifest<MetaType extends MetaWorkspaceView = MetaWorkspaceView> extends ManifestWithView<UmbWorkspaceViewElement> {
    type: 'seoToolkitDocumentView';
    meta: MetaType;
}

declare global {
	interface UmbExtensionManifestMap {
		SeoToolkitDocumentView: SeoDocumentViewManifest;
	}
}