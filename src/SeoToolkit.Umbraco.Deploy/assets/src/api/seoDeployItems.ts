export const SEO_DEPLOY_ENTITY_TYPES = {
  document: "document",
  metafieldsValue: "seotoolkit-metafields-value",
  sitemapContent: "seotoolkit-sitemap-content",
} as const;

export interface SeoDeployItem {
  id: string;
  entityType: string;
}

// The items to transfer/queue for a node: the document plus its existing SEO entities. Empty when
// the node has no SEO entities, so callers move nothing rather than the bare document.
export function buildTransferSet(contentKey: string, seoItems: SeoDeployItem[]): SeoDeployItem[] {
  if (seoItems.length === 0) {
    return [];
  }
  return [{ id: contentKey, entityType: SEO_DEPLOY_ENTITY_TYPES.document }, ...seoItems];
}

// The UDIs of a node's two SEO entities. A GuidUdi renders the key as 32 dashless hex chars.
export function buildSeoUdis(contentKey: string): string[] {
  const guid = contentKey.replace(/-/g, "").toLowerCase();
  return [
    `umb://${SEO_DEPLOY_ENTITY_TYPES.metafieldsValue}/${guid}`,
    `umb://${SEO_DEPLOY_ENTITY_TYPES.sitemapContent}/${guid}`,
  ];
}
