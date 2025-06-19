import {
    ManifestCondition,
    UmbConditionConfigBase,
    UmbConditionControllerArguments,
    UmbExtensionCondition
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export type WorkspaceEntityIdConditionConfig = UmbConditionConfigBase & {
    match: string;
};

export class WorkspaceEntityIdCondition extends UmbConditionBase<WorkspaceEntityIdConditionConfig> implements UmbExtensionCondition {
    constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceEntityIdConditionConfig>) {
        super(host, args);

        if (args.config.match === 'Yes') {
            this.permitted = true;
            args.onChange(true);
        }
    }
}

export const manifest: ManifestCondition = {
    type: 'condition',
    name: 'Workspace Entity Id Condition',
    alias: 'SeoToolkit.WorkspaceEntityIdCondition',
    api: WorkspaceEntityIdCondition,
};