import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { css, html, LitElement } from "lit";
import { SeoSettingsFieldViewModel } from "../api";
import { ISeoContentPreviewer } from "./ISeoContentPreviewer";

import "@umbraco-cms/backoffice/imaging";
import { UmbImagingRepository } from "@umbraco-cms/backoffice/imaging";

interface UmbMediaPickerPropertyValueEntry {
  mediaKey: string;
}

@customElement("st-opengraphpreviewer")
export default class OpenGraphPreviewer
  extends UmbElementMixin(LitElement)
  implements ISeoContentPreviewer
{
  #imagingRepository = new UmbImagingRepository(this);

  @property({ type: Array })
  public set value(value: SeoSettingsFieldViewModel[]) {
    this._value = value;
    this.#generateImageUrl();
    this.requestUpdate();
  }

  private _value: SeoSettingsFieldViewModel[] = [];

  @state()
  public currentTab: string = "facebook";

  @state()
  public imageUrl: string | undefined;

  public getValue(key: string) {
    const foundItem = this._value.find((item) => item.alias === key);
    if (!foundItem) {
      return "";
    }
    const returnValue = foundItem.userValue?.toString();
    if (returnValue && returnValue !== "") {
      return returnValue;
    }
    return foundItem.value;
  }

  async #generateImageUrl() {
    const foundItem = this._value.find(
      (item) => item.alias === "openGraphImage"
    );
    if (!foundItem) {
      this.imageUrl = "";
    }
    if (foundItem?.userValue) {
      const userValue =
        foundItem.userValue as UmbMediaPickerPropertyValueEntry[];
      if (userValue.length === 0) {
        this.imageUrl = foundItem?.value ?? "";
        return;
      }
      const { data } = await this.#imagingRepository.requestThumbnailUrls(
        [userValue[0].mediaKey],
        200,
        300
      );

      this.imageUrl = data?.[0]?.url ?? "";
    } else {
      this.imageUrl = foundItem?.value ?? "";
    }
  }

  public getDomain() {
    return window.location.hostname;
  }

  public handleTabClick(tab: string) {
    this.currentTab = tab;
  }

  protected render() {
    return html` <div class="open-graph-previewer">
      <div class="umb-editor-tab-bar">
        <uui-tab-group class="navigation" slot="header">
          <uui-tab
            ?active=${this.currentTab === "facebook"}
            @click=${() => this.handleTabClick("facebook")}
          >
            Facebook
          </uui-tab>
          <uui-tab
            ?active=${this.currentTab === "twitter"}
            @click=${() => this.handleTabClick("twitter")}
          >
            Twitter
          </uui-tab>
          <uui-tab
            ?active=${this.currentTab === "linkedin"}
            @click=${() => this.handleTabClick("linkedin")}
          >
            Linkedin
          </uui-tab>
        </uui-tab-group>
      </div>

      <div class="previewers">
        ${when(
          this.currentTab === "facebook",
          () => html`
            <div class="facebook-previewer">
              <div class="card">
                <div
                  class="card-image"
                  .style="background-image: url(${this.imageUrl})"
                ></div>
                <div class="card-text">
                  <p class="card-subtitle">${this.getDomain()}</p>
                  <div class="card-content">
                    <div class="card-title">
                      ${this.getValue("openGraphTitle")}
                    </div>
                    <div class="card-description">
                      ${this.getValue("openGraphDescription")}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          `
        )}
        ${when(
          this.currentTab === "twitter",
          () => html`
            <div class="twitter-previewer">
              <div class="card">
                <div
                  class="card-image"
                  .style="background-image: url(${this.imageUrl})"
                ></div>
                <div class="card-text">
                  <p class="card-subtitle">${this.getDomain()}</p>
                  <div class="card-content">
                    <div class="card-title">
                      ${this.getValue("openGraphTitle")}
                    </div>
                    <div class="card-description">
                      ${this.getValue("openGraphDescription")}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          `
        )}
        ${when(
          this.currentTab === "linkedin",
          () => html`
            <div class="linkedin-previewer">
              <div class="card">
                <div
                  class="card-image"
                  .style="background-image: url(${this.imageUrl})"
                ></div>
                <div class="card-text">
                  <h2 class="card-title">${this.getValue("openGraphTitle")}</h2>
                  <h3 class="card-subtitle">${this.getDomain()}</h3>
                </div>
              </div>
            </div>
          `
        )}
      </div>
    </div>`;
  }

  static styles = [
    css`
      .facebook-previewer .card {
        width: 300px;
        font-family: Helvetica, Arial, sans-serif;
      }

      .facebook-previewer .card-image {
        height: 157px;
      }

      .facebook-previewer .card-text {
        padding: 10px 12px;
        margin: 0;
        font-size: 14px;
        color: #4b4f56;
        background-color: #f2f3f5;
        border-left: 1px solid #dadde1;
        border-right: 1px solid #dadde1;
        border-bottom: 1px solid #dadde1;
      }

      .facebook-previewer .card-content {
        max-height: 46px;
        overflow: hidden;
      }

      .facebook-previewer .card-subtitle {
        color: #606770;
        flex-shrink: 0;
        font-size: 12px;
        line-height: 16px;
        overflow: hidden;
        padding: 0;
        text-overflow: ellipsis;
        text-transform: uppercase;
        white-space: nowrap;
        margin: 0;
      }

      .facebook-previewer .card-title {
        font-weight: 600;
        font-family: inherit;
        font-size: 16px;
        line-height: 20px;
        margin: 3px 0 0;
        padding-top: 2px;
        color: #1d2129;
      }

      .facebook-previewer .card-description {
        margin: 3px 0 0 0;
        color: #606770;
        font-size: 14px;
        line-height: 20px;
        word-break: break-word;
        max-height: 80px;
        overflow: hidden;
        text-overflow: clip;
        white-space: normal;
        -webkit-line-clamp: 1;
        border-collapse: collapse;
        display: -webkit-box;
        overflow-wrap: break-word;
        -webkit-box-orient: vertical;
      }

      .twitter-previewer .card {
        width: 300px;
        max-width: 100%;
        overflow: hidden;
        border-radius: 16px;
        border: 1px solid #e1e8ed;
        background: #fff;
        font-family: TwitterChirp, -apple-system, BlinkMacSystemFont, "Segoe UI",
          Roboto, Helvetica, Arial, sans-serif;
        line-height: 1.3em;
        cursor: pointer;
        font-size: 15px;
      }

      .twitter-previewer .card-image {
        height: 145px;
        border-bottom: 1px solid #e1e8ed;
      }

      .twitter-previewer .card-text {
        padding: 12px;
        margin: 0;
        background-color: white;
        gap: 2px;
        -webkit-box-pack: center;
        justify-content: center;
        display: flex;
        flex-basis: auto;
        flex-direction: column;
      }

      .twitter-previewer .card-subtitle {
        overflow-wrap: break-word;
        color: rgb(83, 100, 113);
        font-weight: 400;
        font-size: 15px;
        line-height: 20px;
        min-width: 0px;
        max-width: 100%;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        margin: 0;
      }

      .twitter-previewer .card-title {
        overflow-wrap: break-word;
        color: rgb(15, 20, 25);
        font-weight: 400;
        font-size: 15px;
        line-height: 20px;
        min-width: 0px;
        max-width: 100%;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        margin: 0;
      }

      .twitter-previewer .card-description {
        color: rgb(83, 100, 113);
        font-weight: 400;
        font-size: 15px;
        line-height: 20px;
        min-width: 0px;
        max-width: 100%;
        overflow: hidden;
        text-overflow: ellipsis;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        display: -webkit-box;
      }

      .linkedin-previewer .card {
        width: 300px;
        overflow: hidden;
        margin: 0;
        background-color: #eef3f8;
      }

      .linkedin-previewer .card-image {
        height: 187px;
      }

      .linkedin-previewer .card-text {
        padding: 8px 12px;
      }

      .linkedin-previewer .card-title {
        display: -webkit-box;
        -webkit-box-orient: vertical;
        -webkit-line-clamp: 2;
        max-height: 4rem;
        overflow: hidden;
        text-overflow: ellipsis;
        color: rgba(0, 0, 0, 0.9);
        font-size: 1rem;
        font-weight: 600;
        margin: 0;
        line-height: 20px;
      }

      .linkedin-previewer .card-subtitle {
        margin-top: 8px;
        max-height: none;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        color: rgba(0, 0, 0, 0.6);
        font-size: 0.8rem;
        font-weight: 400;
        margin: 0;
        line-height: 16px;
      }
    `,
  ];
}
