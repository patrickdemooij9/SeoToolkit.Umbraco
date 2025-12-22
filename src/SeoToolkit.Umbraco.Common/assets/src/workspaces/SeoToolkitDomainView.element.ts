import { css, html } from "lit";
import SeoToolkitDomainContext, {
  ST_DOMAIN_DETAIL_TOKEN_CONTEXT,
} from "./SeoToolkitDomainContext";
import { umbFocus, UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UUIInputElement } from "@umbraco-cms/backoffice/external/uui";
import { customElement, state } from "@umbraco-cms/backoffice/external/lit";

@customElement("seotoolkit-domain-detail")
export default class SeoToolkitDomainViewElement extends UmbLitElement {
  #context?: SeoToolkitDomainContext;

  @state()
  private _name: string = "";

  constructor() {
    super();

    this.consumeContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT, (instance) => {
      if (!instance) {
        return;
      }

      this.#context = instance;

      this.observe(this.#context.domain, (value) => {
        this._name = value.name!;
      });
    });
  }

  #onNameInput(event: Event) {
    const target = event.target as UUIInputElement;
    const value = target.value as string;
    this.#context?.updateDomain({
      name: value,
    });
  }

  protected render() {
    return html` <umb-workspace-editor alias="seoToolkit.domain.detail">
      <div id="workspace-header" slot="header">
        <uui-input
          placeholder=${this.localize.term("placeholders_entername")}
          .value=${this._name}
          @input=${this.#onNameInput}
          label=${this.localize.term("placeholders_entername")}
          required="true"
          required-message="Name is required"
          ${umbFocus()}
        >
        </uui-input>
      </div>
    </umb-workspace-editor>`;
  }

  static styles = [
    css`
      #workspace-header {
        width: 100%;
      }

      uui-input {
        width: 100%;
      }
    `,
  ];
}
