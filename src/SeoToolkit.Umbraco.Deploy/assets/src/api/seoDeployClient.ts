import { SeoDeployItem } from "./seoDeployItems";

const BASE = "/umbraco/deploy/management/api/v1";

export class SeoDeployClient {
  #token: string;
  constructor(token: string) {
    this.#token = token;
  }

  #headers(): HeadersInit {
    return {
      "Content-Type": "application/json",
      Authorization: `Bearer ${this.#token}`,
    };
  }

  /** The per-node SEO entities that actually have data for the node (empty when none). */
  async getSeoItems(contentKey: string): Promise<SeoDeployItem[]> {
    const resp = await fetch(
      `/umbraco/seoToolkitDeploy/seoTransferItems?contentKey=${contentKey}`,
      { headers: this.#headers() },
    );
    if (!resp.ok) throw new Error(`Could not resolve SEO items (${resp.status}).`);
    const body = await resp.json();
    return (body?.items ?? []) as SeoDeployItem[];
  }

  async getTargetUrl(): Promise<string | undefined> {
    const resp = await fetch(`${BASE}/configuration/client`, { headers: this.#headers() });
    if (!resp.ok) return undefined;
    const body = await resp.json();
    return body?.clientConfiguration?.target?.umbracoUrl ?? undefined;
  }

  async instantDeploy(items: SeoDeployItem[]): Promise<Response> {
    const targetUrl = await this.getTargetUrl();
    if (!targetUrl) throw new Error("No upstream target environment configured.");
    return fetch(`${BASE}/deploy/instant`, {
      method: "POST",
      headers: this.#headers(),
      body: JSON.stringify({
        targetUrl,
        ignoreDependencies: false,
        enableLogging: false,
        items,
      }),
    });
  }

  // Partial-restore the given SEO UDIs from the upstream source. ignoreDependencies leaves the
  // local document content untouched (Deploy returns 400 unless the environment allows it).
  async restorePartial(udis: string[]): Promise<Response> {
    const sourceUrl = await this.getTargetUrl();
    if (!sourceUrl) throw new Error("No upstream source environment configured.");
    return fetch(`${BASE}/restore/partial`, {
      method: "POST",
      headers: this.#headers(),
      body: JSON.stringify({
        sourceUrl,
        enableLogging: false,
        ignoreDependencies: true,
        restoreNodes: udis.map((udi) => ({ udi, includeDescendants: false, selector: "this" })),
      }),
    });
  }

  async queueAdd(items: SeoDeployItem[]): Promise<void> {
    // Sequential: a failure throws immediately, so earlier items may already be queued.
    for (const item of items) {
      const resp = await fetch(`${BASE}/queue/add`, {
        method: "POST",
        headers: this.#headers(),
        body: JSON.stringify({ id: item.id, entityType: item.entityType, culture: null }),
      });
      if (!resp.ok) throw new Error(`Queue add failed (${resp.status}).`);
    }
  }
}
