import { SeoModuleEnabledConditionConfig } from "../../../../SeoToolkit.Umbraco.Common/assets/src/conditions/SeoModuleEnabledCondition";

//TODO: This probably needs to be moved to a common package and then imported in both places
declare global {
	interface UmbExtensionConditionConfigMap {
		SeoModuleEnabledConditionConfig: SeoModuleEnabledConditionConfig;
	}
}