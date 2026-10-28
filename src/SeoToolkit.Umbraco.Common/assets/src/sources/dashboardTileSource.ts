import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { BackofficeSeoToolkit } from "../api";

export class DashboardTileSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  // Without error notifications: a tile that can't load shows an error state instead.
  async getData(dataSource: string, parameters?: Record<string, string>) {
    return await tryExecute(
      this.#host,
      BackofficeSeoToolkit.postUmbracoSeoToolkitDashboardData({ body: { dataSource, parameters } }),
      { disableNotifications: true }
    );
  }
}
