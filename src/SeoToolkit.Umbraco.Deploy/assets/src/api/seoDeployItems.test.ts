import { describe, it, expect } from "vitest";
import { buildSeoUdis, buildTransferSet, SeoDeployItem, SEO_DEPLOY_ENTITY_TYPES } from "./seoDeployItems";

const KEY = "11111111-1111-1111-1111-111111111111";

describe("buildTransferSet", () => {
  it("returns an empty set when the node has no SEO entities (queue/transfer nothing)", () => {
    expect(buildTransferSet([])).toEqual([]);
  });

  it("returns the SEO entities unchanged, without adding a document node", () => {
    const seoItems: SeoDeployItem[] = [
      { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue },
      { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.sitemapContent },
    ];
    expect(buildTransferSet(seoItems)).toEqual(seoItems);
    expect(buildTransferSet(seoItems).some((i) => i.entityType === "document")).toBe(false);
  });

  it("preserves each item's own node key across a multi-node descendant set", () => {
    const other = "22222222-2222-2222-2222-222222222222";
    const seoItems: SeoDeployItem[] = [
      { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue },
      { id: other, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue },
      { id: other, entityType: SEO_DEPLOY_ENTITY_TYPES.sitemapContent },
    ];
    expect(buildTransferSet(seoItems)).toEqual(seoItems);
  });

  it("uses the SeoToolkit UDI entity-type strings", () => {
    expect(SEO_DEPLOY_ENTITY_TYPES.metafieldsValue).toBe("seotoolkit-metafields-value");
    expect(SEO_DEPLOY_ENTITY_TYPES.sitemapContent).toBe("seotoolkit-sitemap-content");
  });
});

describe("buildSeoUdis", () => {
  it("builds both SEO UDIs with the dashless, lower-case node key", () => {
    expect(buildSeoUdis(KEY)).toEqual([
      "umb://seotoolkit-metafields-value/11111111111111111111111111111111",
      "umb://seotoolkit-sitemap-content/11111111111111111111111111111111",
    ]);
  });

  it("strips dashes and lower-cases a mixed-case key", () => {
    const udis = buildSeoUdis("AABBCCDD-1122-3344-5566-778899AABBCC");
    expect(udis[0]).toBe("umb://seotoolkit-metafields-value/aabbccdd112233445566778899aabbcc");
    expect(udis[1]).toBe("umb://seotoolkit-sitemap-content/aabbccdd112233445566778899aabbcc");
  });
});
