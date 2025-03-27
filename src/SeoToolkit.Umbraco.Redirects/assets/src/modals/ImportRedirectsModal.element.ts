import {
  customElement,
  state,
  when,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import {
  UmbPropertyDatasetElement,
  UmbPropertyValueData,
} from "@umbraco-cms/backoffice/property";
import { css, html } from "lit";
import { DomainViewModel } from "../api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import RedirectRepository from "../dataLayer/RedirectRepository";
import { ApiError } from "@umbraco-cms/backoffice/external/backend-api";

interface ImportFileType {
  label: string;
  extensions: string[];
}

interface State {
  domain?: number;
  fileType?: ImportFileType;
  file?: MediaValueType;
  notification?: string;
  isValid: boolean;
}

interface MediaValueType {
  temporaryFileId?: string | null;
  src?: string;
}

@customElement("st-import-redirect-modal")
export default class ImportRedirectsModal extends UmbModalBaseElement {
  #redirectRepository = new RedirectRepository(this);

  @state()
  _content: UmbPropertyValueData[] = [];

  @state()
  _domains: Array<DomainViewModel> = [];

  @state()
  _fileTypes: Array<ImportFileType> = [
    {
      label: "CSV",
      extensions: ["csv"],
    },
    {
      label: "Excel",
      extensions: ["xls", "xlsx"],
    },
  ];

  @state()
  _canValidate = false;

  State: UmbObjectState<State> = new UmbObjectState<State>({ isValid: false });

  override async connectedCallback() {
    super.connectedCallback();

    this._domains = (await this.#redirectRepository.getDomains()).data!;
    this._domains.splice(0, 0, { id: 0, name: "All Sites" });

    this.State.asObservable().subscribe((value) => {
      const domain = value.domain
        ? this._domains.find((item) => item.id === value.domain)
        : undefined;

      this._content = [
        {
          alias: "domain",
          value: domain ? [domain.name] : [],
        },
        {
          alias: "fileType",
          value: value.fileType ? [value.fileType.label] : [],
        },
        {
          alias: "file",
          value: value.file,
        },
      ];

      this._canValidate = (value.file?.src?.length ?? 0) > 0;
    });
  }

  #onPropertyDataChange(e: Event) {
    const value = (e.target as UmbPropertyDatasetElement).value;

    const newValue = {} as any;
    value.forEach((item) => {
      if (!item.value) {
        return;
      }
      switch (item.alias) {
        case "domain":
          const domainId = this._domains.find(
            (d) => d.name === (item.value as string[])[0]
          )?.id;
          newValue["domain"] = domainId;
          break;

        case "fileType":
          const fileType = this._fileTypes.find(
            (f) => f.label === (item.value as string[])[0]
          );
          newValue["fileType"] = fileType;
          break;

        case "file":
          newValue["file"] = item.value as MediaValueType;
          break;
      }
    });
    newValue["isValid"] = false;
    newValue["notification"] = undefined;
    this.State?.update(newValue);
  }

  async #handleValidate() {
    const state = this.State.getValue();
    const result = await this.#redirectRepository.verifyImport(
      state.fileType!.label,
      state.file!.temporaryFileId!,
      state.domain
    );
    if (result.error) {
      this.State.update({
        notification:
          ((result.error as ApiError)?.body as string) ??
          "Something went wrong",
      });
      return;
    }
    this.State.update({
      notification:
        "Validated successfully! You can now import your redirects!",
      isValid: true,
    });
  }

  async #handleSubmit() {
    await this.#redirectRepository.submitImport();
    this.modalContext?.submit();
  }

  override render() {
    return html`
      <umb-body-layout headline="Import redirects">
        <uui-box>
          <umb-property-dataset
            .value=${this._content!}
            @change=${this.#onPropertyDataChange}
          >
            <umb-property
              alias="domain"
              label="Select the domain to import for"
              description="If nothing is selected, redirects will be active for all domains"
              property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
              val
              .config=${[
                {
                  alias: "items",
                  value: this._domains.map((item) => item.name),
                },
              ]}
            >
            </umb-property>
            <umb-property
              alias="fileType"
              label="Select the file type"
              description="Select the type of file you want to import the redirects from"
              property-editor-ui-alias="Umb.PropertyEditorUi.Dropdown"
              val
              required
              .config=${[
                {
                  alias: "items",
                  value: this._fileTypes.map((item) => item.label),
                },
              ]}
              .validation=${{
                mandatory: true,
                mandatoryMessage: "This field is required",
              }}
            >
            </umb-property>
            ${when(
              this.State.getValue().fileType,
              () => html`
                <umb-property
                  alias="file"
                  label="File to import"
                  property-editor-ui-alias="Umb.PropertyEditorUi.UploadField"
                  val
                  .config=${[
                    {
                      alias: "fileExtensions",
                      value: this.State.getValue().fileType!.extensions,
                    },
                  ]}
                  .validation=${{
                    mandatory: true,
                    mandatoryMessage: "This field is required",
                  }}
                >
                </umb-property>
              `
            )}
          </umb-property-dataset>
          ${when(
            this.State.getValue().notification,
            () => html`
              <div
                class="${this.State.getValue().isValid ? "success" : "danger"}"
              >
                <p>${this.State.getValue().notification}</p>
              </div>
            `
          )}
        </uui-box>
        <umb-workspace-footer slot="footer" data-mark="workspace:footer">
          <slot name="footer-info"></slot>
          <slot
            name="actions"
            slot="actions"
            data-mark="workspace:footer-actions"
          >
            ${when(
              this.State.getValue().isValid,
              () => html`
                <uui-button
                  slot="actions"
                  id="save"
                  label="Submit"
                  look="primary"
                  color="positive"
                  @click="${this.#handleSubmit}"
                  >Submit</uui-button
                >
              `,
              () => html`
                <uui-button
                  slot="actions"
                  id="save"
                  label="Validate"
                  look="primary"
                  color="positive"
                  .disabled=${!this._canValidate}
                  @click="${this.#handleValidate}"
                  >Validate</uui-button
                >
              `
            )}
          </slot>
        </umb-workspace-footer>
      </umb-body-layout>
    `;
  }

  static styles = [
    css`
      .danger {
        color: red;
      }
      .success {
        color: green;
      }
    `,
  ];
}
