import type { ManifestWithView } from '@umbraco-cms/backoffice/extension-api';
import { MetaWorkspaceView, UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';

export interface SeoContentViewManifest<MetaType extends MetaWorkspaceView = MetaWorkspaceView> extends ManifestWithView<UmbWorkspaceViewElement> {
    type: 'seoToolkitContentView';
    meta: MetaType;
}

declare global {
    interface UmbExtensionManifestMap {
        SeoToolkitContentView: SeoContentViewManifest;
    }
}