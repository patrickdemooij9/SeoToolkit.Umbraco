import { ManifestWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { SitemapEnabledCondition } from '../conditions/SitemapEnabledCondition';
import { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

const documentView: any = {
    type: 'seoToolkitDocumentView',
    alias: 'seoToolkit.sitemap.documentView',
    name: 'SeoToolkit Sitemap document view',
    js: () => import('../workspaces/sitemapDocumentView.element'),
    weight: 300,
    meta: {
        label: 'Sitemap',
        pathname: 'sitemap',
        icon: 'icon-globe'
    },
    conditions: [
        {
            alias: 'SeoToolkit.SeoModuleEnabled',
            moduleAlias: 'sitemap'
        }
    ]
}

const documentWorkspaceContext: ManifestWorkspaceContext = {
    type: 'workspaceContext',
    alias: 'seoToolkit.sitemap.documentWorkspaceContext',
    name: 'SeoToolkit Sitemap workspace context',
    api: () => import('../workspaces/sitemapDocumentViewContext'),
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'Umb.Workspace.DocumentType'
        },
        {
            alias: 'SeoToolkit.SeoModuleEnabled',
            moduleAlias: 'sitemap'
        }
    ]
};

const sitemapEnabledConditionManifest: ManifestCondition = {
  type: "condition",
  name: "Sitemap Enabled Condition",
  alias: "SeoToolkit.SitemapEnabled",
  api: SitemapEnabledCondition,
};

const contentView: any = {
    type: 'seoToolkitContentView',
    alias: 'seoToolkit.sitemap.contentView',
    name: 'SeoToolkit Sitemap content view',
    js: () => import('../workspaces/sitemapContentView.element'),
    weight: 300,
    meta: {
        label: 'Sitemap',
        pathname: 'sitemap',
        icon: 'icon-globe'
    },
    conditions: [
        {
            alias: 'SeoToolkit.SeoModuleEnabled',
            moduleAlias: 'sitemap'
        },
        {
            alias: 'SeoToolkit.SitemapEnabled'
        }
    ]
};

const contentWorkspaceContext: ManifestWorkspaceContext = {
    type: 'workspaceContext',
    alias: 'seoToolkit.sitemap.contentWorkspaceContext',
    name: 'SeoToolkit Sitemap content workspace context',
    api: () => import('../workspaces/sitemapContentViewContext'),
    conditions: [
        {
            alias: 'Umb.Condition.WorkspaceAlias',
            match: 'Umb.Workspace.Document'
        },
        {
            alias: 'SeoToolkit.SeoModuleEnabled',
            moduleAlias: 'sitemap'
        },
        {
            alias: 'SeoToolkit.SitemapEnabled'
        }
    ]
};

export const DocumentManifests = [documentView, documentWorkspaceContext, contentView, contentWorkspaceContext, sitemapEnabledConditionManifest];