import type { ManifestWorkspaceActionMenuItem } from "@umbraco-cms/backoffice/workspace";
import type { ManifestEntityAction } from "@umbraco-cms/backoffice/entity-action";

const transferSeoWorkspaceAction: ManifestWorkspaceActionMenuItem = {
	type: "workspaceActionMenuItem",
	kind: "default",
	alias: "seoToolkit.deploy.transferSeoNow",
	name: "SeoToolkit Deploy Transfer SEO Now",
	weight: 90,
	api: () => import("../actions/transferSeoAction"),
	forWorkspaceActions: ["Umb.WorkspaceAction.Document.SaveAndPublish"],
	meta: {
		label: "Transfer SEO Now",
		icon: "icon-cloud-upload",
	},
	conditions: [
		{
			alias: "Umb.Condition.WorkspaceAlias",
			match: "Umb.Workspace.Document",
		},
		{
			alias: "SeoToolkit.SeoEnabled",
		},
	],
};

const queueSeoEntityAction: ManifestEntityAction = {
	type: "entityAction",
	kind: "default",
	alias: "seoToolkit.deploy.queueSeo",
	name: "SeoToolkit Deploy Queue SEO",
	forEntityTypes: ["document"],
	api: () => import("../actions/queueSeoEntityAction"),
	// Just below Deploy's "Partial restore" (weight -40) in the content action menu.
	weight: -41,
	meta: {
		label: "Add SEO to Transfer Queue",
		icon: "icon-cloud-upload",
	},
};

const restoreSeoEntityAction: ManifestEntityAction = {
	type: "entityAction",
	kind: "default",
	alias: "seoToolkit.deploy.restoreSeo",
	name: "SeoToolkit Deploy Restore SEO",
	forEntityTypes: ["document"],
	api: () => import("../actions/restoreSeoEntityAction"),
	// Just below "Add SEO to Transfer Queue" (-41).
	weight: -42,
	meta: {
		label: "Restore SEO",
		icon: "icon-download",
	},
};

export const DeployManifests = [transferSeoWorkspaceAction, queueSeoEntityAction, restoreSeoEntityAction];
