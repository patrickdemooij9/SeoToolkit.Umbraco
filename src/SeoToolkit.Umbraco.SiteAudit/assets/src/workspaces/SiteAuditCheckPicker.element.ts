import {
  css,
  customElement,
  html,
  nothing,
  property,
  repeat,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { SiteAuditCheckCatalogueEntry } from "../dataAccess/SiteAuditApi";

/** Fired whenever the selection changes, carrying the full set of selected aliases. */
export class SiteAuditCheckSelectionEvent extends CustomEvent<{ aliases: string[] }> {
  static readonly TYPE = "selection-change";

  constructor(aliases: string[]) {
    super(SiteAuditCheckSelectionEvent.TYPE, {
      detail: { aliases },
      bubbles: true,
      composed: true,
    });
  }
}

interface CheckGroup {
  category: string;
  checks: SiteAuditCheckCatalogueEntry[];
}

/**
 * How severely a check reports, most serious first. Mirrors the SeoSeverity enum on the server,
 * which is serialised by name; anything not listed sorts to the bottom rather than to the top,
 * so a severity this build does not know about cannot push itself above the real problems.
 */
const SEVERITY_RANK: Record<string, number> = {
  critical: 4,
  error: 3,
  warning: 2,
  notice: 1,
  passed: 0,
};

const severityRank = (check: SiteAuditCheckCatalogueEntry) =>
  SEVERITY_RANK[check.defaultSeverity?.toLowerCase() ?? ""] ?? 0;

/**
 * Picks the checks an audit should run.
 * <p>
 * Replaces a plain checkbox list of names, which stopped being usable the moment the catalogue
 * grew past a handful: it was one flat column of fifty-odd labels with no descriptions, no
 * grouping and no way to select or clear a whole area. Selection is by alias throughout, so a
 * check contributed by an add-on needs nothing more than a registration to appear here.
 * </p>
 */
@customElement("st-siteaudit-check-picker")
export default class SiteAuditCheckPicker extends UmbLitElement {
  @property({ type: Array })
  checks: SiteAuditCheckCatalogueEntry[] = [];

  @property({ type: Array })
  selected: string[] = [];

  @state() private _search = "";
  @state() private _collapsed = new Set<string>();

  /** Selectable checks only. A locked one can never be part of the selection. */
  get #available() {
    return this.checks.filter((check) => check.isAvailable);
  }

  get #selectedSet() {
    return new Set(this.selected.map((alias) => alias.toLowerCase()));
  }

  #isSelected(check: SiteAuditCheckCatalogueEntry) {
    return this.#selectedSet.has(check.alias.toLowerCase());
  }

  /**
   * Groups by category, keeping the order the server sent so the catalogue decides how areas
   * are ordered rather than this element imposing an alphabet on them. Within a group the
   * checks are ordered by severity, so what breaks a site is read before what merely polishes it.
   */
  #groups(): CheckGroup[] {
    const term = this._search.trim().toLowerCase();

    const matches = (check: SiteAuditCheckCatalogueEntry) =>
      term.length === 0 ||
      check.name.toLowerCase().includes(term) ||
      check.alias.toLowerCase().includes(term) ||
      (check.description ?? "").toLowerCase().includes(term);

    const groups: CheckGroup[] = [];
    const byCategory = new Map<string, CheckGroup>();

    for (const check of this.checks) {
      if (!matches(check)) continue;

      let group = byCategory.get(check.category);
      if (!group) {
        group = { category: check.category, checks: [] };
        byCategory.set(check.category, group);
        groups.push(group);
      }

      group.checks.push(check);
    }

    // Sorted by severity alone, and Array.sort is stable, so checks of equal severity keep the
    // catalogue's own order - which keeps related ones such as the title checks together.
    for (const group of groups)
      group.checks.sort((a, b) => severityRank(b) - severityRank(a));

    return groups;
  }

  #emit(aliases: string[]) {
    this.selected = aliases;
    this.dispatchEvent(new SiteAuditCheckSelectionEvent(aliases));
  }

  #toggle(check: SiteAuditCheckCatalogueEntry, checked: boolean) {
    if (!check.isAvailable) return;

    const next = this.selected.filter(
      (alias) => alias.toLowerCase() !== check.alias.toLowerCase()
    );

    if (checked) next.push(check.alias);

    this.#emit(next);
  }

  /** Applies to what is currently visible, so it follows the search rather than ignoring it. */
  #toggleGroup(group: CheckGroup, checked: boolean) {
    const aliases = group.checks.filter((it) => it.isAvailable).map((it) => it.alias);
    const lowered = new Set(aliases.map((alias) => alias.toLowerCase()));

    const next = this.selected.filter((alias) => !lowered.has(alias.toLowerCase()));

    this.#emit(checked ? [...next, ...aliases] : next);
  }

  #selectAll() {
    this.#emit(this.#available.map((check) => check.alias));
  }

  #selectNone() {
    this.#emit([]);
  }

  #toggleCollapsed(category: string) {
    const next = new Set(this._collapsed);
    if (!next.delete(category)) next.add(category);

    this._collapsed = next;
  }

  render() {
    const groups = this.#groups();
    const available = this.#available;

    return html`
      <div class="toolbar">
        <uui-input
          type="search"
          label="Search checks"
          placeholder="Search checks"
          .value=${this._search}
          @input=${(event: InputEvent) =>
            (this._search = (event.target as HTMLInputElement).value)}
        ></uui-input>
        <div class="toolbar-actions">
          <uui-button label="Select all" look="outline" compact @click=${this.#selectAll}>
            Select all
          </uui-button>
          <uui-button label="Select none" look="outline" compact @click=${this.#selectNone}>
            Select none
          </uui-button>
        </div>
      </div>

      <p class="summary ${this.selected.length === 0 ? "danger" : "muted"}">
        ${this.selected.length} of ${available.length} checks selected${this.selected.length === 0
          ? " - an audit needs at least one"
          : ""}
      </p>

      ${when(
        groups.length === 0,
        () => html`<p class="muted">No checks match "${this._search}".</p>`,
        () => html`<div class="groups">
          ${repeat(groups, (group) => group.category, (group) => this.#renderGroup(group))}
        </div>`
      )}
    `;
  }

  #renderGroup(group: CheckGroup) {
    const selectable = group.checks.filter((check) => check.isAvailable);
    const selectedCount = selectable.filter((check) => this.#isSelected(check)).length;
    const allSelected = selectable.length > 0 && selectedCount === selectable.length;

    // Collapsing follows the search: hiding a group the user just searched into would be
    // actively unhelpful.
    const isCollapsed = this._collapsed.has(group.category) && this._search.trim().length === 0;

    return html`
      <div class="group">
        <div class="group-header">
          <uui-checkbox
            label=${group.category}
            ?checked=${allSelected}
            ?disabled=${selectable.length === 0}
            @change=${(event: Event) =>
              this.#toggleGroup(group, (event.target as HTMLInputElement).checked)}
          ></uui-checkbox>
          <button
            class="group-toggle"
            type="button"
            aria-expanded=${isCollapsed ? "false" : "true"}
            @click=${() => this.#toggleCollapsed(group.category)}
          >
            <strong>${group.category}</strong>
            <span class="muted">${selectedCount} of ${group.checks.length}</span>
            <uui-icon
              name=${isCollapsed ? "icon-navigation-right" : "icon-navigation-down"}
            ></uui-icon>
          </button>
        </div>

        ${when(
          !isCollapsed,
          () => html`<div class="group-checks">
            ${repeat(group.checks, (check) => check.alias, (check) => this.#renderCheck(check))}
          </div>`
        )}
      </div>
    `;
  }

  #renderCheck(check: SiteAuditCheckCatalogueEntry) {
    return html`
      <div class="check ${check.isAvailable ? "" : "locked"}">
        <uui-checkbox
          ?checked=${this.#isSelected(check)}
          ?disabled=${!check.isAvailable}
          @change=${(event: Event) =>
            this.#toggle(check, (event.target as HTMLInputElement).checked)}
        ></uui-checkbox>
        <!-- Deliberately not a <label> around the checkbox: uui-checkbox renders its own, and
             nesting the two makes one click register as two and cancel itself out. -->
        <span
          class="check-body"
          @click=${() => this.#toggle(check, !this.#isSelected(check))}
        >
          <span class="check-name">
            ${check.name}
            <span class="severity severity-${check.defaultSeverity.toLowerCase()}">
              ${check.defaultSeverity}
            </span>
            ${when(
              !check.isAvailable,
              () => html`<span class="lock" title=${`Requires ${check.requiresFeature}`}>
                <uui-icon name="icon-lock"></uui-icon>
                ${check.requiresFeature}
              </span>`
            )}
          </span>
          ${check.description
            ? html`<span class="muted">${check.description}</span>`
            : nothing}
        </span>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
    }

    .toolbar {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 8px;
    }

    .toolbar uui-input {
      flex: 1 1 200px;
    }

    .toolbar-actions {
      display: flex;
      gap: 4px;
    }

    .summary {
      margin: 0 0 12px 0;
      font-size: 12px;
    }

    .muted {
      color: var(--uui-color-text-alt);
      font-size: 12px;
    }

    .danger {
      color: var(--uui-color-danger);
    }

    .groups {
      display: flex;
      flex-direction: column;
      gap: 4px;
      /* Bounded so the panel next to it stays reachable however many checks are installed. */
      max-height: 60vh;
      overflow-y: auto;
    }

    .group-header {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 4px;
      border-bottom: 1px solid var(--uui-color-divider);
      background-color: var(--uui-color-surface);
      position: sticky;
      top: 0;
      z-index: 1;
    }

    .group-toggle {
      display: flex;
      flex: 1;
      align-items: center;
      gap: 8px;
      background: none;
      border: none;
      padding: 0;
      font: inherit;
      cursor: pointer;
      text-align: left;
      color: inherit;
    }

    .group-checks {
      display: flex;
      flex-direction: column;
      padding: 4px 0 8px 0;
    }

    .check {
      display: flex;
      align-items: flex-start;
      gap: 8px;
      padding: 4px 4px 4px 12px;
      cursor: pointer;
    }

    .check:hover {
      background-color: var(--uui-color-surface-alt);
    }

    .check.locked {
      cursor: default;
      opacity: 0.7;
    }

    .check-body {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .check-name {
      display: flex;
      align-items: center;
      gap: 6px;
      flex-wrap: wrap;
    }

    .severity {
      padding: 0 6px;
      border-radius: 3px;
      font-size: 11px;
      background-color: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .severity-critical,
    .severity-error {
      background-color: var(--uui-color-danger);
      color: var(--uui-color-surface);
    }

    .severity-warning {
      background-color: var(--uui-color-warning);
      color: var(--uui-color-text);
    }

    .lock {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      font-size: 11px;
      color: var(--uui-color-text-alt);
    }
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    "st-siteaudit-check-picker": SiteAuditCheckPicker;
  }
}
