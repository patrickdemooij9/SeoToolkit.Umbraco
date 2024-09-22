import { ManifestSection } from "@umbraco-cms/backoffice/extension-registry";

export const seoToolkitSection : ManifestSection = {
    type: 'section',
    alias: 'SeoToolkit',
    name: 'SeoToolkit',
    weight: 10,
    meta: {
        label: 'SeoToolkit',
        pathname: 'SeoToolkit'
    }
}