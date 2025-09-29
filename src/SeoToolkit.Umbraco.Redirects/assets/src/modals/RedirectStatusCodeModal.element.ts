import { customElement, state } from "@umbraco-cms/backoffice/external/lit";
import { html } from "lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import { UmbNumberState } from "@umbraco-cms/backoffice/observable-api";

@customElement("st-redirect-status-code-modal")
export default class RedirectStatusCodeModalElement extends UmbModalBaseElement<
  never,
  number
> {
  @state()
  _content: UmbPropertyValueData[] = [];

  @state()
  redirectStatusCode = new UmbNumberState(undefined);

  constructor() {
    super();

    this.observe(this.redirectStatusCode.asObservable(), (value) => {
      let propValue: string | undefined = undefined;
      if (value === 301) {
        propValue = "Permanent (301)";
      } else if (value === 302) {
        propValue = "Temporary (302)";
      }
      this._content = [
        {
          alias: "redirectCode",
          value: propValue,
        },
      ];
    });
  }

  #handleStatusCodeChange(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    let propValue: number | undefined = undefined;
    value.forEach((item) => {
      if (!item.value) {
        return;
      }

      if (item.alias === "redirectCode") {
        propValue = item.value === "Permanent (301)" ? 301 : 302;
      }
    });
    this.redirectStatusCode.setValue(propValue);
    e.preventDefault();
  }

  #handleSubmit() {
    this.value = this.redirectStatusCode.value!;
    this.modalContext?.submit();
  }

  override render() {
    return html`
      <umb-body-layout headline="Update status codes">
        <uui-box>
          <umb-property-dataset
            .value=${this._content!}
            @change=${this.#handleStatusCodeChange}
          >
            <umb-property
              alias="redirectCode"
              label="Status code"
              description="Status code of the redirects"
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
              id="submit"
              label="Validate"
              look="primary"
              color="positive"
              .disabled=${!this.redirectStatusCode.value}
              @click="${this.#handleSubmit}"
              >Submit</uui-button
            >
          </slot>
        </umb-workspace-footer>
      </umb-body-layout>
    `;
  }
}
