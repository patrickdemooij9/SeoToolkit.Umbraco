import {
  LitElement,
  css,
  html,
  customElement,
  state,
  repeat,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { SeoToolkitModule, SeoToolkitModuleStatus } from "../../api";
import { ModuleRepository } from "../../repositories/moduleRepository";

type InsightCategory = "on_page" | "technical" | "indexability";
type InsightSeverity = "low" | "medium" | "high";

interface Insight {
  id: string;
  type: string;
  category: InsightCategory;
  severity: InsightSeverity;
  count: number;
  description: string;
  actionLabel: string;
  actionUrl: string;
  module: string;
}

interface InsightProvider {
  module: string;
  categories: InsightCategory[];
  getInsights: () => Insight[] | Promise<Insight[]>;
}

const sampleInsightsByModule: Record<string, Insight[]> = {
  metaFields: [
    {
      id: "1",
      type: "missing_meta_description",
      category: "on_page",
      severity: "high",
      count: 34,
      description: "Pages missing meta descriptions",
      actionLabel: "Fix meta descriptions",
      actionUrl: "/meta",
      module: "metaFields",
    },
    {
      id: "2",
      type: "missing_titles",
      category: "on_page",
      severity: "high",
      count: 5,
      description: "Pages missing titles",
      actionLabel: "Fix titles",
      actionUrl: "/meta",
      module: "metaFields",
    },
  ],
  siteAudit: [
    {
      id: "3",
      type: "broken_links",
      category: "technical",
      severity: "high",
      count: 12,
      description: "Broken internal links",
      actionLabel: "View broken links",
      actionUrl: "/links",
      module: "siteAudit",
    },
  ],
  redirects: [
    {
      id: "4",
      type: "redirect_chains",
      category: "technical",
      severity: "medium",
      count: 3,
      description: "Redirect chains detected",
      actionLabel: "Fix redirects",
      actionUrl: "/redirects",
      module: "redirects",
    },
  ],
  sitemap: [
    {
      id: "5",
      type: "sitemap_ok",
      category: "indexability",
      severity: "low",
      count: 1,
      description: "Sitemap is valid",
      actionLabel: "View sitemap",
      actionUrl: "/sitemap",
      module: "sitemap",
    },
  ],
};

const insightProviders: InsightProvider[] = [
  {
    module: "metaFields",
    categories: ["on_page"],
    getInsights: () => sampleInsightsByModule.metaFields,
  },
  {
    module: "siteAudit",
    categories: ["technical"],
    getInsights: () => sampleInsightsByModule.siteAudit,
  },
  {
    module: "redirects",
    categories: ["technical"],
    getInsights: () => sampleInsightsByModule.redirects,
  },
  {
    module: "sitemap",
    categories: ["indexability"],
    getInsights: () => sampleInsightsByModule.sitemap,
  },
];

const categoryCards: Array<{ key: InsightCategory; title: string }> = [
  { key: "on_page", title: "On-page SEO" },
  { key: "technical", title: "Technical SEO" },
  { key: "indexability", title: "Indexability" },
];

const severityOrder: Record<InsightSeverity, number> = {
  high: 3,
  medium: 2,
  low: 1,
};

const severityPenalty: Record<InsightSeverity, number> = {
  high: 10,
  medium: 5,
  low: 2,
};

@customElement("welcome-dashboard")
export class MyWelcomeDashboardElement extends UmbElementMixin(LitElement) {
  @state()
  modules: SeoToolkitModule[] = [];

  @state()
  insights: Insight[] = [];

  @state()
  isLoading = true;

  constructor() {
    super();
  }

  async connectedCallback(): Promise<void> {
    super.connectedCallback();
    await this.#loadDashboard();
  }

  async #loadDashboard(): Promise<void> {
    this.isLoading = true;
    const moduleResponse = await new ModuleRepository(this).getModules();
    this.modules = moduleResponse.data ?? [];
    await this.#runScan();
    this.isLoading = false;
  }

  async #runScan(): Promise<void> {
    const activeModules = this.#activeModuleAliases;
    const activeProviders = insightProviders.filter((provider) =>
      activeModules.has(this.#normalizeModuleAlias(provider.module)),
    );

    const insightsByProvider = await Promise.all(
      activeProviders.map(async (provider) => provider.getInsights()),
    );

    this.insights = insightsByProvider
      .flat()
      .filter((insight) =>
        activeModules.has(this.#normalizeModuleAlias(insight.module)),
      );
  }

  get #activeModuleAliases(): Set<string> {
    return new Set(
      this.modules
        .filter((module) => module.status === SeoToolkitModuleStatus.INSTALLED)
        .map((module) => this.#normalizeModuleAlias(module.alias)),
    );
  }

  #normalizeModuleAlias(alias: string): string {
    return alias.toLowerCase().replace(/[^a-z0-9]/g, "");
  }

  #isCategoryMonitored(category: InsightCategory): boolean {
    const activeModules = this.#activeModuleAliases;
    return insightProviders.some(
      (provider) =>
        activeModules.has(this.#normalizeModuleAlias(provider.module)) &&
        provider.categories.includes(category),
    );
  }

  #getCategoryInsights(category: InsightCategory): Insight[] {
    return this.insights.filter((insight) => insight.category === category);
  }

  #calculateScore(insights: Insight[]): number | null {
    if (insights.length === 0) {
      return null;
    }
    const totalPenalty = insights.reduce(
      (total, insight) => total + severityPenalty[insight.severity],
      0,
    );
    return Math.max(0, 100 - totalPenalty);
  }

  #scoreLabel(score: number | null): string {
    return score === null ? "Not enough data" : score.toString();
  }

  #topPriorities(): Insight[] {
    return [...this.insights]
      .sort((left, right) => {
        const severityDiff =
          severityOrder[right.severity] - severityOrder[left.severity];
        if (severityDiff !== 0) {
          return severityDiff;
        }
        return right.count - left.count;
      })
      .slice(0, 5);
  }

  #severityClass(severity: InsightSeverity): string {
    return `severity severity-${severity}`;
  }

  render() {
    const overallScore = this.#calculateScore(this.insights);
    const topPriorities = this.#topPriorities();

    return html`
      <div class="seo-dashboard">
        <div class="header">
          <div>
            <p class="eyebrow">SEO health dashboard</p>
            <h1>Site SEO Overview</h1>
          </div>
          <uui-button
            look="primary"
            ?disabled=${this.isLoading}
            @click=${() => this.#runScan()}
          >
            Re-run scan
          </uui-button>
        </div>

        <div class="card score-overview">
          <div class="overall-score">
            <p class="label">Overall SEO score</p>
            <p class="value">${this.#scoreLabel(overallScore)}</p>
          </div>
          <div class="category-scores">
            ${repeat(
              categoryCards,
              (category) => category.key,
              (category) =>
                html`<div class="category-score-item">
                  <p>${category.title}</p>
                  <strong
                    >${this.#scoreLabel(
                      this.#calculateScore(this.#getCategoryInsights(category.key)),
                    )}</strong
                  >
                </div>`,
            )}
          </div>
        </div>

        <div class="card top-priorities">
          <div class="card-header">
            <h2>Top Priorities</h2>
          </div>
          ${topPriorities.length === 0
            ? html`<p class="empty">No priority issues from active modules.</p>`
            : html`${repeat(
                topPriorities,
                (insight) => insight.id,
                (insight) => html`
                  <div class="priority-item">
                    <span class="${this.#severityClass(insight.severity)}"
                      >${insight.severity}</span
                    >
                    <p>${insight.count} ${insight.description}</p>
                    <uui-button
                      look="outline"
                      href="${insight.actionUrl}"
                      label="${insight.actionLabel}"
                    >
                      ${insight.actionLabel}
                    </uui-button>
                  </div>
                `,
              )}`}
        </div>

        <div class="area-grid">
          ${repeat(
            categoryCards,
            (category) => category.key,
            (category) => {
              const insights = this.#getCategoryInsights(category.key);
              const monitored = this.#isCategoryMonitored(category.key);
              return html`
                <div class="card area-card">
                  <div class="card-header">
                    <h3>${category.title}</h3>
                    <p class="score">
                      ${this.#scoreLabel(this.#calculateScore(insights))}
                    </p>
                  </div>
                  ${insights.length > 0
                    ? html`<ul>
                        ${repeat(
                          insights,
                          (insight) => insight.id,
                          (insight) =>
                            html`<li>
                              <span>${insight.description}</span>
                              <strong>${insight.count}</strong>
                            </li>`,
                        )}
                      </ul>`
                    : html`<p class="empty">
                        ${monitored ? "No issues found" : "Not monitored"}
                      </p>`}
                  <uui-button
                    look="secondary"
                    href="${insights[0]?.actionUrl ?? "#"}"
                    ?disabled=${insights.length === 0}
                  >
                    View issues
                  </uui-button>
                </div>
              `;
            },
          )}
        </div>

        <div class="card coverage">
          <div class="card-header">
            <h2>Module Coverage</h2>
          </div>
          <div class="coverage-list">
            ${repeat(
              this.modules,
              (module) => module.alias,
              (module) => {
                const isActive =
                  module.status === SeoToolkitModuleStatus.INSTALLED;
                return html`
                  <div class="coverage-item">
                    <div class="module-meta">
                      <p class="module-name">${module.title}</p>
                      <span
                        class="${isActive
                          ? "module-status module-status-active"
                          : "module-status module-status-disabled"}"
                        >${isActive ? "Active" : "Disabled"}</span
                      >
                    </div>
                    ${isActive
                      ? null
                      : html`<uui-button
                          look="outline"
                          href="${module.link}"
                          target="_blank"
                        >
                          Enable module
                        </uui-button>`}
                  </div>
                `;
              },
            )}
          </div>
        </div>
      </div>
    `;
  }

  static styles = [
    css`
      .seo-dashboard {
        display: grid;
        gap: 16px;
      }

      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
      }

      .eyebrow {
        text-transform: uppercase;
        font-size: 12px;
        margin: 0;
        color: var(--uui-color-default-emphasis);
      }

      h1,
      h2,
      h3 {
        margin: 0;
      }

      .card {
        background: var(--uui-color-surface);
        border-radius: 12px;
        border: 1px solid var(--uui-color-divider-emphasis);
        padding: 16px;
      }

      .score-overview {
        display: grid;
        grid-template-columns: minmax(220px, 1fr) 2fr;
        gap: 12px;
      }

      .overall-score .label {
        margin: 0;
      }

      .overall-score .value {
        margin: 0;
        font-size: 36px;
        font-weight: 700;
      }

      .category-scores {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
      }

      .category-score-item {
        min-width: 150px;
        background: var(--uui-color-surface-alt);
        padding: 10px;
        border-radius: 8px;
      }

      .category-score-item p {
        margin: 0 0 6px 0;
      }

      .top-priorities {
        border-width: 2px;
      }

      .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 10px;
      }

      .priority-item {
        display: grid;
        grid-template-columns: auto 1fr auto;
        gap: 12px;
        align-items: center;
        padding: 10px 0;
        border-top: 1px solid var(--uui-color-divider);
      }

      .priority-item:first-of-type {
        border-top: none;
      }

      .priority-item p {
        margin: 0;
      }

      .severity {
        text-transform: uppercase;
        font-size: 12px;
        border-radius: 10px;
        padding: 4px 8px;
        font-weight: 700;
      }

      .severity-high {
        background: rgba(210, 37, 37, 0.15);
        color: #c53131;
      }

      .severity-medium {
        background: rgba(228, 155, 0, 0.2);
        color: #a96f00;
      }

      .severity-low {
        background: rgba(21, 131, 76, 0.15);
        color: #0f7f47;
      }

      .area-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 12px;
      }

      .area-card ul {
        margin: 0;
        padding-left: 0;
        list-style: none;
      }

      .area-card li {
        display: flex;
        justify-content: space-between;
        gap: 8px;
        padding: 6px 0;
        border-top: 1px solid var(--uui-color-divider);
      }

      .area-card li:first-child {
        border-top: none;
      }

      .score {
        font-size: 20px;
        font-weight: 700;
      }

      .coverage-list {
        display: grid;
        gap: 8px;
      }

      .coverage-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 8px;
        padding: 10px;
        border-radius: 8px;
        background: var(--uui-color-surface-alt);
      }

      .module-meta {
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .module-name {
        margin: 0;
        font-weight: 600;
      }

      .module-status {
        border-radius: 999px;
        padding: 3px 8px;
        font-size: 12px;
        font-weight: 600;
      }

      .module-status-active {
        background: rgba(21, 131, 76, 0.15);
        color: #0f7f47;
      }

      .module-status-disabled {
        background: rgba(228, 155, 0, 0.2);
        color: #a96f00;
      }

      .empty {
        margin: 0 0 12px 0;
        color: var(--uui-color-default-emphasis);
      }

      @media (max-width: 1100px) {
        .score-overview {
          grid-template-columns: 1fr;
        }

        .area-grid {
          grid-template-columns: 1fr;
        }
      }
    `,
  ];
}

export default MyWelcomeDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    "welcome-dashboard": MyWelcomeDashboardElement;
  }
}
