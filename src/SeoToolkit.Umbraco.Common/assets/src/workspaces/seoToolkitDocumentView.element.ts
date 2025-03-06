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
import { SeoToolkitSettingsRepository } from "../repositories/seoToolkitSettingsRepository";
import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";

@customElement("st-document-view")
export default class SeoToolkitDocumentViewElement extends UmbElementMixin(
  LitElement
) {
  #settingsRepository = new SeoToolkitSettingsRepository(this);
  #actionEventContext?: UmbActionEventContext;

  @state()
  private _documentViews: Array<SeoDocumentViewManifest> = [];

  @state()
  private _documentUnique?: string;

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
      instance.unique.subscribe((value) => {
        this._documentUnique = value?.toString();

        this.#settingsRepository.getSettings(value!).then((resp) => {
          this._seoEnabled = resp.data!.isEnabled;
        });
      });
      instance.isElement.subscribe((value) => {
        this._showViews = !value;
      });
    });

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      this.#actionEventContext = instance;
      instance.addEventListener("document.save", () => this.#save());
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

  #save() {
    this.#settingsRepository.setSettings({
      contentTypeId: this._documentUnique!,
      enabled: this._seoEnabled,
    });
  }

  destroy(): void {
    this.#actionEventContext?.removeEventListener("document.save", () =>
      this.#save()
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
    this._seoEnabled = value;
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
