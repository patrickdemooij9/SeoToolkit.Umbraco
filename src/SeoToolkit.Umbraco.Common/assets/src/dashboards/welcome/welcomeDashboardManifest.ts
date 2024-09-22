import { ManifestDashboard } from "@umbraco-cms/backoffice/extension-registry";
import MyWelcomeDashboardElement from "./welcomeDashboard.element";

export const welcomeDashboardManifest : ManifestDashboard = {
    type: "dashboard",
    alias: "seoToolkitWelcomeDashboard",
    name: "Welcome",
    meta: {
        pathname: "welcome"
    },
    element: MyWelcomeDashboardElement,
    conditions: [
        {
            alias: "Umb.Condition.SectionAlias",
            match: "SeoToolkit"
        }
    ]
}