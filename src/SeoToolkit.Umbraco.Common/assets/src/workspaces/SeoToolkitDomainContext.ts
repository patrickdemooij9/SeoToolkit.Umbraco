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
import { SeoDomainCollection } from "../api";
import { SeoToolkitDomainRepository } from "../repositories/SeoToolkitDomainRepository";

export default class SeoToolkitDomainContext
  extends UmbContextBase
  implements UmbWorkspaceContext
{
  workspaceAlias = "seoToolkit.domain.detail";

  routes = new UmbWorkspaceRouteManager(this);

  #domain = new UmbObjectState<SeoDomainCollection>({
    id: 0,
    name: "",
    domainIds: [],
    settings: {},
  });
  public readonly domain = this.#domain.asObservable();

  getEntityType(): string {
    return "st-domain";
  }

  constructor(host: UmbControllerHost) {
    super(host, UMB_WORKSPACE_CONTEXT.toString());
    this.provideContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT, this);

    this.routes.setRoutes([
      {
        path: "create",
        component: SeoToolkitDomainViewElement,
      },
      {
        path: "edit/:unique",
        component: SeoToolkitDomainViewElement,
        setup: (_component, info) => {
          this.load(Number.parseInt(info.match.params.unique));
        },
      },
    ]);
  }

  load(domainId?: number) {
    console.log("Load domain with id", domainId);
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
