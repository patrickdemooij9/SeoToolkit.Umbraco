import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { customElement } from "@umbraco-cms/backoffice/external/lit";
import { html, LitElement } from "lit";

@customElement("st-siteaudit-content-view")
export default class SiteAuditContentViewElement extends UmbElementMixin(LitElement){
    render(){
        return html`
        <h1>Hello world</h1>
        `    
    }
}