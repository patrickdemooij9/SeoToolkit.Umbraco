import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbWorkspaceViewElement } from "@umbraco-cms/backoffice/workspace";
import SeoToolkitDomainContext, {
  ST_DOMAIN_DETAIL_TOKEN_CONTEXT,
} from "./SeoToolkitDomainContext";
import { css, html } from "lit";
import {
  customElement,
  repeat,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { SeoDomainCollection, SeoDomainConfigViewModel } from "../api";

interface CheckboxItem {
  label: string;
  value: string | undefined;
  checked: boolean;
}

type UmbInputCheckboxListElement = {
  selection: string[];
};

@customElement("seotoolkit-domain-edit-view")
export class SeoToolkitDomainEditViewElement
  extends UmbLitElement
  implements UmbWorkspaceViewElement
{
  #context?: SeoToolkitDomainContext;

  @state()
  model?: SeoDomainCollection;

  @state()
  config?: SeoDomainConfigViewModel;

  @state()
  domainList: CheckboxItem[] = [];

  constructor() {
    super();

    this.consumeContext(ST_DOMAIN_DETAIL_TOKEN_CONTEXT, (instance) => {
      if (!instance) {
        return;
      }
      this.#context = instance;

      this.observe(this.#context.domain, (value) => {
        this.model = value;
        this.loadList();
      });

      this.observe(this.#context?.config, (config) => {
        this.config = config;
        this.loadList();
      });
    });
  }

  loadList() {
    this.domainList = (this.config?.domains ?? []).map((domain) => ({
      label: domain.domainName ?? "",
      value: domain.id?.toString(),
      checked: this.model?.domainIds.includes(domain.id ?? 0) ?? false,
    }));
  }

  #onDomainChange(
    event: CustomEvent & { target: UmbInputCheckboxListElement }
  ) {
    let newValue = event.target.selection.map((it) => Number.parseInt(it));
    this.#context?.updateDomain({ domainIds: newValue });
  }

  getModuleEnabled(module: string): boolean {
    return Object.keys(this.model?.settings ?? {}).includes(module);
  }

  toggleModule(module: string) {
    let newSettings = { ...(this.model?.settings ?? {}) };
    if (this.getModuleEnabled(module)) {
      delete newSettings[module];
    } else {
      newSettings[module] = 'true';
    }
    this.#context?.updateDomain({ settings: newSettings });
  }

  override render() {
    return html`
      <div id="domain-edit">
        <uui-box>
          <umb-property-layout
            label="Domains"
            description="All umbraco domains that are grouped in this SEO domain."
          >
            <div slot="editor">
              <umb-input-checkbox-list
                .list=${this.domainList}
                .selection=${this.model?.domainIds.map((item) => item.toString()) ?? []}
                @change=${this.#onDomainChange}
              ></umb-input-checkbox-list></div
          ></umb-property-layout>
          <h3>Domain modules</h3>
          <p>
            Here you can configure modules for this domain. If disabled, it will
            fallback to the global module.
          </p>
          <div class="module-items">
            ${repeat(
              this.config?.moduleSettings ?? [],
              (item) => item.id,
              (item) =>
                html`
                  <div class="module-item" @click=${() => this.toggleModule("Module." + item.id)}>
                    <h4>${item.name}</h4>
                    <input
                      type="checkbox"
                      .checked=${this.getModuleEnabled("Module." + item.id)}
                    />
                  </div>
                `
            )}
          </div>
        </uui-box>
      </div>
    `;
  }

  static styles = [
    css`
      #domain-edit {
        padding: var(--uui-size-layout-1);
      }

      .module-items {
        display: flex;
        gap: 12px;
      }

      .module-item {
        padding: 12px 6px;
        border: 1px solid var(--uui-color-border);
        border-radius: 4px;
        flex: 1;
        text-align: center;

        cursor: pointer;

        > h4 {
            margin: 0;
        }
      }
    `,
  ];
}
