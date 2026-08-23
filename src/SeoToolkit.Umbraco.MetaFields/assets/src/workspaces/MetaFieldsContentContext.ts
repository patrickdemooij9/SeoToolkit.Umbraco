import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { MetaFieldsSettingsViewModel } from "../api";
import { UmbWorkspaceContext } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { MetaFieldsContentRepository } from "../dataAccess/MetaFieldsContentRepository";
import { MetaFieldsAISource } from "../dataAccess/MetaFieldsAISource";
import type { MetaFieldsAIFieldSuggestion } from "../dataAccess/MetaFieldsAISource";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { UmbBooleanState, UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

interface MetaFieldsSettingsVariant {
  variant: string;
  model: UmbObjectState<MetaFieldsSettingsViewModel>;
  isDirty: boolean;
  editVersion: number;
  saving: boolean;
  saveQueued: boolean;
}

export default class MetaFieldsContentContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias: string = "Umb.Workspace.Document";

  #repository: MetaFieldsContentRepository;
  #aiSource: MetaFieldsAISource;

  #nodeId?: string;
  #cultures: string[] = [];

  #variants: { [key: string]: MetaFieldsSettingsVariant } = {};

  #isAIAvailable = new UmbBooleanState(false);
  readonly isAIAvailable = this.#isAIAvailable.asObservable();

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_CONTENT_TOKEN_CONTEXT.toString());

    this.#repository = new MetaFieldsContentRepository(host);
    this.#aiSource = new MetaFieldsAISource(host);

    this.#checkAIAvailability();

    this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
      this.#nodeId = instance?.getUnique()?.toString();

      this.observe(instance?.splitView.activeVariantsInfo, (variants) => {
        variants?.forEach((variant) => {
          const culture = variant.culture ?? "invariant";
          if (!this.#cultures.includes(culture)) {
            this.#cultures.push(culture);
          }
        });
        this.#loadDataFromRepository(this.#nodeId);
      });
      this.observe(instance?.unique, (unique) => {
        Object.keys(this.#variants).forEach((culture) => {
          delete this.#variants[culture];
        });
        this.#loadDataFromRepository(unique?.toString());
      });
      this.observe(instance?.data, (item) => {
        if (item?.isTrashed) return;
        
        item?.variants.forEach((variant) => {
          const culture = variant.culture ?? "invariant";
          const currentDate = this.#getVariant(culture).lastUpdated;
          if (currentDate && currentDate !== variant.updateDate) {
            this.save(culture);
          });
        },
        "stMetaFieldsData"
      );
    });
  }

  #resetVariants() {
    this.#lastUpdated = {};

    // Reset in place. MetaFieldsContentView observes the state object handed out by
    // getModel(), so replacing it would leave the view bound to a dead observable.
    Object.values(this.#variants).forEach((variant) => {
      variant.model.setValue({ seoEnabled: false });
      variant.isDirty = false;
      // Invalidate any in-flight save so its response cannot be applied on top
      // of the node we just switched to.
      variant.editVersion++;
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
        const data = resp.data;
        if (data.seoEnabled === false) return;

        this.#getVariant(variant).model.update(resp.data);
      });
    });
  }

  #getVariant(variant: string) {
    if (this.#variants[variant]) {
      return this.#variants[variant];
    }
    this.#variants[variant] = {
      variant,
      model: new UmbObjectState<MetaFieldsSettingsViewModel>({
        seoEnabled: false,
      }),
      isDirty: false,
      editVersion: 0,
      saving: false,
      saveQueued: false,
    };
    return this.#variants[variant];
  }

  getModel(variant: string) {
    return this.#getVariant(variant).model.asObservable();
  }

  async save(culture: string) {
    const variant = this.#variants[culture];
    if (!variant || !this.#nodeId) return;

    if (variant.saving) {
      variant.saveQueued = true;
      return;
    }

    variant.saving = true;
    try {
      await this.#performSave(variant, culture);
    } finally {
      variant.saving = false;
    }

    if (variant.saveQueued) {
      variant.saveQueued = false;
      // Only worth another request when something is still unsaved — either
      // edits that arrived mid-flight or a save attempt that failed.
      if (variant.isDirty) {
        await this.save(culture);
      }
    }
  }

  async #performSave(variant: MetaFieldsSettingsVariant, culture: string) {
    const nodeId = this.#nodeId;
    if (!nodeId) return;

    const model = variant.model.getValue();
    if (model.seoEnabled === false) return;

    const userValues: { [key: string]: unknown } = {};
    model.fields?.forEach((field) => {
      if (field.userValue) {
        userValues[field.alias!] = field.userValue;
      }
    });

    const editVersionAtSave = variant.editVersion;
    const resp = await this.#repository.save({
      nodeId: nodeId,
      culture: culture,
      userValues: userValues,
    });

    // tryExecute never throws; a failed request comes back as an error without
    // data. Leave the variant dirty so the next document save retries, and tell
    // the editor — the document itself saved fine, so nothing else will. When a
    // follow-up save is already queued, let that attempt decide instead of
    // showing an error for a state that may recover on its own.
    if (!resp || resp.error || !resp.data) {
      if (variant.saveQueued) return;
      this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
        instance?.peek("danger", {
          data: {
            headline: "SEO",
            message:
              "The SEO meta fields could not be saved. Your changes are still here — save the page again to retry.",
          },
        });
      });
      return;
    }

    // Edits made while the request was in flight are not in the response;
    // applying it would wipe them. Keep the variant dirty so the next document
    // save picks them up.
    if (variant.editVersion !== editVersionAtSave) return;

    // Reconcile with what was actually persisted, otherwise this model keeps
    // serving the values it was first loaded with for the rest of the session.
    const data = resp.data;
    if (data.seoEnabled !== false) {
      variant.model.update(data);
    }
    variant.isDirty = false;
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
    entry.isDirty = true;
    entry.editVersion++;
    entry.model.update({
      fields: updated,
    });
  }

  applyAISuggestions(culture: string, suggestions: MetaFieldsAIFieldSuggestion[]) {
    const model = this.#variants[culture]?.model;
    if (!model) return;

    const fields = [...model.getValue().fields!];
    for (const suggestion of suggestions) {
      const foundField = fields.find((item) => item.alias === suggestion.alias);
      if (foundField) {
        fields[fields.indexOf(foundField)] = {
          ...foundField,
          userValue: suggestion.value,
        };
      }
    }
    entry.isDirty = true;
    entry.editVersion++;
    entry.model.update({ fields: updated });
  }

  async #checkAIAvailability() {
    const { data } = await this.#aiSource.isAvailable();
    this.#isAIAvailable.setValue(data === true);
  }

  async generateSuggestions(culture: string): Promise<MetaFieldsAIFieldSuggestion[]> {
    if (!this.#nodeId) return [];
    const { data } = await this.#aiSource.generate(this.#nodeId, culture);
    return data?.suggestions ?? [];
  }

  getEntityType(): string {
    return "st-metafield";
  }
}

export const ST_METAFIELDS_CONTENT_TOKEN_CONTEXT =
  new UmbContextToken<MetaFieldsContentContext>("ST-MetaFieldsContent-Context");