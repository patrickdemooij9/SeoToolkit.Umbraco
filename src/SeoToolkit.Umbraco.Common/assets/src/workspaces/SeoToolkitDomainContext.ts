import { UmbContextBase } from "@umbraco-cms/backoffice/class-api";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UMB_WORKSPACE_CONTEXT,
  UmbWorkspaceContext,
  UmbWorkspaceRouteManager,
} from "@umbraco-cms/backoffice/workspace";
import SeoToolkitDomainViewElement from "./SeoToolkitDomainView.element";
import { UmbObjectState } from "@umbraco-cms/backoffice/observable-api";
import { SeoDomainCollection, SeoDomainConfigViewModel } from "../api";
import { SeoToolkitDomainRepository } from "../repositories/SeoToolkitDomainRepository";

export default class SeoToolkitDomainContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias = "seoToolkit.domain.detail";

  repository = new SeoToolkitDomainRepository(this);
  routes = new UmbWorkspaceRouteManager(this);

  #domain = new UmbObjectState<SeoDomainCollection>({
    id: 0,
    name: "",
    domainIds: [],
    settings: {},
  });
  public readonly domain = this.#domain.asObservable();

  #config = new UmbObjectState<SeoDomainConfigViewModel>({
    domains: [],
    moduleSettings: []
  });
  public readonly config = this.#config.asObservable();

  getEntityType(): string {
    return "seoToolkit-domain";
  }

  constructor(host: UmbControllerHost) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());
    this.provideContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT, this);

    this.loadConfig();
    this.routes.setRoutes([
      {
        path: "create",
        component: SeoToolkitDomainViewElement,
      },
      {
        path: "edit/:unique",
        component: SeoToolkitDomainViewElement,
        setup: (_component, info) => {
          // This is a bit ugly, but the tree system is so difficult to understand
          if (info.match.params.unique.includes("~")) {
            const parts = info.match.params.unique.split("~");
            if (parts.length === 2) {
              this.load(Number.parseInt(parts[1]));
            }
          }
        },
      },
    ]);
  }

  load(domainId: number) {
    this.repository.get(domainId).then((resp) => {
      this.#domain.setValue(resp.data);
    });
  }

  loadConfig(){
    this.repository.getConfig().then(resp => {
      this.#config.setValue(resp.data);
    });
  }

  save() {
    new SeoToolkitDomainRepository(this).saveDomain(this.#domain.getValue());
  }

  updateDomain(domain: Partial<SeoDomainCollection>) {
    this.#domain.update(domain);
  }
}

export const ST_DOMAIN_DETAIL_TOKEN_CONTEXT =
  new UmbContextToken<SeoToolkitDomainContext>("seoToolkitDomainDetailContext");
