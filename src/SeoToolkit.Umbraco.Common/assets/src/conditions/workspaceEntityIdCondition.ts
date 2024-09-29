import {
    ManifestCondition,
    UmbConditionConfigBase,
    UmbConditionControllerArguments,
    UmbExtensionCondition
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

export type WorkspaceEntityIdConditionConfig = UmbConditionConfigBase & {
    match: string;
};

export class WorkspaceEntityIdCondition extends UmbConditionBase<WorkspaceEntityIdConditionConfig> implements UmbExtensionCondition {
    constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceEntityIdConditionConfig>) {
        super(host, args);

        this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
            console.log(context);
		});
        console.log(args);
        if (args.config.match === 'Yes') {
            this.permitted = true;
            args.onChange();
        }
    }
}

export const manifest: ManifestCondition = {
    type: 'condition',
    name: 'Workspace Entity Id Condition',
    alias: 'SeoToolkit.WorkspaceEntityIdCondition',
    api: WorkspaceEntityIdCondition,
};