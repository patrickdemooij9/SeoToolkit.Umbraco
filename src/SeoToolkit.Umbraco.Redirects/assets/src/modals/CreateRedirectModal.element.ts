import {
  css,
  customElement,
  html,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import {
  UMB_MODAL_MANAGER_CONTEXT,
  UmbModalBaseElement,
} from "@umbraco-cms/backoffice/modal";
import { RedirectModalData } from "../models/RedirectModalData";
import { Redirect } from "../models/Redirect";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import {
  UUIInputEvent,
  UUISelectEvent,
} from "@umbraco-cms/backoffice/external/uui";
import { RedirectLinkType } from "../types/RedirectLinkType";
import { RedirectSelectLinkData } from "../models/RedirectSelectLinkData";
import { UmbDocumentUrlRepository } from "@umbraco-cms/backoffice/document";
import { UmbMediaUrlRepository } from "@umbraco-cms/backoffice/media";
import RedirectRepository from "../dataLayer/RedirectRepository";
import { DomainViewModel } from "../api";

@customElement("st-create-redirect-modal")
export default class CreateRedirectModal extends UmbModalBaseElement<
  RedirectModalData,
  RedirectModalData
> {
  #umbMediaUrlRepository = new UmbMediaUrlRepository(this);
  #documentUrlRepository = new UmbDocumentUrlRepository(this);
  #redirectRepository = new RedirectRepository(this);
  #options: Array<Option> = [];
  #oldUrl?: string;

  @state()
  redirect?: UmbObjectState<Redirect>;

  @state()
  newUrlName?: string;

  @state()
  domains: Array<DomainViewModel> = [];

  @state()
  _content: UmbPropertyValueData[] = [];

  @state()
  showValidationMessage: boolean = false;

  override async connectedCallback() {
    super.connectedCallback();

    this.domains = (await this.#redirectRepository.getDomains()).data;
    this.domains.splice(0, 0, { id: 0, name: "All Sites" });
    this.domains.push({ id: -1, name: "Custom Domain" });

    this.redirect = new UmbObjectState(this.data!.redirect);
    this.observe(this.redirect.asObservable(), (value) => {
      const domainName =
        this.domains.find((d) => d.id === value.domain)?.name ??
        this.domains[0].name;
      this._content = [
        {
          alias: "domain",
          value: [domainName],
        },
        {
          alias: "customDomain",
          value: value.customDomain,
        },
        {
          alias: "isEnabled",
          value: value.isEnabled,
        },
        {
          alias: "toUrl",
          value: value.newUrl,
        },
        {
          alias: "redirectCode",
          value:
            value.redirectCode === 301 ? "Permanent (301)" : "Temporary (302)",
        },
      ];
      this.#options = [
        {
          name: "URL",
          value: "url",
          selected: !value.isRegex,
        },
        {
          name: "Regex",
          value: "regex",
          selected: value.isRegex,
        },
      ];
      this.#oldUrl = value.oldUrl ?? "";

      if (value.newUrl || value.newNodeId) {
        if (value.newUrl) {
          this.newUrlName = value.newUrl;
        } else {
          if (value.newCultureIso) {
            this.#documentUrlRepository
              .requestItems([value.newNodeId!])
              .then((resp) => {
                this.newUrlName = resp.data![0].urls.find(
                  (u) => u.culture === value.newCultureIso,
                )?.url;
              });
          } else {
            this.#umbMediaUrlRepository
              .requestItems([value.newNodeId!])
              .then((resp) => {
                this.newUrlName = resp.data![0].url;
              });
          }
        }
      }
      this.showValidationMessage = false;
    });
  }

  #onPropertyDataChange(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    const newValue = {} as any;
    value.forEach((item) => {
      if (item.alias === "domain") {
        const domainId = this.domains.find(
          (d) => d.name === (item.value as string[])[0],
        )?.id;
        newValue["domain"] = domainId;
      } else if (item.alias === "redirectCode") {
        newValue["redirectCode"] = item.value === "Permanent (301)" ? 301 : 302;
      } else {
        if (Array.isArray(item.value)) {
          newValue[item.alias] = item.value[0];
        } else {
          newValue[item.alias] = item.value;
        }
      }
    });
    this.redirect?.update(newValue);
  }

  #onRegexOptionChange(e: UUISelectEvent) {
    this.redirect!.update({
      isRegex: e.target.value === "regex",
    });
  }

  #onOldUrlChange(e: UUIInputEvent) {
    this.redirect!.update({
      oldUrl: e.target.value.toString(),
    });
  }

  #onSetLinkClick() {
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, async (instance) => {
      if (!instance) {
        return;
      }

      let linkType = RedirectLinkType.Url;
      if (this.redirect?.value.newNodeId) {
        linkType = this.redirect.value.newCultureIso
          ? RedirectLinkType.Content
          : RedirectLinkType.Media;
      }
      const modal = instance.open(this, "seoToolkit.modal.redirect.link", {
        modal: { type: "sidebar", size: "medium" },
        data: {
          linkType: linkType,
          culture: this.redirect?.value.newCultureIso,
          url: this.redirect?.value.newUrl,
          contentKey:
            linkType === RedirectLinkType.Content
              ? this.redirect?.value.newNodeId
              : undefined,
          mediaKey:
            linkType === RedirectLinkType.Media
              ? this.redirect?.value.newNodeId
              : undefined,
        },
      });
      await modal.onSubmit();
      const value = modal.getValue() as RedirectSelectLinkData;

      let newNodeId = undefined;
      if (value.linkType !== RedirectLinkType.Url) {
        newNodeId =
          value.linkType === RedirectLinkType.Content
            ? value.contentKey!
            : value.mediaKey!;
      }
      this.redirect?.update({
        newUrl: value.linkType === RedirectLinkType.Url ? value.url : undefined,
        newCultureIso: value.culture,
        newNodeId: newNodeId,
      });
    });
  }

  #handleSubmit() {
    if (!this.redirect?.value.oldUrl || !this.newUrlName) {
      this.showValidationMessage = true;
      this.requestUpdate();
      return;
    }

    this.value = {
      redirect: {
        ...this.redirect!.value,
        domain:
          this.redirect?.value.domain && this.redirect.value.domain <= 0
            ? null
            : this.redirect?.value.domain,
      },
    };
    this.modalContext?.submit();
  }

  render() {
    return html`
      <umb-body-layout headline="Create redirect">
        <uui-box>
          <umb-property-dataset
            .value=${this._content!}
            @change=${this.#onPropertyDataChange}
          >
            <umb-property
              alias="domain"
              label="Domain"
              description="Choose the domain set within Umbraco or use a custom domain"
              property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
              val
              .validation=${{
                mandatory: true,
                mandatoryMessage: "This field is required",
              }}
              .config=${[
                {
                  alias: "items",
                  value: this.domains.map((item) => item.name),
                },
              ]}
            >
            </umb-property>
            ${when(
              this.redirect?.value.domain === -1,
              () => html`
                <umb-property
                  alias="customDomain"
                  label="Custom domain"
                  description="Choose a custom domain if it isn't present in Umbraco"
                  property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
                  val
                >
                </umb-property>
              `,
            )}
            <umb-property
              alias="isEnabled"
              label="Enabled"
              description="Uncheck this box to stop the redirect from functioning"
              property-editor-ui-alias="Umb.PropertyEditorUi.Toggle"
              val
            >
            </umb-property>
            <umb-property-layout
              label="From url"
              description="Relative url of where the redirect starts"
            >
              <div slot="editor" class="from-url">
                <uui-input
                  .value=${this.#oldUrl ?? ""}
                  @change=${this.#onOldUrlChange}
                >
                </uui-input>
                <uui-select
                  .options=${this.#options}
                  @change=${this.#onRegexOptionChange}
                >
                </uui-select>
                ${when(
                  this.showValidationMessage && !this.#oldUrl,
                  () => html`<div class="error">This field is required!</div>`,
                )}
              </div>
            </umb-property-layout>
            <umb-property-layout
              label="New Url"
              description="The url where the redirect is going to"
            >
              <div slot="editor">
                ${when(
                  !this.redirect?.value.newUrl &&
                    !this.redirect?.value.newNodeId,
                  () => html`
                    <uui-button
                      look="placeholder"
                      @click=${this.#onSetLinkClick}
                    >
                      <span>Set link</span>
                    </uui-button>
                  `,
                  () => html`
                    <uui-button look="outline" @click=${this.#onSetLinkClick}>
                      <span>${this.newUrlName}</span>
                    </uui-button>
                  `,
                )}
                ${when(
                  this.showValidationMessage && !this.newUrlName,
                  () => html`<div class="error">This field is required!</div>`,
                )}
              </div>
            </umb-property-layout>
            <umb-property
              alias="redirectCode"
              label="Status code"
              description="Status code of the redirect"
              property-editor-ui-alias="Umb.PropertyEditorUi.RadioButtonList"
              val
              .config=${[
                {
                  alias: "items",
                  value: ["Permanent (301)", "Temporary (302)"],
                },
              ]}
            >
            </umb-property>
          </umb-property-dataset>
        </uui-box>

        <umb-workspace-footer slot="footer" data-mark="workspace:footer">
          <slot name="footer-info"></slot>
          <slot
            name="actions"
            slot="actions"
            data-mark="workspace:footer-actions"
          >
            <uui-button
              slot="actions"
              id="save"
              label="Submit"
              look="primary"
              color="positive"
              @click="${this.#handleSubmit}"
              >Submit</uui-button
            >
          </slot>
        </umb-workspace-footer>
      </umb-body-layout>
    `;
  }

  static override styles = [
    css`
      .error {
        color: red;
      }

      .from-url {
        display: flex;
        gap: 8px;
      }
    `,
  ];
}
