import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
  UmbTreeAncestorsOfRequestArgs,
  UmbTreeChildrenOfRequestArgs,
  UmbTreeRootItemsRequestArgs,
  UmbTreeServerDataSourceBase,
} from "@umbraco-cms/backoffice/tree";
import {
  BackofficeSeoToolkit,
  SeoToolkitTreeItemApiModel,
} from "../api";
import {
  SEOTOOLKIT_MODULE_ENTITY,
  SEOTOOLKIT_REDIRECT_ENTITY,
  SEOTOOLKIT_ROBOTSTXT_ENTITY,
  SEOTOOLKIT_SCRIPTMANAGER_ENTITY,
  SEOTOOLKIT_SITEAUDIT_ENTITY,
  SEOTOOLKIT_NOTFOUND_ENTITY,
  SEOTOOLKIT_TREE_ROOT,
  SEOTOOLKIT_DOMAIN_ENTITY,
  SEOTOOLKIT_DOMAIN_ROOT_ENTITY,
} from "../constants/seoToolkitConstants";
import { SeoToolkitTreeItemModel } from "../trees/types";

export class seoToolkitTreeSource extends UmbTreeServerDataSourceBase<
  SeoToolkitTreeItemApiModel,
  SeoToolkitTreeItemModel
> {
  constructor(host: UmbControllerHost) {
    super(host, {
      getRootItems,
      getChildrenOf,
      getAncestorsOf,
      mapper,
    });
  }
}

const getRootItems = async (_args: UmbTreeRootItemsRequestArgs) => {
  const data =
    await BackofficeSeoToolkit.getUmbracoSeoToolkitTreeInfoRoot();
  return data;
};

const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => {
  if (args.parent.unique === null) {
    return getRootItems(args);
  } else {
    // eslint-disable-next-line local-rules/no-direct-api-import
    const data =
      await BackofficeSeoToolkit.getUmbracoSeoToolkitTreeInfoChildren({
        query: {
          parentUnique: args.parent.unique,
          skip: args.skip,
          take: args.take,
        },
      });
    return data;
  }
};

const getAncestorsOf = async (args: UmbTreeAncestorsOfRequestArgs) => {
  const response =
    await BackofficeSeoToolkit.getUmbracoSeoToolkitTreeInfoAncestors({
      query: {
        descendantId: args.treeItem.unique,
      },
    });
  // Assuming response is an array of NamedEntityTreeItemResponseModel
  return response;
};

const mapper = (item: SeoToolkitTreeItemApiModel): SeoToolkitTreeItemModel => {
  let entity = SEOTOOLKIT_MODULE_ENTITY;
  let icon = "icon-book";
  let unique = item.id;

  //TODO: Reword this
  if (
    item.id.startsWith("20A2086E-7D72-44BA-B97B-5836CAF6E28E".toLowerCase())
  ) {
    entity = SEOTOOLKIT_ROBOTSTXT_ENTITY;
    icon = "icon-cloud";
  } else if (
    item.id.startsWith("94E95F4A-2ECB-4038-BCFD-8357B7C41F1A".toLowerCase())
  ) {
    entity = SEOTOOLKIT_SCRIPTMANAGER_ENTITY;
    icon = "icon-script";
  } else if (
    item.id.startsWith("1147F58D-D2D5-425B-AEDE-DB537BDAC9EF".toLowerCase())
  ) {
    entity = SEOTOOLKIT_REDIRECT_ENTITY;
    icon = "icon-trafic";
  } else if (
    item.id.startsWith("B0D1C655-472B-40E7-9AC4-C6328EA9CF32".toLowerCase())
  ) {
    entity = SEOTOOLKIT_SITEAUDIT_ENTITY;
    icon = "icon-diagnostics";
  } else if (
    item.id.startsWith("a9b6dec6-e045-476a-ba3f-742355e18e33".toLowerCase())
  ) {
    entity = SEOTOOLKIT_NOTFOUND_ENTITY;
    icon = "icon-article";
  } else if (item.id == "ab248b43-9757-432a-9821-22f9eeb513e7".toLowerCase()){
    entity = SEOTOOLKIT_DOMAIN_ROOT_ENTITY;
    icon = "icon-globe";
  } else if (
    item.id.startsWith("ab248b43-9757-432a-9821-22f9eeb513e7".toLowerCase())
  ) {
    entity = SEOTOOLKIT_DOMAIN_ENTITY;
    icon = "icon-globe";
  }

  return {
    unique: unique,
    parent: {
      unique: item.parentId || null,
      entityType: item.parentId ? entity : SEOTOOLKIT_TREE_ROOT,
    },
    name: item.name,
    entityType: entity,
    hasChildren: item.hasChildren,
    isFolder: false,
    icon: icon,
    isDraft: item.isDraft
  };
};
