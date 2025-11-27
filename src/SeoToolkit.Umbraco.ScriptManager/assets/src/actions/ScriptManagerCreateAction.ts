import { UmbWorkspaceActionBase } from "@umbraco-cms/backoffice/workspace";
import ScriptManagerModuleContext from "../workspaces/ScriptManagerModuleContext";

export class ScriptManagerCreateAction extends UmbWorkspaceActionBase<ScriptManagerModuleContext> {
  override async execute() {
    let url = `/umbraco/section/SeoToolkit/workspace/st-script/create`;

    const lastSegment = window.location.href.split("/").pop();
    if (lastSegment && lastSegment.includes("~")) {
      const parts = lastSegment.split("~");
      if (parts.length === 2) {
        url += `?domainId=${parts[1]}`;
      }
    }

    history.pushState(
      {},
      "",
      url
    );
  }
}
