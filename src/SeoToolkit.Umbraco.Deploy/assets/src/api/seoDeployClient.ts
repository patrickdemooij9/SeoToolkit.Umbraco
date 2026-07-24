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

  // The upstream target's Deploy API endpoint (deployUrl), NOT its backoffice umbracoUrl. Instant
  // deploy opens a Deploy session against this URL; passing umbracoUrl makes the remote session
  // request 404 ("The remote API was not found") in Deploy's SourceDeployWorkItem.
  async getTargetDeployUrl(): Promise<string | undefined> {
    const resp = await fetch(`${BASE}/configuration/client`, { headers: this.#headers() });
    if (!resp.ok) return undefined;
    const body = await resp.json();
    return body?.clientConfiguration?.target?.deployUrl ?? undefined;
  }

  // Queues the node's SEO (and every descendant's when includeDescendants is set) in one server
  // call, and returns how many SEO entities were queued (0 when there is no SEO data).
  async queueSeo(
    contentKey: string,
    includeDescendants: boolean,
    releaseDate: string | null,
  ): Promise<{ added: number }> {
    const descendants = includeDescendants ? "&includeDescendants=true" : "";
    const release = releaseDate ? `&releaseDate=${encodeURIComponent(releaseDate)}` : "";
    const resp = await fetch(
      `/umbraco/seoToolkitDeploy/seoQueueAdd?contentKey=${contentKey}${descendants}${release}`,
      { method: "POST", headers: this.#headers() },
    );
    if (!resp.ok) throw new Error(`Could not add SEO to the transfer queue (${resp.status}).`);
    const body = await resp.json();
    return { added: (body?.added ?? 0) as number };
  }

  async instantDeploy(items: SeoDeployItem[]): Promise<Response> {
    const targetUrl = await this.getTargetDeployUrl();
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

  // Partial-restore the given SEO UDIs from the chosen source environment. The SEO artifacts
  // depend on the document in Exist mode only, so the document content is left untouched;
  // ignoreDependencies stays false unless the environment allows it and the user opts in.
  async restorePartial(udis: string[], sourceUrl: string, ignoreDependencies = false): Promise<Response> {
    return fetch(`${BASE}/restore/partial`, {
      method: "POST",
      headers: this.#headers(),
      body: JSON.stringify({
        sourceUrl,
        enableLogging: false,
        ignoreDependencies,
        restoreNodes: udis.map((udi) => ({ udi, includeDescendants: false, selector: "this" })),
      }),
    });
  }

}
