import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";
import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document-type";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { SeoSettingsPostModel } from "../api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { SeoToolkitSettingsRepository } from "../repositories/seoToolkitSettingsRepository";
import { UmbEntityUpdatedEvent } from "@umbraco-cms/backoffice/entity-action";

export default class SeoToolkitDocumentContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.DocumentType";

  #actionEventContext?: UmbActionEventContext;
  #settingsRepository = new SeoToolkitSettingsRepository(this);

  #model = new UmbObjectState<SeoSettingsPostModel>({
    contentTypeId: "",
    enabled: false,
  });
  public readonly model = this.#model.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_SETTINGSDOCUMENT_TOKEN_CONTEXT.toString());

    this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
      this.observe(instance?.unique, (value) => {
        this.#settingsRepository.getSettings(value!).then((resp) => {
          this.#model.update({
            contentTypeId: value?.toString(),
            enabled: resp.data.isEnabled,
          });
        });
      });
    });

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      if (this.#actionEventContext || !instance) {
        return;
      }

      this.#actionEventContext = instance;
      this.#actionEventContext.addEventListener(
        UmbEntityUpdatedEvent.TYPE,
        this.#save
      );
    });
  }

  #save = () => {
    this.save();
  }

  public save() {
    const value = { ...this.#model.getValue() };
    if (value.contentTypeId === "") return;

    this.#settingsRepository.setSettings(value);
  }

  public setSeoSettings(value: boolean) {
    this.#model.update({
      enabled: value,
    });
  }

  getEntityType(): string {
    return "st-metafield";
  }

  destroy(): void {
    super.destroy();
    this.#actionEventContext?.removeEventListener(UmbEntityUpdatedEvent.TYPE, this.#save);
  }
}

export const ST_METAFIELDS_SETTINGSDOCUMENT_TOKEN_CONTEXT =
  new UmbContextToken<SeoToolkitDocumentContext>(
    "ST-MetaFieldsSettingsDocument-Context"
  );
