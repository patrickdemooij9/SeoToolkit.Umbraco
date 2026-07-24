export const SEO_DEPLOY_ENTITY_TYPES = {
  metafieldsValue: "seotoolkit-metafields-value",
  sitemapContent: "seotoolkit-sitemap-content",
} as const;

export interface SeoDeployItem {
  id: string;
  entityType: string;
}

// The SEO entities to transfer — never the content node itself (SEO artifacts depend on the
// document in Exist mode only). Returned unchanged; empty input means transfer nothing.
export function buildTransferSet(seoItems: SeoDeployItem[]): SeoDeployItem[] {
  return seoItems;
}

// The UDIs of a node's two SEO entities. A GuidUdi renders the key as 32 dashless hex chars.
export function buildSeoUdis(contentKey: string): string[] {
  const guid = contentKey.replace(/-/g, "").toLowerCase();
  return [
    `umb://${SEO_DEPLOY_ENTITY_TYPES.metafieldsValue}/${guid}`,
    `umb://${SEO_DEPLOY_ENTITY_TYPES.sitemapContent}/${guid}`,
  ];
}
