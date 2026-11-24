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

  @state()
  private _faviconFailed = false;

  @property({ type: Boolean, reflect: true })
  public dark = false;

  constructor() {
    super();

    new MetaFieldsSettingsSource(this)
      .getTitleFormat("pageTitleTemplate")
      .then((response) => {
        if (response.data && response.data.value != '') {
          this.titleTemplate = response.data.value ?? "%value%";
        }
      });

    this.consumeContext(UMB_THEME_CONTEXT, (instance) => {
      this.observe(instance?.theme, (themeAlias) => {
        this.dark = themeAlias === "umb-dark-theme";
      });
    });
  }

  public getValue(key: string) {
    const foundItem = this.value.find((item) => item.alias === key);
    if (!foundItem) {
      return "";
    }
    const returnValue = foundItem.userValue?.toString();
    if (returnValue && returnValue !== "") {
      return returnValue;
    }
    return foundItem.value;
  }

  #getUrl() {
    const canonicalUrl = this.getValue("canonicalUrl") ?? "";
    try {
      return new URL(canonicalUrl, window.location.origin);
    } catch {
      return undefined;
    }
  }

  #getBreadcrumb(url: URL) {
    const segments = url.pathname.split("/").filter((segment) => segment);
    return [`${url.protocol}//${url.host}`, ...segments].join(" › ");
  }

  protected render() {
    const url = this.#getUrl();
    const siteName = url?.hostname.replace(/^www\./, "") ?? "";
    const title = this.titleTemplate.replace(
      "%value%",
      this.getValue("title") ?? "",
    ).trim();
    const description = this.getValue("metaDescription");

    return html`<div class="preview">
      <div class="preview-label">
        <uui-icon name="icon-search"></uui-icon>
        <span>Google search preview</span>
      </div>
      <div class="google-preview">
        <a class="site" .href=${url?.href ?? ""} target="_blank" rel="noopener">
          <span class="favicon">
            ${when(
              url && !this._faviconFailed,
              () => html`<img
                src="${url!.origin}/favicon.ico"
                alt=""
                @error=${() => (this._faviconFailed = true)}
              />`,
              () => html`<uui-icon name="icon-globe"></uui-icon>`,
            )}
          </span>
          <span class="site-text">
            <span class="site-name">${siteName}</span>
            <cite class="site-url">${url ? this.#getBreadcrumb(url) : ""}</cite>
          </span>
        </a>
        <a class="title" .href=${url?.href ?? ""} target="_blank" rel="noopener">
          <h3 class=${title ? "" : "empty"}>${title || "No title set"}</h3>
        </a>
        <p class="description ${description ? "" : "empty"}">
          ${description || "No meta description set"}
        </p>
      </div>
      <small class="preview-hint">
        This is an approximation. Google may shorten or rewrite the title and
        description.
      </small>
    </div>`;
  }

  static styles = [
    css`
      :host {
        --st-google-background: #ffffff;
        --st-google-border: #dadce0;
        --st-google-site-name: #202124;
        --st-google-url: #4d5156;
        --st-google-title: #1a0dab;
        --st-google-text: #474747;
        --st-google-muted: #70757a;
        --st-google-favicon-background: #f1f3f4;
        display: block;
      }

      :host([dark]) {
        --st-google-background: #1f1f1f;
        --st-google-border: #3c4043;
        --st-google-site-name: #dadce0;
        --st-google-url: #bdc1c6;
        --st-google-title: #99c3ff;
        --st-google-text: #bfbfbf;
        --st-google-muted: #9aa0a6;
        --st-google-favicon-background: #303134;
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

      .google-preview {
        max-width: 600px;
        padding: 16px 20px;
        border: 1px solid var(--st-google-border);
        border-radius: 12px;
        background: var(--st-google-background);
        font-family: arial, sans-serif;
        box-sizing: border-box;
      }

      .google-preview a {
        text-decoration: none;
      }

      .site {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 4px;
      }

      .favicon {
        display: flex;
        flex-shrink: 0;
        align-items: center;
        justify-content: center;
        width: 28px;
        height: 28px;
        border: 1px solid var(--st-google-border);
        border-radius: 50%;
        background: var(--st-google-favicon-background);
        color: var(--st-google-muted);
        font-size: 14px;
      }

      .favicon img {
        width: 18px;
        height: 18px;
      }

      .site-text {
        display: flex;
        flex-direction: column;
        min-width: 0;
      }

      .site-name,
      .site-url {
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
        line-height: 20px;
      }

      .site-name {
        color: var(--st-google-site-name);
        font-size: 14px;
      }

      .site-url {
        color: var(--st-google-url);
        font-size: 12px;
        font-style: normal;
        line-height: 18px;
      }

      h3 {
        margin: 0;
        padding: 5px 0 3px 0;
        overflow: hidden;
        color: var(--st-google-title);
        font-size: 20px;
        font-weight: normal;
        line-height: 26px;
        white-space: nowrap;
        text-overflow: ellipsis;
      }

      .title:hover h3 {
        text-decoration: underline;
      }

      .description {
        display: -webkit-box;
        margin: 0;
        overflow: hidden;
        color: var(--st-google-text);
        font-size: 14px;
        line-height: 22px;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
      }

      .empty {
        color: var(--st-google-muted);
        font-style: italic;
      }

      .preview-hint {
        display: block;
        max-width: 600px;
        margin-top: var(--uui-size-space-2);
        color: var(--uui-color-text-alt);
      }
    `,
  ];
}
