import { LitElement, css, html, customElement, state, when, repeat } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { SeoToolkitModule } from "../../api";
import { ModuleRepository } from "../../repositories/moduleRepository";

@customElement('welcome-dashboard')
export class MyWelcomeDashboardElement extends UmbElementMixin(LitElement) {

  @state()
  modules?: SeoToolkitModule[];

  connectedCallback(): void {
    super.connectedCallback();

    new ModuleRepository(this).getModules().then((resp) => {
      this.modules = resp.data;
      console.log(this.modules);
    });
  }

  render() {
    return html`
      <div class="welcomeDashboard">
        <h1>Welcome!</h1>
        <div class="intro">
            <p>This is the dashboard for SeoToolkit. The SEO package for Umbraco.</p>
            <p>Here you can see what modules are installed. Each functionality is shipped in its own package. So you can mix and match to your liking!</p>
        </div>
        <div class="modules">
          ${when(this.modules, () => html`
      
        ${repeat(this.modules!, (item) => item.alias, (item) =>
      html`
                <a href="#" href="${item.link}" class="module" target="_blank">
                    <div class="module-icon">
                        <umb-icon name="${item.icon}"></umb-icon>
                    </div>
                    <p class="module-title">${item.title}</p>
            ${when(item.status === 'Disabled', () => html`<p class="module-status module-status-disabled">Disabled</p>`)}
            ${when(item.status === 'Installed', () => html`<p class="module-status module-status-installed">Installed</p>`)}
            ${when(item.status === 'NotInstalled', () => html`<p class="module-status">Not installed</p>`)}
                </a>`
    )}
      `
    )}
            
        </div>
      </div>
    `;
  }

  static styles = [
    css`
      .welcomeDashboard h1 {
    text-align: center;
}

.welcomeDashboard .intro {
    text-align: center;
}

.welcomeDashboard .modules {
    display: flex;
}

    .welcomeDashboard .modules .module {
        padding: 10px;
        margin: 10px;
        background-color: white;
        border-radius: 3px;
        width: 20%;
        color: black;
        text-decoration: none;
    }

.welcomeDashboard .module-icon {
    text-align: center;
    padding-bottom: 10px;
    padding-top: 10px;
}

.welcomeDashboard .module-title {
    text-align: center;
    font-weight: 600;
}

.welcomeDashboard .module-status {
    text-align: center;
    color: red;
}

.welcomeDashboard .module-status-installed {
    color: green;
}

.welcomeDashboard .module-status-disabled {
    color: #f0ac00;
}
    `,
  ];
}

export default MyWelcomeDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    'welcome-dashboard': MyWelcomeDashboardElement;
  }
}