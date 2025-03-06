import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import {
  DocumentTypeValuePostViewModel,
  MetaFieldsSettingsViewModel,
} from "../api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { MetaFieldsContentRepository } from "../dataAccess/MetaFieldsContentRepository";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";
import { UmbRequestReloadStructureForEntityEvent } from "@umbraco-cms/backoffice/entity-action";

export default class MetaFieldsContentContext
  extends UmbContextBase<DocumentTypeValuePostViewModel>
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #repository: MetaFieldsContentRepository;
  #actionEventContext?: UmbActionEventContext;

  #nodeId?: string;
  #culture?: string;

  #model = new UmbObjectState<MetaFieldsSettingsViewModel>({});
  public readonly model = this.#model.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_CONTENT_TOKEN_CONTEXT.toString());

    this.#repository = new MetaFieldsContentRepository(host);

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      instance.splitView.activeVariantsInfo.subscribe((variants) => {
        if (variants.length > 0) {
          this.#culture = variants[0].culture!;
        }
      });
      instance.unique.subscribe((unique) => {
        this.#repository.get(unique!).then((resp) => {
          this.#model.update(resp.data!);
        });
        this.#nodeId = unique?.toString();
      });
    });

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      this.#actionEventContext = instance;
      instance.addEventListener("document.save", () => this.#save(this));

      // This is quite ugly, but the only way to do it. Should be able to be changed for v15
      instance.addEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, () => this.#save(this));
    });
  }

  destroy(): void {
    this.#actionEventContext?.removeEventListener("document.save", () =>
      this.#save(this)
    );
    this.#actionEventContext?.removeEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, () => this.#save(this));
  }

  #save(context: MetaFieldsContentContext) {
    context.save();
  }

  save() {
    const model = this.#model.getValue();
    const userValues: { [key: string]: unknown } = {};
    model.fields?.forEach((field) => {
      if (field.userValue) {
        userValues[field.alias!] = field.userValue;
      }
    });

    this.#repository.save({
      nodeId: this.#nodeId!,
      culture: this.#culture,
      userValues: userValues,
    });
  }

  updateField(alias: string, userValue: any) {
    const fields = [...this.#model.getValue().fields!];

    const foundField = fields.find((item) => item.alias === alias);
    if (!foundField) {
      return;
    }
    fields[fields.indexOf(foundField)] = {
      ...foundField,
      userValue: userValue,
    };
    this.#model.update({
      fields: fields,
    });
  }

  getEntityType(): string {
    return "st-metafield";
  }
}

export const ST_METAFIELDS_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<MetaFieldsContentContext>("ST-MetaFieldsContent-Context");
