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
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement, nothing } from "lit";
import {
  UmbRoute,
  UmbRouterSlotChangeEvent,
  UmbRouterSlotInitEvent,
} from "@umbraco-cms/backoffice/router";
import { SeoContentViewManifest } from "../manifests/seoContentViewManifest";

@customElement("st-content-view")
export default class SeoToolkitDocumentViewElement extends UmbElementMixin(
  LitElement
) {
  @state()
  private _contentViews: Array<SeoContentViewManifest> = [];

  @state()
  private _routes?: UmbRoute[];

  @state()
  private _routerPath?: string;

  @state()
  private _activePath?: string;

  constructor() {
    super();

    new UmbExtensionsManifestInitializer(
      this,
      umbExtensionsRegistry,
      "seoToolkitContentView",
      null,
      (documentViews) => {
        this._contentViews = documentViews.map(
          (view) => view.manifest as unknown as SeoContentViewManifest
        );
        this._createRoutes();
      }
    );
  }

  private _createRoutes() {
    let newRoutes: UmbRoute[] = [];

    if (this._contentViews.length > 0) {
      newRoutes = this._contentViews.map((manifest) => {
        return {
          path: `view/${manifest.meta.pathname}`,
          component: () => createExtensionElement(manifest),
          setup: (component) => {
            if (component) {
              (component as any).manifest = manifest;
            }
          },
        } as UmbRoute;
      });

      newRoutes.push({ ...newRoutes[0], path: "" });
    }

    this._routes = newRoutes;
  }

  render() {
    return html`
      <umb-body-layout header-fit-height
        >${this.#renderViews()} ${this.#renderRoutes()}</umb-body-layout
      >
    `;
  }

  #renderViews() {
    return html`
      <uui-tab-group class="navigation" slot="header">
        ${repeat(
          this._contentViews,
          (view) => view.alias,
          (view, index) =>
            html`
              <uui-tab
                href="${this._routerPath}/view/${view.meta.pathname}"
                .label="${view.meta.label
                  ? this.localize.string(view.meta.label)
                  : view.name}"
                ?active=${"view/" + view.meta.pathname === this._activePath ||
                (index === 0 && this._activePath === "")}
              >
                ${view.meta.label
                  ? this.localize.string(view.meta.label)
                  : view.name}
              </uui-tab>
            `
        )}
      </uui-tab-group>
    `;
  }

  #renderRoutes() {
    if (!this._routes || this._routes.length === 0) return nothing;
    return html`
      <umb-router-slot
          id="router-slot"
          .routes=${this._routes}
          @init=${(event: UmbRouterSlotInitEvent) => {
            this._routerPath = event.target.absoluteRouterPath;
          }}
          @change=${(event: UmbRouterSlotChangeEvent) => {
            this._activePath = event.target.localActiveViewPath;
          }}
        ></umb-router-slot>
    `;
  }

  static override styles = [
    css`
      :host {
        display: block;
        width: 100%;
        height: 100%;
      }

      #router-slot {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
    `,
  ];
}
