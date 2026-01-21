import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkit } from "../api";

export default class SeoToolkitKeyValueSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getSettings(domainId?: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.getUmbracoSeoToolkitSeoKeyValueSettings({
        query: {
          domainId,
        },
      })
    );
  }

  async saveSettings(values: { [key: string]: string }, domainId?: string) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.postUmbracoSeoToolkitSeoKeyValueSettingsSave({
        body: values,
        query: {
          domainId,
        },
      })
    );
  }
}
