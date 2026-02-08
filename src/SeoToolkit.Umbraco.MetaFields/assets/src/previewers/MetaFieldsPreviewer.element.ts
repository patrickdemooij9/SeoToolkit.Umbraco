import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement, property, state } from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import { SeoSettingsFieldViewModel } from "../api";
import { ISeoContentPreviewer } from "./ISeoContentPreviewer";
import { MetaFieldsSettingsSource } from "../dataAccess/MetaFieldsSettingsSource";

@customElement("st-metafieldspreviewer")
export default class MetaFieldsPreviewer
  extends UmbElementMixin(LitElement)
  implements ISeoContentPreviewer
{
  @property({ type: Array })
  public value: SeoSettingsFieldViewModel[] = [];

  @state()
  public titleTemplate: string = "%value%";

  constructor() {
    super();
    
    new MetaFieldsSettingsSource(this).getTitleFormat("pageTitleTemplate").then((response) => {
      if (response.data != ""){
        this.titleTemplate = response.data;
      }
    })
  }

  public getValue(key: string) {
    const foundItem = this.value.find((item) => item.alias === key);
    if (!foundItem) {
      return "";
    }
    const returnValue = foundItem.userValue?.toString();
    if (returnValue && returnValue !== ''){
        return returnValue;
    }
    return foundItem.value;
  }

  protected render() {
    return html` <div class="google-preview">
      <div class="listing">
        <a .href=${this.getValue("canonicalUrl")!}>
          <p class="link">${this.getValue("canonicalUrl")}</p>
          <h3>${this.titleTemplate.replace("%value%", this.getValue("title") ?? "")}</h3>
        </a>
        <p class="description">${this.getValue("metaDescription")}</p>
      </div>
    </div>`;
  }

  static styles = [
    css`
      .google-preview {
        min-height: 120px;
        max-width: 520px;
        border-radius: 2px;
        font-family: arial, sans-serif;
        zoom: 1;
      }

      .google-preview p {
        margin: 0;
      }

      .google-preview h3 {
        font-size: 20px;
        line-height: 1.3;
        margin: 0;
        padding: 5px 0 3px 0;
        font-weight: normal;
        color: #1a0dab;
      }

      .google-preview .link {
        color: #202124;
        line-height: 1.3;
        font-size: 14px;
      }

      .google-preview a {
        text-decoration: none;
      }

      .google-preview .description {
        max-width: 504px;
        color: #4d5156;
        font-size: 14px;
        line-height: 1.58;
      }

      .google-review .listing {
        margin: 23px 0;
      }
    `,
  ];
}
