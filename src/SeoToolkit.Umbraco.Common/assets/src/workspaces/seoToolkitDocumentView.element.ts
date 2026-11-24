import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  createExtensionElement,
  UmbExtensionsManifestInitializer,
} from "@umbraco-cms/backoffice/extension-api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import {
  customElement,
  repeat,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement, nothing } from "lit";
import {
  UmbRoute,
  UmbRouterSlotChangeEvent,
  UmbRouterSlotInitEvent,
} from "@umbraco-cms/backoffice/router";
import { SeoDocumentViewManifest } from "../manifests/seoDocumentViewManifest";
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document-type";
import SeoToolkitDocumentContext, {
  ST_METAFIELDS_SETTINGSDOCUMENT_TOKEN_CONTEXT,
} from "./SeoToolkitDocumentContext";
import { SeoDocumentNavItem } from "./seoDocumentNavigation";

@customElement("st-document-view")
export default class SeoToolkitDocumentViewElement extends UmbElementMixin(
  LitElement
) {
  #context?: SeoToolkitDocumentContext;

  @state()
  private _documentViews: Array<SeoDocumentViewManifest> = [];

  @state()
  private _navItems: Record<string, SeoDocumentNavItem[]> = {};

  @state()
  private _routes?: UmbRoute[];

  @state()
  private _routerPath?: string;

  @state()
  private _activePath?: string;

  @state()
  private _showViews: boolean = true;

  @state()
  private _seoEnabled: boolean = false;

  constructor() {
    super();

    this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
      this.observe(instance?.isElement, (value) => {
        this._showViews = !value;
      });
    });

    this.consumeContext(ST_METAFIELDS_SETTINGSDOCUMENT_TOKEN_CONTEXT, (instance) => {
      this.#context = instance;

      this.observe(instance?.model, (value) => {
        if (!value) return;

        this._seoEnabled = value.enabled;
      });

      this.observe(instance?.navItems, (value) => {
        this._navItems = value ?? {};
      });
    });

    new UmbExtensionsManifestInitializer(
      this,
      umbExtensionsRegistry,
      "seoToolkitDocumentView",
      null,
      (documentViews) => {
        this._documentViews = documentViews.map(
          (view) => view.manifest as unknown as SeoDocumentViewManifest
        );
        this._createRoutes();
      }
    );
  }

  private _createRoutes() {
    let newRoutes: UmbRoute[] = [];

    if (this._documentViews.length > 0) {
      newRoutes = this._documentViews.flatMap((manifest) => {
        const route = {
          component: () => createExtensionElement(manifest),
          setup: (component: HTMLElement) => {
            if (component) {
              (component as any).manifest = manifest;
            }
          },
        };
        // The item route comes first, as routes match on prefix.
        return [
          { ...route, path: `view/${manifest.meta.pathname}/:item` },
          { ...route, path: `view/${manifest.meta.pathname}` },
        ] as UmbRoute[];
      });

      newRoutes.push({ ...newRoutes[1], path: "" });
    }

    this._routes = newRoutes;
  }

  // Splits the active router path into the view pathname and the selected nav item.
  private _getActiveSelection(): { pathname?: string; item?: string } {
    const [, pathname, item] = (this._activePath ?? "").split("/");
    return {
      pathname: pathname ?? this._documentViews[0]?.meta.pathname,
      item,
    };
  }

  private _onRouteChange(event: UmbRouterSlotChangeEvent) {
    this._activePath = event.target.localActiveViewPath;

    const { pathname, item } = this._getActiveSelection();
    if (pathname) {
      this.#context?.setActiveNavItem(pathname, item);
    }
  }

  private setSeoSettings(value: boolean) {
    this.#context?.setSeoSettings(value);
  }

  render() {
    return html`
      <umb-body-layout header-fit-height
        >${this.#renderHeader()} ${this.#renderContent()}</umb-body-layout
      >
    `;
  }

  #renderHeader() {
    if (!this._showViews) return nothing;
    return html`
      <div class="top" slot="header">
        ${when(
          this._seoEnabled,
          () => html`
            <uui-button
              look="outline"
              label="Disable SEO settings"
              @click=${() => this.setSeoSettings(false)}
            >
              Disable SEO settings
            </uui-button>
          `,
          () => html`
            <uui-button
              look="outline"
              label="Enable SEO settings"
              @click=${() => this.setSeoSettings(true)}
            >
              Enable SEO settings
            </uui-button>
          `
        )}
      </div>
    `;
  }

  #renderContent() {
    if (!this._routes || this._routes.length === 0) return nothing;
    if (!this._showViews) {
      return html`
        <div class="empty-state">This is not applicable for element types</div>
      `;
    }
    if (!this._seoEnabled) {
      return html` <div class="empty-state">Enable SEO to get started</div> `;
    }
    return html`
      <div class="layout">
        <nav class="navigation" aria-label="SEO settings">
          ${repeat(
            this._documentViews,
            (view) => view.alias,
            (view) => this.#renderViewNav(view)
          )}
        </nav>
        <umb-router-slot
          id="router-slot"
          .routes=${this._routes}
          @init=${(event: UmbRouterSlotInitEvent) => {
            this._routerPath = event.target.absoluteRouterPath;
          }}
          @change=${this._onRouteChange}
        ></umb-router-slot>
      </div>
    `;
  }

  #renderViewNav(view: SeoDocumentViewManifest) {
    const label = view.meta.label
      ? this.localize.string(view.meta.label)
      : view.name;
    const items = this._navItems[view.meta.pathname] ?? [];
    const active = this._getActiveSelection();
    const isActiveView = active.pathname === view.meta.pathname;
    const href = `${this._routerPath}/view/${view.meta.pathname}`;

    if (items.length === 0) {
      return this.#renderNavItem(
        href,
        { key: view.meta.pathname, label, icon: view.meta.icon },
        isActiveView
      );
    }

    return html`
      <div class="section">
        <h4 class="section-title">${label}</h4>
        ${repeat(
          items,
          (item) => item.key,
          (item, index) =>
            this.#renderNavItem(
              `${href}/${item.key}`,
              item,
              isActiveView &&
                (active.item === item.key || (!active.item && index === 0))
            )
        )}
      </div>
    `;
  }

  #renderNavItem(href: string, item: SeoDocumentNavItem, active: boolean) {
    return html`
      <a
        class="nav-item ${active ? "active" : ""}"
        href=${href}
        aria-current=${active ? "page" : "false"}
      >
        <umb-icon name=${item.icon ?? "icon-settings"}></umb-icon>
        <span class="nav-label">${item.label}</span>
        ${item.badge
          ? html`<span class="nav-badge">${item.badge}</span>`
          : nothing}
      </a>
    `;
  }

  static override styles = [
    css`
      :host {
        display: block;
        width: 100%;
        height: 100%;
      }

      .top {
        width: 100%;
        height: 48px;

        display: flex;
        justify-content: flex-end;
        align-items: center;
      }

      .layout {
        display: grid;
        grid-template-columns: 240px minmax(0, 1fr);
        gap: var(--uui-size-space-5);
        align-items: start;
      }

      .navigation {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-4);
        position: sticky;
        top: 0;
      }

      .section {
        display: flex;
        flex-direction: column;
        gap: 2px;
      }

      .section-title {
        margin: 0;
        padding: 0 var(--uui-size-space-4) var(--uui-size-space-2);
        font-size: var(--uui-type-small-size);
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: var(--uui-color-text-alt);
      }

      .nav-item {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-3);
        padding: var(--uui-size-space-3) var(--uui-size-space-4);
        border-radius: var(--uui-border-radius);
        color: inherit;
        text-decoration: none;
      }

      .nav-item:hover {
        background: var(--uui-color-surface-emphasis);
      }

      .nav-item:focus-visible {
        outline: 2px solid var(--uui-color-focus);
      }

      .nav-item.active {
        background: var(--uui-color-current);
        color: var(--uui-color-current-contrast);
        font-weight: 700;
      }

      .nav-label {
        flex: 1;
      }

      .nav-badge {
        font-size: var(--uui-type-small-size);
        color: var(--uui-color-text-alt);
      }

      #router-slot {
        display: flex;
        flex-direction: column;
        height: 100%;
      }

      @media (max-width: 800px) {
        .layout {
          grid-template-columns: 1fr;
        }

        .navigation {
          position: static;
        }
      }
    `,
  ];
}
