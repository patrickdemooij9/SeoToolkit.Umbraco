import { ManifestDashboard } from "@umbraco-cms/backoffice/extension-registry";

export const welcomeDashboardManifest : ManifestDashboard = {
    type: "dashboard",
    alias: "seoToolkitWelcomeDashboard",
    name: "Welcome",
    meta: {
        pathname: "welcome"
    },
    element: () => import("./welcomeDashboard.element"),
    conditions: [
        {
            alias: "Umb.Condition.SectionAlias",
            match: "SeoToolkit"
        }
    ]
}