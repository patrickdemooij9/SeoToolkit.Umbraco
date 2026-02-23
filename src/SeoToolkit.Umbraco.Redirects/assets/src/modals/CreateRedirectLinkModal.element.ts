import {
  UmbModalBaseElement,
  umbOpenModal,
} from "@umbraco-cms/backoffice/modal";
import { RedirectSelectLinkData } from "../models/RedirectSelectLinkData";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import {
  css,
  customElement,
  html,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import { UmbPropertyTypeAppearanceModel } from "@umbraco-cms/backoffice/content-type";
import { RedirectLinkType } from "../types/RedirectLinkType";
import {
  UMB_APP_LANGUAGE_CONTEXT,
  UmbAppLanguageContext,
  UmbLanguageCollectionRepository,
  UmbLanguageDetailModel,
} from "@umbraco-cms/backoffice/language";
import { UmbId } from "@umbraco-cms/backoffice/id";
import {
  UMB_DOCUMENT_PICKER_MODAL,
  UmbDocumentItemModel,
  UmbDocumentItemRepository,
  UmbDocumentTreeItemModel,
} from "@umbraco-cms/backoffice/document";

@customElement("st-create-redirect-link-modal")
export default class CreateRedirectLinkModal extends UmbModalBaseElement<
  RedirectSelectLinkData,
  RedirectSelectLinkData
> {
  #languageContext?: UmbAppLanguageContext;
  #documentRepository = new UmbDocumentItemRepository(this);

  model?: UmbObjectState<RedirectSelectLinkData>;

  @state()
  linkType: RedirectLinkType = RedirectLinkType.Url;

  @state()
  _content: UmbPropertyValueData[] = [];

  @state()
  _languages: Array<UmbLanguageDetailModel> = [];

  @state()
  _selectedContentName?: {
    name: string;
    icon?: string;
  };

  propertyAppearance: UmbPropertyTypeAppearanceModel = {
    labelOnTop: false,
  };

  constructor() {
    super();
    this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
      this.#languageContext = instance;
    });
  }

  override async connectedCallback() {
    super.connectedCallback();

    this.model = new UmbObjectState(this.data!);
    const { data } = await new UmbLanguageCollectionRepository(
      this,
    ).requestCollection({});

    if (data) {
      this._languages = data.items;

      const passedCulture = this.data?.culture;
      let defaultCulture = this._languages[0]?.unique;

      if (passedCulture) {
        const matchedLanguage = this._languages.find(
          (lan) =>
            lan.unique === passedCulture ||
            lan.name.toLowerCase() === passedCulture.toLowerCase(),
        );
        if (matchedLanguage) {
          defaultCulture = matchedLanguage.unique;
        }
      }

      this.model.update({
        culture: defaultCulture,
      });
      this.#languageContext?.setLanguage(defaultCulture!);
    }

    this.observe(this.model.asObservable(), (value) => {
      const culture =
        this._languages.find((lan) => lan.unique === value.culture) ??
        this._languages[0];
      this._content = [
        {
          alias: "linkType",
          value: value.linkType,
        },
        {
          alias: "url",
          value: value.url,
        },
        {
          alias: "contentKey",
          value: value.contentKey,
        },
        {
          alias: "mediaKey",
          value: value.mediaKey
            ? [
                {
                  key: UmbId.new(),
                  mediaKey: value.mediaKey,
                  crops: [],
                },
              ]
            : [],
        },
        {
          alias: "culture",
          value: culture?.name,
        },
      ];
      this.linkType = value.linkType ?? RedirectLinkType.Url;

      this.#loadContentName(this.model!.getValue().contentKey);
    });
  }

  async openDocumentPicker() {
    const value = await umbOpenModal(this, UMB_DOCUMENT_PICKER_MODAL, {
      data: {
        hideTreeRoot: true,
        pickableFilter: (
          item: UmbDocumentItemModel | UmbDocumentTreeItemModel,
        ) => {
          if (
            typeof item === "object" &&
            item !== null &&
            "noAccess" in item &&
            item.noAccess === true
          ) {
            // https://github.com/umbraco/Umbraco-CMS/blob/main/src/Umbraco.Web.UI.Client/src/packages/documents/documents/tree/utils.ts#L8
            return false;
          }

          const allowedStates = ["Published", "PublishedPendingChanges"];
          const culture = this.model?.getValue().culture;
          var cultureSpecificVariant = item.variants.find(
            (variant) => variant.culture === culture,
          );
          if (
            cultureSpecificVariant &&
            allowedStates.includes(cultureSpecificVariant.state ?? "")
          ) {
            return true;
          }
          var globalVariant = item.variants.find(
            (variant) => variant.culture === null,
          );
          if (
            globalVariant &&
            allowedStates.includes(globalVariant.state ?? "")
          ) {
            return true;
          }
          return false;
        },
      },
    });
    if (value.selection.length === 1) {
      const selectedKey = value.selection[0]!;
      this.model?.update({
        contentKey: selectedKey,
      });
    }
  }

  async #loadContentName(contentKey?: string) {
    if (!contentKey) {
      this._selectedContentName = undefined;
      return;
    }
    const docResponse = await this.#documentRepository.requestItems([
      contentKey,
    ]);
    const item = docResponse.data?.[0];
    const culture = this.model?.getValue().culture;
    const cultureVariant = item?.variants.find(
      (variant) => variant.culture === culture,
    );
    const invariantVariant = item?.variants.find(
      (variant) => variant.culture === null,
    );
    const selectedVariant = cultureVariant ?? invariantVariant;
    if (selectedVariant) {
      this._selectedContentName = {
        name: selectedVariant.name,
        icon: item?.documentType?.icon?.split(" ")[0],
      };
    } else {
      this._selectedContentName = {
        name: contentKey,
        icon: undefined,
      };
    }
  }

  #onPropertyDataChange(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    const newValue = {} as any;
    const modelKeys = Object.keys(this.model!.getValue());
    value.forEach((item) => {
      if (item.alias === "culture") {
        const itemValue = (item.value as string[])[0];
        const culture = this._languages.find((lan) => lan.name === itemValue);
        if (!culture) {
          return;
        }
        newValue[item.alias] = culture.unique;
        this.#languageContext?.setLanguage(culture.unique);
        newValue["contentKey"] = undefined;
      } else if (modelKeys.includes(item.alias)) {
        if (Array.isArray(item.value)) {
          if (item.alias === "mediaKey") {
            newValue[item.alias] = item.value[0]?.mediaKey;
          } else {
            newValue[item.alias] = item.value[0];
          }
        } else {
          newValue[item.alias] = item.value;
        }
      }
    });
    this.model?.update(newValue);
  }

  #handleSubmit() {
    const modelValue = this.model!.getValue();

    this.value = {
      ...modelValue,
      culture:
        modelValue.linkType === RedirectLinkType.Media
          ? undefined
          : modelValue.culture,
    };
    this.modalContext?.submit();
  }

  render() {
    return html`
      <umb-body-layout headline="Set link">
        <uui-box>
          <umb-property-dataset
            .value=${this._content!}
            @change=${this.#onPropertyDataChange}
          >
            <umb-property
              alias="linkType"
              label="Link type"
              description="Choose the type of link that you want to redirect to"
              property-editor-ui-alias="Umb.PropertyEditorUi.RadioButtonList"
              val
              required
              .config=${[
                {
                  alias: "items",
                  value: ["Url", "Content", "Media"],
                },
              ]}
            >
            </umb-property>
            ${when(
              this.linkType === RedirectLinkType.Url,
              () => html`
                <umb-property
                  alias="url"
                  label="Url"
                  description="Choose the url that you want to redirect to"
                  property-editor-ui-alias="Umb.PropertyEditorUi.TextBox"
                  val
                >
                </umb-property>
              `,
            )}
            ${when(
              this.linkType === RedirectLinkType.Content,
              () => html`
                ${when(
                  this._languages.length > 1,
                  () => html`
                    <umb-property
                      alias="culture"
                      label="Culture"
                      description="Choose the type of link that you want to redirect to"
                      property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
                      val
                      .validation=${{
                        mandatory: this._languages.length > 1,
                        mandatoryMessage: "This field is required",
                      }}
                      .config=${[
                        {
                          alias: "items",
                          value: this._languages.map((item) => item.name),
                        },
                      ]}
                    >
                    </umb-property>
                  `,
                )}

                <umb-property-layout
                  label="Content"
                  description="Select the content item you would like to connect to"
                >
                  <div slot="editor">
                    <uui-button
                      .look="${this._selectedContentName
                        ? "outline"
                        : "placeholder"}"
                      @click=${() => this.openDocumentPicker()}
                      class="document-picker-button"
                    >
                      ${when(
                        this._selectedContentName,
                        () => html`
                          <uui-icon
                            name="${this._selectedContentName?.icon ?? "cube"}"
                          ></uui-icon>
                          ${this._selectedContentName?.name}
                        `,
                        () => "Select content",
                      )}
                    </uui-button>
                  </div>
                </umb-property-layout>
              `,
            )}
            ${when(
              this.linkType === RedirectLinkType.Media,
              () => html`
                <umb-property
                  alias="mediaKey"
                  label="Media"
                  property-editor-ui-alias="Umb.PropertyEditorUi.MediaPicker"
                  val
                  required
                  .config=${[
                    {
                      alias: "max",
                      value: 1,
                    },
                  ]}
                >
                </umb-property>
              `,
            )}
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

  static styles = css`
    .document-picker-button {
      width: 100%;
    }
  `;
}
