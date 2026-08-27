import {
  css,
  customElement,
  html,
  nothing,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import SiteAuditCreateContext, {
  ST_SITEAUDIT_CREATE_TOKEN_CONTEXT,
  SiteAuditStartMode,
} from "./SiteAuditCreateContext";
import {
  CreateSiteAuditRequest,
  SiteAuditCreateOptions,
  SiteAuditStartNode,
} from "../dataAccess/SiteAuditApi";
import { SiteAuditCheckSelectionEvent } from "./SiteAuditCheckPicker.element";
import "./SiteAuditCheckPicker.element";

@customElement("seotoolkit-site-audit-create")
export default class SiteAuditCreateWorkspace extends UmbLitElement {
  #context?: SiteAuditCreateContext;

  @state()
  _auditInformationProps: UmbPropertyValueData[] = [];

  @state()
  _selectedChecks: string[] = [];

  @state()
  _config?: SiteAuditCreateOptions;

  @state()
  _mode: SiteAuditStartMode = "node";

  @state()
  _startNode?: SiteAuditStartNode;

  @state()
  _loadingStartNode = false;

  @state()
  _startingUrl = "";

  @state()
  _culture?: string | null;

  @state()
  _canSubmit = false;

  constructor() {
    super();

    this.consumeContext(ST_SITEAUDIT_CREATE_TOKEN_CONTEXT, (instance) => {
      if (!instance) {
        return;
      }
      this.#context = instance;

      this.observe(instance.model, (value) => {
        this._auditInformationProps = [
          {
            alias: "name",
            value: value.name,
          },
          {
            alias: "selectedNode",
            value: value.selectedNodeId ?? "",
          },
          {
            alias: "maxPagesToCrawl",
            value: value.maxPagesToCrawl,
          },
          {
            alias: "delayBetweenRequests",
            value: value.delayBetweenRequests,
          },
        ];

        this._selectedChecks = value.checks ?? [];
        this._startingUrl = value.startingUrl ?? "";
        this._culture = value.culture;
        this.#refreshCanSubmit();
      });

      this.observe(instance.config, (value) => {
        this._config = value;
      });

      this.observe(instance.mode, (value) => {
        this._mode = value;
        this.#refreshCanSubmit();
      });

      this.observe(instance.startNode, (value) => {
        this._startNode = value;
        this.#refreshCanSubmit();
      });

      this.observe(instance.loadingStartNode, (value) => (this._loadingStartNode = value));
    });
  }

  /** The context owns the rule, since it is the only thing that sees all three parts of it. */
  #refreshCanSubmit() {
    this._canSubmit = this.#context?.isComplete ?? false;
  }

  #onAuditInfoUpdate(e: Event) {
    // currentTarget, not target: the language select and the url box live inside the dataset
    // for layout reasons and bubble a change event of their own. Reading the dataset off the
    // event target would then read `value` off one of those instead.
    const value = (e.currentTarget as UmbPropertyDatasetElement)?.value;
    if (!Array.isArray(value)) return;

    const config: Partial<CreateSiteAuditRequest> = {};
    value.forEach((item) => {
      switch (item.alias) {
        case "name":
          config.name = item.value as string;
          break;
        case "selectedNode":
          // Not merged in with the rest: choosing a node also has to fetch what languages it is
          // published in, which the context handles.
          this.#context?.selectNode(SiteAuditCreateWorkspace.toNodeId(item.value));
          break;
        case "maxPagesToCrawl":
          config.maxPagesToCrawl = item.value as number;
          break;
        case "delayBetweenRequests":
          config.delayBetweenRequests = item.value as number;
          break;
      }
    });

    this.#context?.update(config);
  }

  /**
   * The document picker has carried its value in more than one shape across backoffice
   * versions, so the id is taken from whichever it hands over rather than assumed.
   */
  private static toNodeId(value: unknown): string | null {
    if (typeof value === "string") return value.length > 0 ? value : null;

    if (Array.isArray(value) && value.length > 0) {
      const first = value[0];
      if (typeof first === "string") return first;
      if (first && typeof first === "object" && "unique" in first)
        return (first as { unique?: string }).unique ?? null;
    }

    return null;
  }

  /**
   * Kept from reaching the surrounding dataset, which has no property by this name and would
   * only be re-reading values that have not changed.
   */
  #onCultureChange(event: Event) {
    event.stopPropagation();
    this.#context?.update({ culture: (event.target as HTMLSelectElement).value || null });
  }

  #onStartingUrlChange(event: Event) {
    event.stopPropagation();
    this.#context?.update({ startingUrl: (event.target as HTMLInputElement).value || null });
  }

  /** The picker works in aliases throughout, so nothing has to be looked up by display name. */
  #onChecksUpdate(event: SiteAuditCheckSelectionEvent) {
    this.#context?.update({ checks: event.detail.aliases });
  }

  #handleSubmit(start: boolean) {
    this.#context?.save(start);
  }

  override render() {
    return html`
      <umb-body-layout>
        <div class="create-panels">
          <uui-box
            headline="1. Audit information"
            headline-variant="h5"
            class="panel"
          >
            <umb-property-dataset
              .value=${this._auditInformationProps}
              @change=${this.#onAuditInfoUpdate}
            >
              <umb-property
                alias="name"
                label="Name"
                property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
                val
                .validation=${{
                  mandatory: true,
                  mandatoryMessage: "This field is required",
                }}
              >
              </umb-property>

              ${this.#renderStartMode()}
              ${when(this._mode === "node", () => this.#renderNodeStart(), () => this.#renderUrlStart())}
              ${this.#renderPreview()}

              <umb-property
                alias="maxPagesToCrawl"
                label="Max pages to crawl"
                description="The crawl stops once it has visited this many pages. Set to 0 for no maximum."
                property-editor-ui-alias="Umb.PropertyEditorUi.Integer"
                val
              >
              </umb-property>
              ${when(
                this._config?.allowMinimumDelayBetweenRequestSetting,
                () => html`
                  <umb-property
                    alias="delayBetweenRequests"
                    label="Minimum delay between requests"
                    description="Minimum delay to use between requests in seconds. (Optional)"
                    property-editor-ui-alias="Umb.PropertyEditorUi.Integer"
                    val
                  >
                  </umb-property>
                `
              )}
            </umb-property-dataset>
          </uui-box>
          <uui-box headline="2. Checks" headline-variant="h5" class="panel">
            ${when(
              (this._config?.checks?.length ?? 0) === 0,
              // The catalogue is fetched, so this is a moment of loading rather than a site
              // with no checks installed.
              () => html`<uui-loader></uui-loader>`,
              () => html`<st-siteaudit-check-picker
                .checks=${this._config!.checks}
                .selected=${this._selectedChecks}
                @selection-change=${this.#onChecksUpdate}
              ></st-siteaudit-check-picker>`
            )}
          </uui-box>
        </div>
        <umb-footer-layout slot="footer">
        <uui-button
            slot="actions"
            id="cancel"
            label="Cancel"
            look="primary"
            color="danger"
            href="/umbraco/section/SeoToolkit/workspace/seoToolkit-siteAudit/overview"
          >
            Cancel
          </uui-button>
          <uui-button
            slot="actions"
            id="submit"
            label="Submit"
            look="primary"
            color="positive"
            .disabled="${!this._canSubmit}"
            @click="${() => this.#handleSubmit(true)}"
          >
            Create and start
          </uui-button>
        </umb-footer-layout>
      </umb-body-layout>
    `;
  }

  #renderStartMode() {
    return html`
      <umb-property-layout
        label="Start from"
        description="Pick a node to crawl the site as Umbraco routes it, or enter a url for a frontend that routes its own way."
      >
        <div slot="editor" class="mode">
          <uui-button
            label="Umbraco node"
            look=${this._mode === "node" ? "primary" : "outline"}
            @click=${() => this.#context?.setMode("node")}
          >
            Umbraco node
          </uui-button>
          <uui-button
            label="Url"
            look=${this._mode === "url" ? "primary" : "outline"}
            @click=${() => this.#context?.setMode("url")}
          >
            Url
          </uui-button>
        </div>
      </umb-property-layout>
    `;
  }

  #renderNodeStart() {
    return html`
      <umb-property
        alias="selectedNode"
        label="Starting node"
        property-editor-ui-alias="Umb.PropertyEditorUi.DocumentPicker"
        val
        .config=${[
          {
            alias: "max",
            value: 1,
          },
        ]}
        .validation=${{
          mandatory: true,
          mandatoryMessage: "This field is required",
        }}
      >
      </umb-property>

      ${when(this._loadingStartNode, () => html`<uui-loader></uui-loader>`)}
      ${when(this._startNode?.variesByCulture, () => this.#renderCulture())}
    `;
  }

  /**
   * Only offered when the node is genuinely published in more than one language. Left empty
   * until chosen, because guessing would quietly audit a language nobody asked about.
   */
  #renderCulture() {
    const cultures = this._startNode?.cultures ?? [];

    return html`
      <umb-property-layout
        label="Language"
        description="This node is published in several languages. Each one has its own url to crawl."
      >
        <div slot="editor">
          <uui-select
            label="Language"
            placeholder="Choose a language"
            .options=${cultures.map((culture) => ({
              name: `${culture.name} (${culture.isoCode})`,
              value: culture.isoCode,
              selected: culture.isoCode === this._culture,
            }))}
            @change=${(event: Event) => this.#onCultureChange(event)}
          ></uui-select>
        </div>
      </umb-property-layout>
    `;
  }

  #renderUrlStart() {
    return html`
      <umb-property-layout
        label="Starting url"
        description="Crawled exactly as entered. Use this for a decoupled frontend, where no Umbraco node url matches what is actually served."
      >
        <div slot="editor">
          <uui-input
            label="Starting url"
            placeholder="https://www.example.com/"
            .value=${this._startingUrl}
            @change=${(event: Event) => this.#onStartingUrlChange(event)}
          ></uui-input>
        </div>
      </umb-property-layout>
    `;
  }

  /**
   * Shows what will actually be fetched. Worth its space on a decoupled site: the url comes
   * back from the server with the domain's BaseUrl already applied, so this is where it becomes
   * visible that the crawl is aimed at the frontend rather than at Umbraco.
   */
  #renderPreview() {
    const url = this.#context?.previewUrl;
    if (!url) return nothing;

    return html`<p class="preview">Will start crawling <strong>${url}</strong></p>`;
  }

  static override styles = [
    css`
      .create-panels {
        display: flex;
        flex-wrap: wrap;
        flex-direction: column;
        gap: 14px;
        align-items: flex-start;

        > .panel {
          width: 100%
        }
      }

      .mode {
        display: flex;
        gap: 8px;
      }

      .preview {
        margin: 0 0 12px 0;
        color: var(--uui-color-text-alt);
        font-size: 12px;
        overflow-wrap: anywhere;
      }

      .button-bar {
        padding-top: 12px;
      }
    `,
  ];
}
