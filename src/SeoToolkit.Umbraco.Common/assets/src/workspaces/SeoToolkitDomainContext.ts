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
import { UMB_ACTION_EVENT_CONTEXT } from "@umbraco-cms/backoffice/action";
import { UmbRequestReloadChildrenOfEntityEvent } from "@umbraco-cms/backoffice/entity-action";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

export default class SeoToolkitDomainContext
  extends UmbContextBase
  implements UmbWorkspaceContext {
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
        setup: () => {
          this.#domain.setValue({
            id: 0,
            name: '',
            domainIds: [],
            settings: {}
          })
        }
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
            } else if (parts.length === 3) {
              this.loadUmbracoDomain(Number.parseInt(parts[2]));
            }
          }
        },
      },
    ]);
  }

  loadUmbracoDomain(domainId: number) {
    this.repository.getPredefined(domainId).then((resp) => {
      this.#domain.setValue(resp.data);
    })
  }

  load(domainId: number) {
    this.repository.get(domainId).then((resp) => {
      this.#domain.setValue(resp.data);
    });
  }

  loadConfig() {
    this.repository.getConfig().then(resp => {
      this.#config.setValue(resp.data);
    });
  }

  async save() {
    const isNew = this.#domain.getValue().id === 0;
    const id = (await new SeoToolkitDomainRepository(this).saveDomain(this.#domain.getValue())).data;
    this.updateDomain({
      id: id,
    });

    const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
    if (!actionEventContext) throw new Error('Action Event Context is not available');

    actionEventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent({
      unique: 'ab248b43-9757-432a-9821-22f9eeb513e7',
      entityType: 'seoToolkit-domain-root',
    }));
    actionEventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent({
      unique: 'ab248b43-9757-432a-9821-22f9eeb513e7~' + id,
      entityType: 'seoToolkit-domain',
    }));

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      instance?.peek('positive', {
        data: {
          headline: 'Saved',
          message: 'Domain successfully saved!'
        }
      })
    });
    if (isNew) {
      if (location.href.includes('create')) {
        history.replaceState(null, '', location.href.replace("create", "edit/ab248b43-9757-432a-9821-22f9eeb513e7~" + id));
      } else {
        var lastSegmentStart = location.href.lastIndexOf('/');
        history.replaceState(null, '', location.href.substring(0, lastSegmentStart) + '/ab248b43-9757-432a-9821-22f9eeb513e7~' + id);
      }
    }
  }

  async delete() {
    await new SeoToolkitDomainRepository(this).delete(this.#domain.getValue().id);

    const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
    if (!actionEventContext) throw new Error('Action Event Context is not available');

    actionEventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent({
      unique: 'ab248b43-9757-432a-9821-22f9eeb513e7',
      entityType: 'seoToolkit-domain-root',
    }));

    history.replaceState(null, '', '/umbraco/section/SeoToolkit/workspace/seoToolkit-domain-root/edit/ab248b43-9757-432a-9821-22f9eeb513e7');
  }

  updateDomain(domain: Partial<SeoDomainCollection>) {
    this.#domain.update(domain);
  }
}

export const ST_DOMAIN_DETAIL_TOKEN_CONTEXT =
  new UmbContextToken<SeoToolkitDomainContext>("seoToolkitDomainDetailContext");
