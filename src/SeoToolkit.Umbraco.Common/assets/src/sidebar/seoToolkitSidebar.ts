import { ManifestSectionSidebarApp } from "@umbraco-cms/backoffice/extension-registry";

export const seoToolkitSidebar : ManifestSectionSidebarApp = {
    type: "sectionSidebarApp",
    kind: 'menuWithEntityActions',
    alias: 'seoToolkit.sidebar',
    name: 'SeoToolkit',
    meta: {
        label: "SeoToolkit",
        menu: "SeoToolkitMenu",
    },
    conditions: [
        {
            alias: "Umb.Condition.SectionAlias",
            match: "SeoToolkit"
        }
    ]   
}