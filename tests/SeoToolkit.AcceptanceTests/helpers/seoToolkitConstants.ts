/**
 * Mirrors the SeoToolkit backoffice constants that live in the package source.
 *
 * Entity types:  SeoToolkit.Umbraco.Common/assets/src/constants/seoToolkitConstants.ts
 * GUID mapping:  SeoToolkit.Umbraco.Common/assets/src/sources/seoToolkitTreeSource.ts
 * Node names:    SeoToolkit.Umbraco.*.Core/Startup/*TreeSection.cs  (ISeoTreeSection.Name)
 * Element tags:  the @customElement decorator on each *.element.ts
 *
 * Kept as a copy rather than an import because the acceptance tests are a separate
 * npm project from the src/ workspaces. If you change either source file, change this too.
 */

export const SEOTOOLKIT_SECTION_PATHNAME = 'SeoToolkit';
export const SEOTOOLKIT_SECTION_URL = `/umbraco/section/${SEOTOOLKIT_SECTION_PATHNAME}`;

export const EntityType = {
  module: 'seoToolkit-module',
  robotsTxt: 'seoToolkit-robotstxt',
  scriptManager: 'seoToolkit-scriptManager',
  redirect: 'seoToolkit-redirect',
  siteAudit: 'seoToolkit-siteAudit',
  notFound: 'seoToolkit-notFound',
  settings: 'seoToolkit-settings',
  domainRoot: 'seoToolkit-domain-root',
  domain: 'seoToolkit-domain',
} as const;

export interface SeoToolkitTreeNode {
  /** Label rendered in the tree, from ISeoTreeSection.Name. */
  name: string;
  /** Section GUID, lowercased - the tree serves ids lowercased. */
  id: string;
  entityType: string;
  /**
   * Custom element tag rendered by this node's workspace. Tag names are a stable
   * public contract (the manifests reference these elements by module), which makes
   * them better smoke selectors than DOM structure or visible label text.
   */
  element: string;
}

/**
 * Nodes the tree root always returns on a working install.
 *
 * `Info` is registered by SeoToolkitComposer; the four module nodes by their own
 * composers; `Settings` is appended by SeoToolkitTreeController.GetRoot whenever any
 * module has registered a SeoKeyValueSetting (Core, MetaFields and NotFound all do).
 */
export const ALWAYS_PRESENT_TREE_NODES: SeoToolkitTreeNode[] = [
  {
    name: 'Info',
    id: 'cdf429d1-2380-4ac2-ac3e-22d619ee4529',
    entityType: EntityType.module,
    element: 'seotoolkit-module-root',
  },
  {
    name: 'Robots.txt',
    id: '20a2086e-7d72-44ba-b97b-5836caf6e28e',
    entityType: EntityType.robotsTxt,
    element: 'seotoolkit-module-robotstxt',
  },
  {
    name: 'Script Manager',
    id: '94e95f4a-2ecb-4038-bcfd-8357b7c41f1a',
    entityType: EntityType.scriptManager,
    element: 'seotoolkit-module-scriptmanager',
  },
  {
    name: 'Redirects',
    id: '1147f58d-d2d5-425b-aede-db537bdac9ef',
    entityType: EntityType.redirect,
    element: 'seotoolkit-module-redirect',
  },
  {
    name: 'Site Audit',
    id: 'b0d1c655-472b-40e7-9ac4-c6328ea9cf32',
    entityType: EntityType.siteAudit,
    element: 'seotoolkit-module-site-audit',
  },
  {
    name: 'Settings',
    id: '5ed58cb7-2ec2-4c97-be5b-506d6189086f',
    entityType: EntityType.settings,
    element: 'st-settings',
  },
];

/**
 * The Domains node is appended by GetRoot only when at least one SeoToolkit domain or
 * unmapped Umbraco domain exists, so a fresh install does NOT show it. The root itself
 * has no workspace manifest either - only individual domains (entityType
 * seoToolkit-domain) route to seotoolkit-domain-detail.
 */
export const DOMAIN_ROOT_NODE: SeoToolkitTreeNode = {
  name: 'Domains',
  id: 'ab248b43-9757-432a-9821-22f9eeb513e7',
  entityType: EntityType.domainRoot,
  element: 'seotoolkit-domain-detail',
};

/**
 * Routable workspaces route as edit/:unique under the section, e.g.
 * /umbraco/section/SeoToolkit/workspace/seoToolkit-settings/edit/<guid>
 * (see SeoToolkitSettingsContext.routes).
 */
export function workspaceUrl(node: SeoToolkitTreeNode): string {
  return `${SEOTOOLKIT_SECTION_URL}/workspace/${node.entityType}/edit/${node.id}`;
}

/** Elements rendered by the SEO tabs that other modules plug into. */
export const SeoTabElement = {
  /** Host view for the SEO tab on a content item. */
  contentView: 'st-content-view',
  /** Host view for the SEO tab on a document type. */
  documentTypeView: 'st-document-view',
  metaFieldsContent: 'st-metafield-content-view',
  metaFieldsDocumentType: 'st-metafield-document-view',
  sitemapContent: 'st-sitemap-content-view',
  sitemapDocumentType: 'st-sitemap-document-view',
  siteAuditContent: 'st-siteaudit-content-view',
} as const;

/** SeoToolkit management API routes, from the *Controller.cs BackOfficeRoute attributes. */
export const ApiRoute = {
  tree: '/umbraco/seoToolkit/tree/info',
  modules: '/umbraco/seoToolkit/modules',
  isEnabled: '/umbraco/seoToolkit/isEnabled',
  settings: '/umbraco/seoToolkitSettings/seoSettings',
  // NOTE: /get takes a domainId and returns ONE domain - there is no list endpoint.
  domainGet: '/umbraco/seoToolkitDomains/get',
  domainSave: '/umbraco/seoToolkitDomains/save',
  domainDelete: '/umbraco/seoToolkitDomains/delete',
  domainConfig: '/umbraco/seoToolkitDomains/config',
  robotsTxt: '/umbraco/seoToolkit/robotsTxt',
  redirects: '/umbraco/seoToolkitRedirects/redirects',
  scripts: '/umbraco/seoToolkitScriptManager/scripts',
  siteAudits: '/umbraco/seoToolkitSiteAudit/siteAudits',
  notFound: '/umbraco/seoToolkitNotFound/notFound',
  metaFields: '/umbraco/seoToolkitMetaFields/metaFields',
} as const;

/**
 * Selectors for the SEO tab and its module sub-tabs.
 *
 * Matched on href rather than label text on purpose: Playwright's hasText filter is a
 * case-insensitive substring match, so filtering uui-tab for "SEO" also matches the
 * "SeoToolkit" section tab in the backoffice header and silently navigates away.
 * The href suffixes are the workspaceView pathnames declared in the manifests.
 */
export const SeoTab = {
  seo: 'uui-tab[href$="/view/seo"]',
  metaFields: 'uui-tab[href$="/view/seo/view/metaFields"]',
  sitemap: 'uui-tab[href$="/view/seo/view/sitemap"]',
  pageChecks: 'uui-tab[href$="/view/seo/view/pageChecks"]',
} as const;
