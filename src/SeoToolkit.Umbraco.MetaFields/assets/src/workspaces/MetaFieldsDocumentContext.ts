import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import {
  DocumentTypeSettingsContentViewModel,
  DocumentTypeSettingsPostViewModel,
  DocumentTypeValuePostViewModel,
} from "../api";
import { MetaFieldsSettingsRepository } from "../dataAccess/MetaFieldsSettingsRepository";
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document-type";
import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";

export default class MetaFieldsDocumentContext
  extends UmbContextBase<DocumentTypeSettingsPostViewModel>
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.DocumentType";

  #repository: MetaFieldsSettingsRepository;
  #actionEventContext?: UmbActionEventContext;

  #model = new UmbObjectState<DocumentTypeSettingsContentViewModel>({});
  public readonly model = this.#model.asObservable();

  #nodeId: string | undefined;

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT.toString());

    this.#repository = new MetaFieldsSettingsRepository(host);

    this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
      instance.unique.subscribe((unique) => {
        this.#nodeId = unique?.toString();
        this.fetchFromServer(unique!);
      });
    });

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      if (this.#actionEventContext) {
        return;
      }

      this.#actionEventContext = instance;
      instance.addEventListener("document.save", this.#save);
    });
  }

  fetchFromServer(unique: string) {
    this.#repository.get(unique!).then((metaFieldSettings) => {
      this.#model.update({
        fields: metaFieldSettings.data?.contentModel?.fields,
        inheritance: metaFieldSettings.data?.contentModel?.inheritance,
      });
    });
  }

  destroy(): void {
    this.#actionEventContext?.removeEventListener("document.save", this.#save);
  }

  #save = () => {
    this.save();
  };

  save() {
    const model = this.#model.getValue();
    const fields: { [key: string]: DocumentTypeValuePostViewModel } = {};
    model.fields?.forEach((field) => {
      fields[field.alias!] = {
        useInheritedValue: field.useInheritedValue,
        value: field.value,
      };
    });

    this.#repository.save({
      nodeId: this.#nodeId!,
      enableSeoSettings: true,
      fields: fields,
      inheritanceId: model.inheritance?.id,
    });
  }

  update(model: Partial<DocumentTypeSettingsContentViewModel>) {
    this.#model.update(model);
  }

  updateField(alias: string, value: any) {
    const fields = [...this.#model.getValue().fields!];

    const foundField = fields.find((item) => item.alias === alias);
    if (!foundField) {
      return;
    }
    fields[fields.indexOf(foundField)] = {
      ...foundField,
      value: value,
    };
    this.#model.update({
      fields: fields,
    });
  }

  setInheritance(id?: string) {
    if (id) {
      this.#model.update({
        inheritance: {
          id: id,
        },
        fields: this.#model.getValue().fields?.map((item) => ({
          ...item,
          useInheritedValue: true,
        })),
      });
    } else {
      this.#model.update({
        inheritance: undefined,
        fields: this.#model.getValue().fields?.map((item) => ({
          ...item,
          useInheritedValue: false,
        })),
      });
    }
  }

  toggleInheritance(fieldAlias: string) {
    const fields = this.#model.getValue().fields?.map((field) => {
      if (field.alias == fieldAlias) {
        return {
          ...field,
          useInheritedValue: !field.useInheritedValue,
        };
      }
      return {
        ...field,
      };
    });

    this.#model.update({
      fields: fields,
    });
  }

  getEntityType(): string {
    return "st-metafield";
  }
}

export const ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT =
  new UmbContextToken<MetaFieldsDocumentContext>(
    "ST-MetaFieldsDocument-Context"
  );
