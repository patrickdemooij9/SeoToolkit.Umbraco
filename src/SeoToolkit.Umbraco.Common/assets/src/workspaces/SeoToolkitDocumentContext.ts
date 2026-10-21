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
import {
  SeoDocumentNavigation,
  SeoDocumentNavItem,
  ST_SEO_DOCUMENT_NAVIGATION_CONTEXT,
} from "./seoDocumentNavigation";

export default class SeoToolkitDocumentContext
  extends UmbContextBase
  implements UmbWorkspaceContext, SeoDocumentNavigation
{
  workspaceAlias: string = "Umb.Workspace.DocumentType";

  #actionEventContext?: UmbActionEventContext;
  #settingsRepository = new SeoToolkitSettingsRepository(this);
  #isElementType = false;

  #model = new UmbObjectState<SeoSettingsPostModel>({
    contentTypeId: "",
    enabled: false,
  });
  public readonly model = this.#model.asObservable();

  #navItems = new UmbObjectState<Record<string, SeoDocumentNavItem[]>>({});
  public readonly navItems = this.#navItems.asObservable();

  #activeNavItems = new UmbObjectState<Record<string, string | undefined>>({});

  constructor(host: UmbControllerHost) {
    super(host, ST_METAFIELDS_SETTINGSDOCUMENT_TOKEN_CONTEXT.toString());
    this.provideContext(ST_SEO_DOCUMENT_NAVIGATION_CONTEXT, this);

    this.consumeContext(UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
      this.observe(instance?.isElement, (value) => {
        this.#isElementType = value ?? false;
      });
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
    if (this.#isElementType) return;

    const value = { ...this.#model.getValue() };
    if (value.contentTypeId === "") return;

    // Send out an event for each view to implement their own save method
    this.#actionEventContext?.dispatchEvent(new CustomEvent("seo-settings", {
      detail: {
        contentTypeId: value.contentTypeId
      }
    }));

    this.#settingsRepository.setSettings(value);
  }

  public setSeoSettings(value: boolean) {
    this.#model.update({
      enabled: value,
    });
  }

  setNavItems(viewPathname: string, items: SeoDocumentNavItem[]) {
    this.#navItems.update({ [viewPathname]: items });
  }

  activeNavItem(viewPathname: string) {
    return this.#activeNavItems.asObservablePart((items) => items[viewPathname]);
  }

  setActiveNavItem(viewPathname: string, key: string | undefined) {
    this.#activeNavItems.update({ [viewPathname]: key });
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
