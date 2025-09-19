import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { NamedEntityTreeItemResponseModel } from "@umbraco-cms/backoffice/external/backend-api";
import {
  UmbTreeAncestorsOfRequestArgs,
  UmbTreeChildrenOfRequestArgs,
  UmbTreeRootItemsRequestArgs,
  UmbTreeServerDataSourceBase,
} from "@umbraco-cms/backoffice/tree";
import { SeoToolkitService } from "../api";
import {
  SEOTOOLKIT_MODULE_ENTITY,
  SEOTOOLKIT_REDIRECT_ENTITY,
  SEOTOOLKIT_ROBOTSTXT_ENTITY,
  SEOTOOLKIT_SCRIPTMANAGER_ENTITY,
  SEOTOOLKIT_SITEAUDIT_ENTITY,
  SEOTOOLKIT_NOTFOUND_ENTITY,
  SEOTOOLKIT_TREE_ROOT,
  SEOTOOLKIT_DOMAIN_ENTITY,
} from "../constants/seoToolkitConstants";
import { SeoToolkitTreeItemModel } from "../trees/types";

export class seoToolkitTreeSource extends UmbTreeServerDataSourceBase<
  NamedEntityTreeItemResponseModel,
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
  const data = await SeoToolkitService.getUmbracoSeoToolkitTreeInfoRoot();
  return data;
};

const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => {
  if (args.parent.unique === null) {
    return getRootItems(args);
  } else {
    // eslint-disable-next-line local-rules/no-direct-api-import
    const data = await SeoToolkitService.getUmbracoSeoToolkitTreeInfoChildren({
      query: {
        parentId: args.parent.unique,
        skip: args.skip,
        take: args.take,
      },
    });
    return data;
  }
};

const getAncestorsOf = async (args: UmbTreeAncestorsOfRequestArgs) => {
  const response =
    await SeoToolkitService.getUmbracoSeoToolkitTreeInfoAncestors({
      query: {
        descendantId: args.treeItem.unique,
      }
    });
  // Assuming response is an array of NamedEntityTreeItemResponseModel
  return response;
};

const mapper = (
  item: NamedEntityTreeItemResponseModel
): SeoToolkitTreeItemModel => {
  let entity = SEOTOOLKIT_MODULE_ENTITY;
  let icon = "icon-book";

  //TODO: Reword this
  switch (item.id) {
    case "20A2086E-7D72-44BA-B97B-5836CAF6E28E".toLowerCase():
      entity = SEOTOOLKIT_ROBOTSTXT_ENTITY;
      icon = "icon-cloud";
      break;
    case "94E95F4A-2ECB-4038-BCFD-8357B7C41F1A".toLowerCase():
      entity = SEOTOOLKIT_SCRIPTMANAGER_ENTITY;
      icon = "icon-script";
      break;
    case "1147F58D-D2D5-425B-AEDE-DB537BDAC9EF".toLowerCase():
      entity = SEOTOOLKIT_REDIRECT_ENTITY;
      icon = "icon-trafic";
      break;
    case "B0D1C655-472B-40E7-9AC4-C6328EA9CF32".toLowerCase():
      entity = SEOTOOLKIT_SITEAUDIT_ENTITY;
      icon = "icon-diagnostics";
      break;
    case "a9b6dec6-e045-476a-ba3f-742355e18e33".toLowerCase():
      entity = SEOTOOLKIT_NOTFOUND_ENTITY;
      icon = "icon-article";
      break;
    case "ab248b43-9757-432a-9821-22f9eeb513e7".toLowerCase():
      entity = SEOTOOLKIT_DOMAIN_ENTITY;
      icon = "icon-world-globe";
      break;
    default:
      entity = SEOTOOLKIT_MODULE_ENTITY;
      break;
  }
  return {
    unique: item.id,
    parent: {
      unique: item.parent?.id || null,
      entityType: item.parent ? entity : SEOTOOLKIT_TREE_ROOT,
    },
    name: item.name,
    entityType: entity,
    hasChildren: item.hasChildren,
    isFolder: false,
    icon: icon,
  };
};
