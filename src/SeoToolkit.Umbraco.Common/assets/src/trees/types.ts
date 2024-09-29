import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { SeoToolkitTreeRootType } from '../constants/seoToolkitConstants';

export interface SeoToolkitTreeItemModel extends UmbTreeItemModel {
	entityType: string;
}

export interface SeoToolkitTreeRootModel extends UmbTreeRootModel {
	entityType: SeoToolkitTreeRootType;
}