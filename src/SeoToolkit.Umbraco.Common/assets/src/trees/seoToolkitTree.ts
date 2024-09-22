import { ManifestMenu, ManifestRepository, ManifestTree, ManifestTreeItem, ManifestTreeStore, ManifestTypes } from "@umbraco-cms/backoffice/extension-registry";
import { seoToolkitTreeRepository } from "../repositories/seoToolkitTreeRepository";
import { seoToolkitTreeStore } from "../stores/seoToolkitTreeStore";
import { SEOTOOLKIT_TREE_ENTITY, SEOTOOLKIT_TREE_ROOT } from "../constants/seoToolkitConstants";

export const treeRepository : ManifestRepository = {
    type: 'repository',
    alias: "SeoToolkitTreeRepository",
    name: 'SeoToolkit Tree repository',
    api: seoToolkitTreeRepository
}

export const treeStore: ManifestTreeStore = {
    type: 'treeStore',
    alias: "SeoToolkitTreeStore",
    name: 'SeoToolkit tree Store',
    api: seoToolkitTreeStore
};

export const tree: ManifestTree = {
    type: 'tree',
    kind: "default",
    alias: "SeoToolkitTree",
    name: 'SeoToolkit tree',
    meta: {
        repositoryAlias: "SeoToolkitTreeRepository"
    }
};

export const treeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'default',
    alias: 'SeoToolkitTreeItem',
    name: 'SeoToolkit Tree Item',
    forEntityTypes: [
        SEOTOOLKIT_TREE_ENTITY, SEOTOOLKIT_TREE_ROOT
    ]
}

export const menu: ManifestMenu = {
    type: 'menu',
    alias: "SeoToolkitMenu",
    name: 'SeoToolkit Menu'
}

export const menuItem:  ManifestTypes = {
    type: 'menuItem',
    kind: 'tree',
    alias: 'SeoToolkitMenuItem',
    name: 'SeoToolkit Menu Item',
    weight: 400,
    meta: {
        label: 'Times',
        icon: 'icon-alarm-clock',
        entityType: SEOTOOLKIT_TREE_ROOT,
        menus: ["SeoToolkitMenu"],
        treeAlias: "SeoToolkitTree",
        hideTreeRoot: true
    }
};