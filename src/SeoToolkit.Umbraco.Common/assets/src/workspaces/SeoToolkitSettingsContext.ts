import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import {
  UMB_WORKSPACE_CONTEXT,
  UmbRoutableWorkspaceContext,
  UmbWorkspaceContext,
  UmbWorkspaceRouteManager,
} from "@umbraco-cms/backoffice/workspace";
import { SEOTOOLKIT_SETTINGS_ENTITY } from "../constants/seoToolkitConstants";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import SeoToolkitSettingsViewElement from "./SeoToolkitSettingsView.element";
import SeoToolkitKeyValueSource from "../sources/SeoToolkitKeyValueSource";
import { UmbArrayState } from "@umbraco-cms/backoffice/observable-api";
import { SeoKeyValueSettingViewModel } from "../api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export default class SeoToolkitSettingsContext
  extends UmbControllerBase
  implements UmbWorkspaceContext, UmbRoutableWorkspaceContext
{
  #source = new SeoToolkitKeyValueSource(this);

  workspaceAlias = "seoToolkit.domain.settings.workspace";
  routes = new UmbWorkspaceRouteManager(this);

  #settings = new UmbArrayState<SeoKeyValueSettingViewModel>(
    [],
    (item) => item.key,
  );
  public readonly settings = this.#settings.asObservable();

  #inheritedKeys = new UmbArrayState<string>([], (item) => item);
  public readonly inheritedKeys = this.#inheritedKeys.asObservable();

  #domainId?: string;

  constructor(host: UmbControllerHost) {
    super(host);

    this.provideContext(ST_SETTINGS_MODULE_TOKEN_CONTEXT, this);
    this.provideContext(UMB_WORKSPACE_CONTEXT, this);

    this.routes.setRoutes([
      {
        path: "edit/:unique",
        component: SeoToolkitSettingsViewElement,
        setup: (_component, info) => {
          this.#domainId = undefined;
          // This is a bit ugly, but the tree system is so difficult to understand
          if (info.match.params.unique.includes("~")) {
            const parts = info.match.params.unique.split("~");
            if (parts.length === 2) {
              this.#domainId = parts[1];
            }
          }
          this.load();
        },
      },
    ]);
  }

  load() {
    this.#source.getSettings(this.#domainId).then((result) => {
      this.#settings.setValue(result.data);
      this.#inheritedKeys.setValue(
        result.data
          .filter((item) => !item.isRoot && !item.value)
          .map((item) => item.key),
      );
    });
  }

  async save() {
    const postValue: { [key: string]: string } = {};
    this.#settings.value.forEach((item) => {
      postValue[item.key] = this.#inheritedKeys.value.includes(item.key) ? "" : ((String(item.value ?? "")) ?? "");
    });
    await this.#source.saveSettings(postValue, this.#domainId);

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      instance?.peek("positive", {
        data: {
          headline: "Saved",
          message: "Settings successfully saved!",
        },
      });
    });
  }

  updateValues(values: { [key: string]: string }) {
    const newSettings = structuredClone(this.#settings.value);
    Object.entries(values).forEach((item) => {
      const setting = newSettings.find((s) => s.key == item[0]);
      if (!setting) return;

      setting.value = item[1];
    });
    this.#settings.setValue(newSettings);
  }

  updateValue(key: string, value: string) {
    const newSettings = structuredClone(this.#settings.value);
    newSettings.find((item) => item.key == key)!.value = value;
    this.#settings.setValue(newSettings);
  }

  toggleInheritance(key: string) {
    const inheritedKeys = structuredClone(this.#inheritedKeys.value);
    if (inheritedKeys.includes(key)) {
      inheritedKeys.splice(inheritedKeys.indexOf(key), 1);
    } else {
      inheritedKeys.push(key);
    }
    this.#inheritedKeys.setValue(inheritedKeys);
  }

  getEntityType(): string {
    return SEOTOOLKIT_SETTINGS_ENTITY;
  }
}

export const ST_SETTINGS_MODULE_TOKEN_CONTEXT =
  new UmbContextToken<SeoToolkitSettingsContext>("settingsModuleContext");
