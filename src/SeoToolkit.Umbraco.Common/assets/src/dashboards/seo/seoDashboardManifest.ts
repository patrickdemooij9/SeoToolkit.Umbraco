import { ManifestDashboard } from "@umbraco-cms/backoffice/dashboard";
import SeoDashboardElement from "./seoDashboard.element";

export const seoDashboardManifest: ManifestDashboard = {
    type: "dashboard",
    alias: "seoToolkitSeoDashboard",
    name: "SEO Dashboard",
    meta: {
        pathname: "seo-overview"
    },
    element: SeoDashboardElement,
    conditions: [
        {
            alias: "Umb.Condition.SectionAlias",
            match: "SeoToolkit"
        }
    ]
}
