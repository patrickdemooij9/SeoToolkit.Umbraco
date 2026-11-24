import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { DashboardTileSource } from "../sources/dashboardTileSource";

export class DashboardTileRepository extends UmbControllerBase {
  #source: DashboardTileSource;

  constructor(host: UmbControllerHost) {
    super(host);
    this.#source = new DashboardTileSource(this);
  }

  async getData(dataSource: string, parameters?: Record<string, string>) {
    return this.#source.getData(dataSource, parameters);
  }
}
