import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { SeoToolkitTreeEntityType, SeoToolkitTreeRootType } from '../constants/seoToolkitConstants';

export interface SeoToolkitTreeItemModel extends UmbTreeItemModel {
	entityType: SeoToolkitTreeEntityType;
}

export interface SeoToolkitTreeRootModel extends UmbTreeRootModel {
	entityType: SeoToolkitTreeRootType;
}