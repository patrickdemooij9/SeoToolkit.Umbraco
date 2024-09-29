import { OpenAPI as t } from "@umbraco-cms/backoffice/external/backend-api";
import { UMB_AUTH_CONTEXT as i } from "@umbraco-cms/backoffice/auth";
const T = (e, r) => {
  e.consumeContext(i, (o) => {
    const n = o.getOpenApiConfiguration();
    t.BASE = n.base, t.WITH_CREDENTIALS = n.withCredentials, t.CREDENTIALS = n.credentials, t.TOKEN = n.token;
  });
};
export {
  T as onInit
};
//# sourceMappingURL=robotstxt.js.map
