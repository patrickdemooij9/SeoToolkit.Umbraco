import { LitElement, css, html, customElement, property, when, nothing } from "@umbraco-cms/backoffice/external/lit";
import { DashboardData, DashboardDataItem } from "../../../api";
import { SeoDashboardTileSize, SeoDashboardTileViewElement } from "../../../manifests/seoDashboardTileManifest";

/**
 * Settings of the metric view, set through the tile manifest's `meta.settings`.
 */
export interface MetricTileViewSettings {
  /** Text after the value; `{total}` is replaced by the total. Defaults to "of {total}", or the data's description. */
  caption?: string;
  /** Text before the date of the data, e.g. "Audit of". */
  dateLabel?: string;
  /** Minimum tile height (in rows) at which the items are shown. Defaults to 3. */
  itemsFromHeight?: number;
  /** Maximum number of items shown. */
  maxItems?: number;
}

const statusColor: Record<string, string> = {
  Positive: "positive",
  Warning: "warning",
  Danger: "danger",
};

/**
 * A number (or short text) with a caption and optional rows. Reused by most tiles.
 */
@customElement("seo-dashboard-metric-tile-view")
export class MetricTileViewElement extends LitElement implements SeoDashboardTileViewElement {
  @property({ attribute: false }) data?: DashboardData;
  @property({ attribute: false }) settings?: Record<string, unknown>;
  @property({ attribute: false }) size?: SeoDashboardTileSize;

  get #settings() {
    return (this.settings ?? {}) as MetricTileViewSettings;
  }

  #caption(data: DashboardData) {
    const caption = this.#settings.caption ?? (data.total != null ? "of {total}" : data.description ?? "");
    return caption.replace("{total}", String(data.total ?? ""));
  }

  // The data's description when the caption doesn't already show it, and the date.
  #details(data: DashboardData) {
    const description = this.#settings.caption || data.total != null ? data.description : undefined;
    const date = data.date
      ? [this.#settings.dateLabel, new Date(data.date).toLocaleDateString()].filter(Boolean).join(" ")
      : undefined;
    return [description, date].filter(Boolean).join(" · ");
  }

  #itemValue(item: DashboardDataItem) {
    if (item.value != null) return String(item.value);
    if (item.date) return new Date(item.date).toLocaleString();
    return item.text ?? "";
  }

  render() {
    const data = this.data;
    if (!data) return nothing;

    const height = this.size?.h ?? 2;
    const showItems = height >= (this.#settings.itemsFromHeight ?? 3) && data.items.length > 0;
    const items = data.items.slice(0, this.#settings.maxItems ?? data.items.length);
    const details = this.#details(data);

    return html`
      <div class="value ${height === 1 ? "compact" : ""}">
        <uui-tag color=${statusColor[data.status] ?? "default"}>${data.value ?? data.text ?? "—"}</uui-tag>
        <span class="caption">${this.#caption(data)}</span>
      </div>
      ${when(details && height > 1, () => html`<small class="details">${details}</small>`)}
      ${when(
        showItems,
        () => html`<ul class="items">
          ${items.map(
            (item) => html`<li>
              <span class="label">
                ${when(statusColor[item.status], () => html`<span class="dot ${statusColor[item.status]}"></span>`)}
                ${item.url ? html`<a href=${item.url}>${item.label}</a>` : item.label}
              </span>
              <strong>${this.#itemValue(item)}</strong>
            </li>`
          )}
        </ul>`
      )}
    `;
  }

  static styles = css`
    :host {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
      min-height: 0;
      flex: 1;
    }
    .value {
      display: flex;
      align-items: baseline;
      gap: var(--uui-size-space-3);
      min-width: 0;
    }
    .value uui-tag {
      flex: none;
      font-size: 22px;
      padding: 4px 10px;
    }
    .caption {
      color: var(--uui-color-text-alt);
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .compact .caption {
      display: none;
    }
    .details {
      color: var(--uui-color-text-alt);
    }
    .items {
      list-style: none;
      padding: 0;
      margin: 0;
      overflow: auto;
      flex: 1;
    }
    .items li {
      display: flex;
      justify-content: space-between;
      gap: var(--uui-size-space-3);
      padding: 4px 0;
      border-bottom: 1px solid var(--uui-color-divider);
    }
    .label {
      display: flex;
      align-items: center;
      gap: 6px;
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
    .dot.positive {
      background: var(--uui-color-positive);
    }
    .dot.warning {
      background: var(--uui-color-warning);
    }
    .dot.danger {
      background: var(--uui-color-danger);
    }
    :host {
      container-type: inline-size;
    }
    @container (max-width: 260px) {
      .value {
        flex-direction: column;
        align-items: flex-start;
        gap: var(--uui-size-space-2);
      }
    }
  `;
}

export default MetricTileViewElement;

declare global {
  interface HTMLElementTagNameMap {
    "seo-dashboard-metric-tile-view": MetricTileViewElement;
  }
}
