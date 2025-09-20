import { UmbEntityActionBase } from "@umbraco-cms/backoffice/entity-action";

export class CreateDomainTreeAction extends UmbEntityActionBase<never> {
  async getHref() {
    return "section/SeoToolkit/workspace/st-domain/create";
  }
}
