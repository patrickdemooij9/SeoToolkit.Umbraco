import { ManifestTreeItem } from "@umbraco-cms/backoffice/tree";
import { SEOTOOLKIT_DOMAIN_ENTITY } from "../constants/seoToolkitConstants";

const DomainsTreeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'seoToolkit.module.domains',
    name: 'SeoToolkit Domains',
    forEntityTypes: [
        SEOTOOLKIT_DOMAIN_ENTITY
    ]
}

export const SeoDomainsManifest = [ DomainsTreeItem ];