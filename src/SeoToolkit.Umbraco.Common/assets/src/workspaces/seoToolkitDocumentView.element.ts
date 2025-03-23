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

@customElement("st-document-view")
export default class SeoToolkitDocumentViewElement extends UmbElementMixin(
  LitElement
) {
  #context?: SeoToolkitDocumentContext;

  @state()
  private _documentViews: Array<SeoDocumentViewManifest> = [];

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
      instance.isElement.subscribe((value) => {
        this._showViews = !value;
      });
    });

    this.consumeContext(ST_METAFIELDS_SETTINGSDOCUMENT_TOKEN_CONTEXT, (instance) => {
      this.#context = instance;

      instance.model.subscribe((value) => {
        this._seoEnabled = value.enabled;
      })
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
      newRoutes = this._documentViews.map((manifest) => {
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

  private setSeoSettings(value: boolean) {
    this.#context?.setSeoSettings(value);
  }

  render() {
    return html`
      <umb-body-layout header-fit-height
        >${this.#renderViews()} ${this.#renderRoutes()}</umb-body-layout
      >
    `;
  }

  #renderViews() {
    if (!this._showViews) return nothing;
    return html`
      <div class="top" slot="header">
        <div>
          ${when(
            this._seoEnabled,
            () => html`
              <uui-tab-group class="navigation">
                ${repeat(
                  this._documentViews,
                  (view) => view.alias,
                  (view, index) =>
                    html`
                      <uui-tab
                        href="${this._routerPath}/view/${view.meta.pathname}"
                        .label="${view.meta.label
                          ? this.localize.string(view.meta.label)
                          : view.name}"
                        ?active=${"view/" + view.meta.pathname ===
                          this._activePath ||
                        (index === 0 && this._activePath === "")}
                      >
                        ${view.meta.label
                          ? this.localize.string(view.meta.label)
                          : view.name}
                      </uui-tab>
                    `
                )}
              </uui-tab-group>
            `
          )}
        </div>
        <div>
          ${when(
            this._seoEnabled,
            () => html`
              <uui-button
                look="outline"
                @click=${() => this.setSeoSettings(false)}
              >
                Disable SEO settings
              </uui-button>
            `,
            () => html`
              <uui-button
                look="outline"
                @click=${() => this.setSeoSettings(true)}
              >
                Enable SEO settings
              </uui-button>
            `
          )}
        </div>
      </div>
    `;
  }

  #renderRoutes() {
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

      .top {
        width: 100%;
        height: 48px;

        display: flex;
        justify-content: space-between;
        align-items: center;
      }

      #router-slot {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
    `,
  ];
}
