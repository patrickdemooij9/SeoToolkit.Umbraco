import { ManifestMenu, ManifestRepository, ManifestTree, ManifestTreeItem, ManifestTreeStore, ManifestTypes } from "@umbraco-cms/backoffice/extension-registry";
import { seoToolkitTreeRepository } from "../repositories/seoToolkitTreeRepository";
import { seoToolkitTreeStore } from "../stores/seoToolkitTreeStore";

export const treeRepository : ManifestRepository = {
    type: 'repository',
    alias: "SeoToolkitTreeRepository",
    name: 'Time Tree repository',
    api: seoToolkitTreeRepository
}

export const treeStore: ManifestTreeStore = {
    type: 'treeStore',
    alias: "SeoToolkitTreeStore",
    name: 'Time tree Store',
    api: seoToolkitTreeStore
};

export const tree: ManifestTree = {
    type: 'tree',
    alias: "SeoToolkitTree",
    name: 'Time tree',
    meta: {
        repositoryAlias: "SeoToolkitTreeRepository"
    }
};

export const treeItem: ManifestTreeItem = {
    type: 'treeItem',
    kind: 'unique',
    alias: 'Time.Tree.RootItem',
    name: 'Time Tree Item',
    forEntityTypes: [
        "root"
    ]
}

export const menu: ManifestMenu = {
    type: 'menu',
    alias: "SeoToolkitMenu",
    name: 'Time Tree Menu',
    meta: {
        label: 'Times'
    }
}

export const menuItem:  ManifestTypes = {
    type: 'menuItem',
    kind: 'tree',
    alias: 'Time.Tree.MenuItem',
    name: 'Time Tree Menu Item',
    weight: 400,
    meta: {
        label: 'Times',
        icon: 'icon-alarm-clock',
        entityType: "root",
        menus: ["SeoToolkitMenu"],
        treeAlias: "SeoToolkitTree",
        hideTreeRoot: false
    }
};