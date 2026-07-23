import { afterEach, describe, expect, it, vi } from "vitest";
import { SeoDeployClient } from "./seoDeployClient";
import { SeoDeployItem, SEO_DEPLOY_ENTITY_TYPES } from "./seoDeployItems";

const KEY = "11111111-1111-1111-1111-111111111111";

const items: SeoDeployItem[] = [
  { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.document },
  { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.metafieldsValue },
  { id: KEY, entityType: SEO_DEPLOY_ENTITY_TYPES.sitemapContent },
];

describe("SeoDeployClient.instantDeploy", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("deploys to the target's deployUrl (not umbracoUrl) so the remote Deploy session resolves", async () => {
    const fetchMock = vi.fn(async (url: string, _init?: RequestInit) => {
      if ((url as string).includes("/configuration/client")) {
        return {
          ok: true,
          status: 200,
          json: async () => ({
            clientConfiguration: {
              target: {
                deployUrl: "https://target.example/umbraco/deploy",
                umbracoUrl: "https://target.example/umbraco",
              },
            },
          }),
        } as Response;
      }
      return { ok: true, status: 200 } as Response;
    });
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");
    const resp = await client.instantDeploy(items);

    expect(resp.ok).toBe(true);
    const deployCall = fetchMock.mock.calls.find(([url]) => (url as string).includes("/deploy/instant"));
    expect(deployCall).toBeDefined();
    const body = JSON.parse((deployCall![1] as RequestInit).body as string);
    expect(body.targetUrl).toBe("https://target.example/umbraco/deploy");
    expect(body.items).toHaveLength(items.length);
  });

  it("throws when no upstream target is configured", async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({ clientConfiguration: { target: {} } }),
    } as Response);
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");

    await expect(client.instantDeploy(items)).rejects.toThrow();
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

  it("posts to restore/partial with the given source, ignoreDependencies false, one node per UDI", async () => {
    const fetchMock = vi.fn(async (_url: string, _init?: RequestInit) => ({ ok: true, status: 200 } as Response));
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");
    const resp = await client.restorePartial(udis, "https://upstream.example");

    expect(resp.ok).toBe(true);
    const restoreCall = fetchMock.mock.calls.find(([url]) => (url as string).includes("/restore/partial"));
    expect(restoreCall).toBeDefined();
    const body = JSON.parse((restoreCall![1] as RequestInit).body as string);
    expect(body.sourceUrl).toBe("https://upstream.example");
    expect(body.ignoreDependencies).toBe(false);
    expect(body.restoreNodes).toHaveLength(2);
    expect(body.restoreNodes[0]).toEqual({ udi: udis[0], includeDescendants: false, selector: "this" });
  });

  it("passes ignoreDependencies through when the caller opts in", async () => {
    const fetchMock = vi.fn(async (_url: string, _init?: RequestInit) => ({ ok: true, status: 200 } as Response));
    vi.stubGlobal("fetch", fetchMock);

    const client = new SeoDeployClient("token");
    await client.restorePartial(udis, "https://upstream.example", true);

    const restoreCall = fetchMock.mock.calls.find(([url]) => (url as string).includes("/restore/partial"));
    const body = JSON.parse((restoreCall![1] as RequestInit).body as string);
    expect(body.ignoreDependencies).toBe(true);
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
