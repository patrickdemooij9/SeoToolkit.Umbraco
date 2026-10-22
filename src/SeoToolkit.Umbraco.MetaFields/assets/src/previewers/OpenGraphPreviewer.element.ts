import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  customElement,
  property,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { UMB_THEME_CONTEXT } from "@umbraco-cms/backoffice/themes";
import { css, html, LitElement } from "lit";
import { SeoSettingsFieldViewModel } from "../api";
import { ISeoContentPreviewer } from "./ISeoContentPreviewer";

import "@umbraco-cms/backoffice/imaging";
import { MetaFieldsContentRepository } from "../dataAccess/MetaFieldsContentRepository";

interface UmbMediaPickerPropertyValueEntry {
  mediaKey: string;
}

@customElement("st-opengraphpreviewer")
export default class OpenGraphPreviewer
  extends UmbElementMixin(LitElement)
  implements ISeoContentPreviewer
{
  #imagingRepository = new MetaFieldsContentRepository(this);

  @property({ type: Array })
  public set value(value: SeoSettingsFieldViewModel[]) {
    this._value = value;
    if (!this.loading) {
      this.#generateImageUrl();
      this.requestUpdate();
    }
  }

  private _value: SeoSettingsFieldViewModel[] = [];

  @state()
  public currentTab: string = "facebook";

  @state()
  public imageUrl: string | undefined;

  @state()
  public loading = true;

  @property({ type: Boolean, reflect: true })
  public dark = false;

  constructor() {
    super();

    this.consumeContext(UMB_THEME_CONTEXT, (instance) => {
      this.observe(instance?.theme, (themeAlias) => {
        this.dark = themeAlias === "umb-dark-theme";
      });
    });
  }

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
      const { data } = await this.#imagingRepository.getImagePreview(userValue[0].mediaKey);

      this.imageUrl = data;
    } else {
      this.imageUrl = foundItem?.value ?? "";
    }
  }

  override async connectedCallback() {
    super.connectedCallback();
    await this.#generateImageUrl();
    this.loading = false;
  }

  public getDomain() {
    return window.location.hostname;
  }

  public handleTabClick(tab: string) {
    this.currentTab = tab;
  }

  #renderImage() {
    return when(
      this.imageUrl,
      () => html`<div
        class="card-image"
        style="background-image: url(${this.imageUrl})"
      ></div>`,
      () => html`<div class="card-image empty-image">
        <uui-icon name="icon-picture"></uui-icon>
        <span>No image set</span>
      </div>`
    );
  }

  #renderTitle() {
    const title = this.getValue("openGraphTitle");
    return title
      ? html`${title}`
      : html`<span class="empty">No title set</span>`;
  }

  #renderDescription() {
    const description = this.getValue("openGraphDescription");
    return description
      ? html`${description}`
      : html`<span class="empty">No description set</span>`;
  }

  protected render() {
    return html` <div class="open-graph-previewer">
      <div class="preview-label">
        <uui-icon name="icon-share-alt"></uui-icon>
        <span>Social media preview</span>
      </div>
      <uui-tab-group class="navigation">
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
          X (Twitter)
        </uui-tab>
        <uui-tab
          ?active=${this.currentTab === "linkedin"}
          @click=${() => this.handleTabClick("linkedin")}
        >
          LinkedIn
        </uui-tab>
      </uui-tab-group>

      <div class="previewers">
        ${when(
          this.currentTab === "facebook",
          () => html`
            <div class="facebook-previewer">
              <div class="card">
                ${this.#renderImage()}
                <div class="card-text">
                  <p class="card-subtitle">${this.getDomain()}</p>
                  <div class="card-title">${this.#renderTitle()}</div>
                  <div class="card-description">
                    ${this.#renderDescription()}
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
                ${this.#renderImage()}
                <div class="card-title">${this.#renderTitle()}</div>
              </div>
              <p class="card-subtitle">From ${this.getDomain()}</p>
            </div>
          `
        )}
        ${when(
          this.currentTab === "linkedin",
          () => html`
            <div class="linkedin-previewer">
              <div class="card">
                ${this.#renderImage()}
                <div class="card-text">
                  <h2 class="card-title">${this.#renderTitle()}</h2>
                  <h3 class="card-subtitle">${this.getDomain()}</h3>
                </div>
              </div>
            </div>
          `
        )}
      </div>
      <small class="preview-hint">
        This is an approximation. Each platform may crop the image or shorten
        the text differently.
      </small>
    </div>`;
  }

  static styles = [
    css`
      :host {
        --st-facebook-background: #f0f2f5;
        --st-facebook-border: #dadde1;
        --st-facebook-title: #080809;
        --st-facebook-text: #65686c;

        --st-twitter-border: #cfd9de;
        --st-twitter-text: #536471;
        --st-twitter-label-background: rgba(0, 0, 0, 0.77);

        --st-linkedin-background: #ffffff;
        --st-linkedin-border: #e0dfdc;
        --st-linkedin-title: rgba(0, 0, 0, 0.9);
        --st-linkedin-text: rgba(0, 0, 0, 0.6);

        --st-empty-image-background: #e4e6eb;
        --st-empty-image-text: #65686c;
        display: block;
      }

      :host([dark]) {
        --st-facebook-background: #3a3b3c;
        --st-facebook-border: #3e4042;
        --st-facebook-title: #e4e6eb;
        --st-facebook-text: #b0b3b8;

        --st-twitter-border: #2f3336;
        --st-twitter-text: #71767b;

        --st-linkedin-background: #1b1f23;
        --st-linkedin-border: #38434f;
        --st-linkedin-title: rgba(255, 255, 255, 0.9);
        --st-linkedin-text: rgba(255, 255, 255, 0.6);

        --st-empty-image-background: #303134;
        --st-empty-image-text: #9aa0a6;
      }

      .preview-label {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-2);
        margin-bottom: var(--uui-size-space-3);
        color: var(--uui-color-text-alt);
        font-size: var(--uui-type-small-size);
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }

      .navigation {
        margin-bottom: var(--uui-size-space-4);
        border-bottom: 1px solid var(--uui-color-border);
      }

      .previewers {
        max-width: 500px;
      }

      .card-image {
        aspect-ratio: 1.91 / 1;
        background-size: cover;
        background-position: center;
      }

      .empty-image {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 6px;
        background: var(--st-empty-image-background);
        color: var(--st-empty-image-text);
        font-family: arial, sans-serif;
        font-size: 13px;
      }

      .empty-image uui-icon {
        font-size: 28px;
      }

      .empty {
        font-style: italic;
        font-weight: normal;
        opacity: 0.8;
      }

      .preview-hint {
        display: block;
        max-width: 500px;
        margin-top: var(--uui-size-space-2);
        color: var(--uui-color-text-alt);
      }

      /* Facebook */
      .facebook-previewer .card {
        overflow: hidden;
        border: 1px solid var(--st-facebook-border);
        border-radius: 8px;
        font-family: Helvetica, Arial, sans-serif;
      }

      .facebook-previewer .card-text {
        padding: 10px 12px;
        background-color: var(--st-facebook-background);
        border-top: 1px solid var(--st-facebook-border);
      }

      .facebook-previewer .card-subtitle {
        margin: 0;
        overflow: hidden;
        color: var(--st-facebook-text);
        font-size: 12px;
        line-height: 16px;
        text-overflow: ellipsis;
        text-transform: uppercase;
        white-space: nowrap;
      }

      .facebook-previewer .card-title {
        margin: 3px 0 0;
        overflow: hidden;
        color: var(--st-facebook-title);
        font-size: 16px;
        font-weight: 600;
        line-height: 20px;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      .facebook-previewer .card-description {
        margin: 3px 0 0;
        overflow: hidden;
        color: var(--st-facebook-text);
        font-size: 14px;
        line-height: 20px;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      /* X (Twitter) */
      .twitter-previewer {
        font-family: TwitterChirp, -apple-system, BlinkMacSystemFont, "Segoe UI",
          Roboto, Helvetica, Arial, sans-serif;
      }

      .twitter-previewer .card {
        position: relative;
        overflow: hidden;
        border: 1px solid var(--st-twitter-border);
        border-radius: 16px;
      }

      .twitter-previewer .card-title {
        position: absolute;
        bottom: 12px;
        left: 12px;
        max-width: calc(100% - 40px);
        padding: 0 4px;
        overflow: hidden;
        border-radius: 4px;
        background: var(--st-twitter-label-background);
        color: #ffffff;
        font-size: 13px;
        line-height: 20px;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      .twitter-previewer .card-subtitle {
        margin: 4px 0 0;
        overflow: hidden;
        color: var(--st-twitter-text);
        font-size: 13px;
        line-height: 16px;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      /* LinkedIn */
      .linkedin-previewer .card {
        overflow: hidden;
        border: 1px solid var(--st-linkedin-border);
        border-radius: 8px;
        background-color: var(--st-linkedin-background);
        font-family: -apple-system, system-ui, "Segoe UI", Roboto, Helvetica,
          Arial, sans-serif;
      }

      .linkedin-previewer .card-text {
        padding: 8px 12px 12px;
      }

      .linkedin-previewer .card-title {
        display: -webkit-box;
        margin: 0;
        overflow: hidden;
        color: var(--st-linkedin-title);
        font-size: 14px;
        font-weight: 600;
        line-height: 20px;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
      }

      .linkedin-previewer .card-subtitle {
        margin: 4px 0 0;
        overflow: hidden;
        color: var(--st-linkedin-text);
        font-size: 12px;
        font-weight: 400;
        line-height: 16px;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    `,
  ];
}
