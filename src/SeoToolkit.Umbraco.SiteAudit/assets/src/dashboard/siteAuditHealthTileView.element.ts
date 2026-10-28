import { LitElement, css, html, customElement, property, nothing } from "@umbraco-cms/backoffice/external/lit";
import type { DashboardData } from "../../../../SeoToolkit.Umbraco.Common/assets/src/api";
import type {
  SeoDashboardTileSize,
  SeoDashboardTileViewElement,
} from "../../../../SeoToolkit.Umbraco.Common/assets/src/manifests/seoDashboardTileManifest";

const statusColor: Record<string, string> = {
  Positive: "var(--uui-color-positive)",
  Warning: "var(--uui-color-warning)",
  Danger: "var(--uui-color-danger)",
};

/**
 * The SEO health score as a ring, next to the number of failing pages per check.
 */
@customElement("seo-site-audit-health-tile-view")
export class SiteAuditHealthTileViewElement extends LitElement implements SeoDashboardTileViewElement {
  @property({ attribute: false }) data?: DashboardData;
  @property({ attribute: false }) settings?: Record<string, unknown>;
  @property({ attribute: false }) size?: SeoDashboardTileSize;

  render() {
    const data = this.data;
    if (!data) return nothing;
    if (data.value == null) return html`<p class="empty">${data.description}</p>`;

    const color = statusColor[data.status] ?? "var(--uui-color-border-emphasis)";
    const showChecks = (this.size?.h ?? 4) >= 3 && (this.size?.w ?? 6) >= 4;

    return html`
      <div class="ring" style="--score:${data.value};--color:${color}">
        <span>${data.value}</span>
      </div>
      <div class="details">
        <small>${data.description}${data.date ? ` · audit of ${new Date(data.date).toLocaleDateString()}` : ""}</small>
        ${showChecks
          ? html`<ul>
              ${data.items.map(
                (check) => html`<li>
                  <span class="dot" style="background:${statusColor[check.status] ?? "transparent"}"></span>
                  <span class="label">${check.label}</span>
                  <strong>${check.value ? `${check.value} pages` : "✓"}</strong>
                </li>`
              )}
            </ul>`
          : nothing}
      </div>
    `;
  }

  static styles = css`
    :host {
      display: flex;
      gap: var(--uui-size-space-5);
      align-items: flex-start;
      min-height: 0;
      flex: 1;
    }
    .empty {
      margin: 0;
      color: var(--uui-color-text-alt);
    }
    .ring {
      flex: none;
      width: 96px;
      height: 96px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      background: conic-gradient(var(--color) calc(var(--score) * 1%), var(--uui-color-divider) 0);
    }
    .ring span {
      display: grid;
      place-items: center;
      width: 74px;
      height: 74px;
      border-radius: 50%;
      background: var(--uui-color-surface);
      font-size: 28px;
      font-weight: 700;
    }
    .details {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
      min-width: 0;
      flex: 1;
      max-height: 100%;
    }
    small {
      color: var(--uui-color-text-alt);
    }
    ul {
      list-style: none;
      padding: 0;
      margin: 0;
      overflow: auto;
    }
    li {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 3px 0;
      border-bottom: 1px solid var(--uui-color-divider);
    }
    .label {
      flex: 1;
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .dot {
      flex: none;
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
  `;
}

export default SiteAuditHealthTileViewElement;

declare global {
  interface HTMLElementTagNameMap {
    "seo-site-audit-health-tile-view": SiteAuditHealthTileViewElement;
  }
}
