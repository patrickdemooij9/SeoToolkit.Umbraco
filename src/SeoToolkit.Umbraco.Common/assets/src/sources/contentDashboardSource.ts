import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { UserDataService } from "@umbraco-cms/backoffice/external/backend-api";

const LAYOUT_GROUP = "SeoToolkit";
const LAYOUT_IDENTIFIER = "contentDashboardLayout";

// The layout is stored per user in Umbraco's user data.
export class ContentDashboardSource {
  #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getLayout() {
    return await tryExecute(
      this.#host,
      UserDataService.getUserData({ query: { groups: [LAYOUT_GROUP], identifiers: [LAYOUT_IDENTIFIER] } })
    );
  }

  async createLayout(key: string, value: string) {
    return await tryExecute(
      this.#host,
      UserDataService.postUserData({ body: { group: LAYOUT_GROUP, identifier: LAYOUT_IDENTIFIER, value, key } })
    );
  }

  async updateLayout(key: string, value: string) {
    return await tryExecute(
      this.#host,
      UserDataService.putUserData({ body: { group: LAYOUT_GROUP, identifier: LAYOUT_IDENTIFIER, value, key } })
    );
  }

  async deleteLayout(key: string) {
    return await tryExecute(this.#host, UserDataService.deleteUserDataById({ path: { id: key } }));
  }
}
