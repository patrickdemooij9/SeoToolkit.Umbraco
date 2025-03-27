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

interface MetaFieldsSettingsVariant {
  variant: string;
  model: UmbObjectState<MetaFieldsSettingsViewModel>;
  lastUpdated?: string | null;
}

export default class MetaFieldsContentContext
  extends UmbContextBase<DocumentTypeValuePostViewModel>
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #repository: MetaFieldsContentRepository;

  #nodeId?: string;
  #cultures: string[] = [];

  #variants: { [key: string]: MetaFieldsSettingsVariant } =
    {};

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_CONTENT_TOKEN_CONTEXT.toString());

    this.#repository = new MetaFieldsContentRepository(host);

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      instance.splitView.activeVariantsInfo.subscribe((variants) => {
        variants.forEach((variant) => {
          const culture = variant.culture ?? "invariant";
          if (!this.#cultures.includes(culture)) {
            this.#cultures.push(culture);
          }
        });
        this.#loadDataFromRepository(this.#nodeId);
      });
      instance.unique.subscribe((unique) => {
        this.#cultures.forEach((culture) => {
          delete this.#variants[culture];
        })
        this.#loadDataFromRepository(unique?.toString());
      });
      instance.data.subscribe((item) => {
        item?.variants.forEach((variant) => {
          const culture = variant.culture ?? 'invariant';
          const currentDate = this.#getVariant(culture).lastUpdated;
          if (currentDate && currentDate !== variant.updateDate) {
            this.save(culture);
          }

          this.#getVariant(culture).lastUpdated = variant.updateDate;
        })
      });
    });
  }

  #loadDataFromRepository(node: string | undefined) {
    this.#nodeId = node;

    this.#cultures.forEach((variant) => {
      if (
        this.#variants[variant] &&
        this.#variants[variant].model.getValue().fields
      ) {
        return;
      }

      this.#repository.get(node!, variant).then((resp) => {
        this.#getVariant(variant).model.update(resp.data!);
      });
    });
  }

  #getVariant(variant: string) {
    if (this.#variants[variant]) {
      return this.#variants[variant];
    }
    this.#variants[variant] = {
      variant,
      model: new UmbObjectState<MetaFieldsSettingsViewModel>({})
    }
    return this.#variants[variant];
  }

  getModel(variant: string) {
    return this.#getVariant(variant).model.asObservable();
  }

  save(culture: string) {
    const model = this.#variants[culture]!.model.getValue();
    const userValues: { [key: string]: unknown } = {};
    model.fields?.forEach((field) => {
      if (field.userValue) {
        userValues[field.alias!] = field.userValue;
      }
    });

    this.#repository.save({
      nodeId: this.#nodeId!,
      culture: culture,
      userValues: userValues,
    });
  }

  updateField(variant: string, alias: string, userValue: any) {
    const model = this.#variants[variant].model;
    const fields = [...model.getValue().fields!];

    const foundField = fields.find((item) => item.alias === alias);
    if (!foundField) {
      return;
    }
    fields[fields.indexOf(foundField)] = {
      ...foundField,
      userValue: userValue,
    };
    model.update({
      fields: fields,
    });
  }

  getEntityType(): string {
    return "st-metafield";
  }
}

export const ST_METAFIELDS_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<MetaFieldsContentContext>("ST-MetaFieldsContent-Context");
