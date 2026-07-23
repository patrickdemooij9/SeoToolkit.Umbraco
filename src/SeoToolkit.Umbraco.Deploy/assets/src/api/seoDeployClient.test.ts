import { afterEach, describe, expect, it, vi } from "vitest";
import { SeoDeployClient } from "./seoDeployClient";
import { SeoDeployItem, SEO_DEPLOY_ENTITY_TYPES } from "./seoDeployItems";

const KEY = "11111111-1111-1111-1111-111111111111";

const items: SeoDeployItem[] = [
  { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.document },
  { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue },
  { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.sitemapContent },
];

describe("SeoDeployClient.queueAdd", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("rejects when a /queue/add response is not ok", async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, status: 500 } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.queueAdd(items)).rejects.toThrow();
  });

  it("resolves and issues one POST per item when all responses are ok", async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, status: 200 } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.queueAdd(items)).resolves.toBeUndefined();
    expect(fetchMock).toHaveBeenCalledTimes(items.length);
  });
});

describe("SeoDeployClient.restorePartial", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  const udis = [
    "umb://seotoolkit-metafields-value/11111111111111111111111111111111",
    "umb://seotoolkit-sitemap-content/11111111111111111111111111111111",
  ];

  it("posts to restore/partial with ignoreDependencies and one restore node per UDI", async () => {
    const fetchMock = vi.fn(async (url: string, _init?: RequestInit) => {
      if (url.includes("/configuration/client")) {
        return {
          ok: true,
          status: 200,
          json: async () => ({ clientConfiguration: { target: { umbracoUrl: "https://upstream.example" } } }),
        } as Response;
      }
      return { ok: true, status: 200 } as Response;
    });
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");
    const resp = await client.restorePartial(udis);

    expect(resp.ok).toBe(true);
    const restoreCall = fetchMock.mock.calls.find(([url]) => (url as string).includes("/restore/partial"));
    expect(restoreCall).toBeDefined();
    const body = JSON.parse((restoreCall![1] as RequestInit).body as string);
    expect(body.sourceUrl).toBe("https://upstream.example");
    expect(body.ignoreDependencies).toBe(true);
    expect(body.restoreNodes).toHaveLength(2);
    expect(body.restoreNodes[0]).toEqual({ udi: udis[0], includeDescendants: false, selector: "this" });
  });

  it("throws when no upstream source environment is configured", async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, status: 404 } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.restorePartial(udis)).rejects.toThrow();
  });
});

describe("SeoDeployClient.getSeoItems", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("returns the items array from the endpoint", async () => {
    const seoItems = [{ id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue }];
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({ items: seoItems }),
    } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.getSeoItems(KEY)).resolves.toEqual(seoItems);
  });

  it("returns an empty array when the endpoint returns no items", async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({ items: [] }),
    } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.getSeoItems(KEY)).resolves.toEqual([]);
  });

  it("rejects when the endpoint response is not ok", async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: false, status: 500 } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.getSeoItems(KEY)).rejects.toThrow();
  });
});
