import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  css,
  customElement,
  html,
  LitElement,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import NotFoundModuleWorkspaceContext, {
  ST_NOTFOUND_MODULE_TOKEN_CONTEXT,
} from "./NotFoundModuleWorkspaceContext";

@customElement("seotoolkit-notfound-module")
export default class NotFoundModuleWorkspaceElement extends UmbElementMixin(
  LitElement
) {
  #context?: NotFoundModuleWorkspaceContext;

  @state()
  _content?: UmbPropertyValueData;

  constructor() {
    super();

    this.consumeContext(ST_NOTFOUND_MODULE_TOKEN_CONTEXT, (instance) => {
      this.#context = instance;
      instance.content.subscribe((value) => {
        this._content = {
          alias: "contentNode",
          value,
        };
      });
    });
  }

  #onPropertyDataChange(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    const foundItem = value.find((item) => item.alias === "contentNode");
    if (foundItem) {
      this.#context?.update(foundItem.value as string);
    }
  }

  override render() {
    return html`
      <umb-workspace-editor>
        <div class="notFoundWorkspace">
          <uui-box headline="NotFound" headline-variant="h5">
            <umb-property-dataset
              .value=${[this._content!]}
              @change=${this.#onPropertyDataChange}
            >
              <umb-property
                alias="contentNode"
                label="Page not found"
                description="Select your 404 page here."
                property-editor-ui-alias="Umb.PropertyEditorUi.DocumentPicker"
                .config=${[
                  {
                    alias: "max",
                    value: 1,
                  },
                ]}
              >
              </umb-property>
            </umb-property-dataset>
          </uui-box>
        </div>
      </umb-workspace-editor>
    `;
  }

  static styles = [
    css`
      .notFoundWorkspace {
        padding: var(--uui-size-layout-1);
      }
    `,
  ];
}
