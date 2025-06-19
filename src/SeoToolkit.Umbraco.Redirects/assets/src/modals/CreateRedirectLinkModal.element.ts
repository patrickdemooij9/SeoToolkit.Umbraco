import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { RedirectSelectLinkData } from "../models/RedirectSelectLinkData";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import {
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
  UmbLanguageCollectionRepository,
  UmbLanguageDetailModel,
} from "@umbraco-cms/backoffice/language";
import { UmbId } from "@umbraco-cms/backoffice/id";

@customElement("st-create-redirect-link-modal")
export default class CreateRedirectLinkModal extends UmbModalBaseElement<
  RedirectSelectLinkData,
  RedirectSelectLinkData
> {
  model?: UmbObjectState<RedirectSelectLinkData>;

  @state()
  linkType: RedirectLinkType = RedirectLinkType.Url;

  @state()
  _content: UmbPropertyValueData[] = [];

  @state()
  _languages: Array<UmbLanguageDetailModel> = [];

  propertyAppearance: UmbPropertyTypeAppearanceModel = {
    labelOnTop: false,
  };

  override async connectedCallback() {
    super.connectedCallback();

    this.model = new UmbObjectState(this.data!);
    const { data } = await new UmbLanguageCollectionRepository(
      this
    ).requestCollection({});

    if (data) {
      this._languages = data.items;
      this.model.update({
        culture: this._languages[0].unique,
      });
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
    });
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
        this.modalContext?.consumeContext(
          UMB_APP_LANGUAGE_CONTEXT,
          (instance) => {
            instance?.setLanguage(culture.unique);
          }
        );
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
        culture: modelValue.linkType === RedirectLinkType.Media ? undefined : modelValue.culture
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
              `
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
                      required
                      .config=${[
                        {
                          alias: "items",
                          value: this._languages.map((item) => item.name),
                        },
                      ]}
                    >
                    </umb-property>
                  `
                )}

                <umb-property
                  alias="contentKey"
                  label="Content"
                  property-editor-ui-alias="Umb.PropertyEditorUi.DocumentPicker"
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
              `
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
              `
            )}
          </umb-property-dataset>
        </uui-box>

        <umb-workspace-footer slot="footer" data-mark="workspace:footer">
			<slot name="footer-info"></slot>
			<slot name="actions" slot="actions" data-mark="workspace:footer-actions">
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
}
