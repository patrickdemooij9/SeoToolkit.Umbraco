import {
  css,
  customElement,
  html,
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
} from "./SiteAuditCreateContext";
import { CreateAuditPostModel, SiteAuditCreateConfigViewModel } from "../api";

const UMBRACO_SOURCE = "umbraco";

@customElement("seotoolkit-site-audit-create")
export default class SiteAuditCreateWorkspace extends UmbLitElement {
  #context?: SiteAuditCreateContext;

  @state()
  _auditInformationProps: UmbPropertyValueData[] = [];

  @state()
  _auditChecksProps: UmbPropertyValueData[] = [];

  @state()
  _config?: SiteAuditCreateConfigViewModel;

  @state()
  _canSubmit = false;

  @state()
  _selectedSource: string = UMBRACO_SOURCE;

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
            value: value.selectedNodeId,
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

        this._canSubmit =
          !!value.name &&
          (
            (!!value.selectedNodeId && value.selectedNodeId !== "") ||
            !!value.startingUrl
          );

        const checkNames =
          this._config?.checks
            ?.filter((item) => value.checks?.includes(item.id))
            .map((item) => item.name) ?? [];
        this._auditChecksProps = [
          {
            alias: "checks",
            value: checkNames,
          },
        ];
      });

      this.observe(instance.config, (value) => {
        this._config = value;
      });
    });
  }

  #onSourceChange(e: Event) {
    const select = e.target as HTMLSelectElement;
    this._selectedSource = select.value;

    if (this._selectedSource === UMBRACO_SOURCE) {
      this.#context?.update({ startingUrl: null, selectedNodeId: "" });
    } else {
      this.#context?.update({ startingUrl: this._selectedSource, selectedNodeId: null });
    }
  }

  #onAuditInfoUpdate(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    const config: Partial<CreateAuditPostModel> = {};
    value.forEach((item) => {
      switch (item.alias) {
        case "name":
          config.name = item.value as string;
          break;
        case "selectedNode":
          config.selectedNodeId = item.value as string;
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

  #onChecksUpdate(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    const config: Partial<CreateAuditPostModel> = {};
    value.forEach((item) => {
      switch (item.alias) {
        case "checks":
          const checkNames = item.value as string[];
          const selectedCheckIds = this._config?.checks
            ?.filter((item) => checkNames.includes(item.name!))
            .map((item) => item.id);
          config.checks = selectedCheckIds;
          break;
      }
    });

    this.#context?.update(config);
  }

  #handleSubmit(start: boolean) {
    this.#context?.save(start);
  }

  override render() {
    const hasDomains = (this._config?.domains?.length ?? 0) > 0;
    const isUmbracoSource = this._selectedSource === UMBRACO_SOURCE;

    return html`
      <umb-body-layout>
        <div class="create-panels">
          <uui-box
            headline="1. Audit information"
            headline-variant="h5"
            class="panel"
          >
            ${when(
              hasDomains,
              () => html`
                <div class="source-picker">
                  <label class="source-picker__label">Crawl source</label>
                  <p class="source-picker__description">Choose between using Umbraco pages or a custom domain for the audit</p>
                  <uui-select
                    .options=${[
                      { name: "Standard (Umbraco)", value: UMBRACO_SOURCE },
                      ...(this._config?.domains?.map((d) => ({
                        name: d,
                        value: d,
                      })) ?? []),
                    ]}
                    .value=${this._selectedSource}
                    @change=${this.#onSourceChange}
                  ></uui-select>
                </div>
              `
            )}
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
              ${when(
                isUmbracoSource,
                () => html`
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
                `
              )}
              <umb-property
                alias="maxPagesToCrawl"
                label="Max pages to crawl"
                description="The max pages to crawl. Leave empty for no maximum"
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
            <umb-property-dataset
              .value=${this._auditChecksProps}
              @change=${this.#onChecksUpdate}
            >
              <umb-property
                alias="checks"
                label="Audit checks"
                description="The checks that you would like to do in this audit"
                property-editor-ui-alias="Umb.PropertyEditorUi.CheckBoxList"
                val
                .config=${[
                  {
                    alias: "items",
                    value: this._config?.checks?.map((item) => item.name) ?? [],
                  },
                ]}
              >
              </umb-property>
            </umb-property-dataset>
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

  static override styles = [
    css`
      .create-panels {
        display: flex;
        gap: 14px;

        > .panel {
          width: 50%;
        }
      }

      .button-bar {
        padding-top: 12px;
      }

      .source-picker {
        margin-bottom: 12px;
      }

      .source-picker__label {
        display: block;
        font-weight: bold;
        margin-bottom: 4px;
      }

      .source-picker__description {
        margin: 0 0 6px;
        color: var(--uui-color-text-alt, #6b7280);
        font-size: 0.875em;
      }
    `,
  ];
}
