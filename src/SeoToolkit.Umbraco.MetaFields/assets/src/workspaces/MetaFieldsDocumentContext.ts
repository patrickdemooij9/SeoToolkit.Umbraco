import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import {
  DocumentTypeSettingsContentViewModel,
  DocumentTypeValuePostViewModel,
} from "../api";
import { MetaFieldsSettingsRepository } from "../dataAccess/MetaFieldsSettingsRepository";
import { UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document-type";
import {
  UMB_ACTION_EVENT_CONTEXT,
  UmbActionEventContext,
} from "@umbraco-cms/backoffice/action";
import {
  groupSettingsFields,
  isFieldConfigured,
  METAFIELDS_VIEW_PATHNAME,
  SeoDocumentNavigation,
  ST_SEO_DOCUMENT_NAVIGATION_CONTEXT,
} from "./metaFieldsSettingsGroups";

export default class MetaFieldsDocumentContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.DocumentType";

  #repository: MetaFieldsSettingsRepository;
  #actionEventContext?: UmbActionEventContext;

  #model = new UmbObjectState<DocumentTypeSettingsContentViewModel>({});
  public readonly model = this.#model.asObservable();
  public readonly groups = this.#model.asObservablePart((model) =>
    groupSettingsFields(model.fields ?? [], model.groups ?? [])
  );

  #navigation?: SeoDocumentNavigation;

  #nodeId: string | undefined;

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_DOCUMENT_TOKEN_CONTEXT.toString());

    this.#repository = new MetaFieldsSettingsRepository(host);

    this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
      this.observe(instance?.unique, (unique) => {
        this.#nodeId = unique?.toString();
        this.fetchFromServer(unique!);
      });
    });

    this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
      if (this.#actionEventContext || !instance) {
        return;
      }

      this.#actionEventContext = instance;
      instance.addEventListener("seo-settings", this.#save);
    });

    this.consumeContext(ST_SEO_DOCUMENT_NAVIGATION_CONTEXT, (instance) => {
      this.#navigation = instance;
      this.#updateNavigation();
    });

    this.observe(this.groups, () => this.#updateNavigation());
  }

  activeGroupAlias() {
    return this.#navigation?.activeNavItem(METAFIELDS_VIEW_PATHNAME);
  }

  // Shows the field groups, with how many fields are configured, in the SEO side navigation.
  #updateNavigation() {
    const model = this.#model.getValue();
    const groups = groupSettingsFields(model.fields ?? [], model.groups ?? []);
    this.#navigation?.setNavItems(
      METAFIELDS_VIEW_PATHNAME,
      groups.map((group) => ({
        key: group.alias!,
        label: group.name!,
        icon: group.icon ?? undefined,
        badge: `${group.fields.filter(isFieldConfigured).length}/${group.fields.length}`,
      }))
    );
  }

  fetchFromServer(unique: string) {
    this.#repository.get(unique!).then((metaFieldSettings) => {
      this.#model.update({
        fields: metaFieldSettings?.data?.contentModel?.fields,
        groups: metaFieldSettings?.data?.contentModel?.groups,
        inheritance: metaFieldSettings?.data?.contentModel?.inheritance,
      });
    });
  }

  destroy(): void {
    super.destroy();
    this.#actionEventContext?.removeEventListener("seo-settings", this.#save);
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
