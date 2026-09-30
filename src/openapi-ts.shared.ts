import { defineConfig } from '@hey-api/openapi-ts';

/**
 * Every module generates its client from the same SeoToolkit OpenAPI document,
 * so the generator settings live here instead of being copied into each module.
 * A module only declares which parts of the API it needs.
 */
const input = 'http://localhost:57441/umbraco/openapi/seoToolkit.json';

/**
 * The OpenAPI tags, which match the `ApiExplorerSettings.GroupName` values on
 * the controllers.
 */
export const apiTags = {
	aiIntegration: 'Backoffice SeoToolkit AI Integration',
	deploy: 'Backoffice SeoToolkit Deploy',
	metaFields: 'Backoffice SeoToolkit MetaFields',
	notFound: 'Backoffice SeoToolkit NotFound',
	publicApi: 'SeoToolkit Public Api',
	redirects: 'Backoffice SeoToolkit Redirects',
	robotsTxt: 'Backoffice SeoToolkit RobotsTxt',
	scriptManager: 'Backoffice SeoToolkit ScriptManager',
	siteAudit: 'Backoffice SeoToolkit SiteAudit',
	sitemap: 'Backoffice SeoToolkit Sitemap',
	/** Domains, settings, modules and tree endpoints, shared by every module. */
	shared: 'Backoffice SeoToolkit',
} as const;

/**
 * Builds the config for a single module. Only the operations belonging to the
 * given tags (plus the shared ones) end up in the output, along with the models
 * they reference. Models nothing references are dropped.
 */
export const defineModuleConfig = (...tags: ReadonlyArray<string>) =>
	defineModuleConfigAt('src/api', ...tags);

/**
 * Same as defineModuleConfig, but generates into the given folder. The
 * generator clears its output folder, so use this when src/api also holds
 * hand-written files.
 */
export const defineModuleConfigAt = (outputPath: string, ...tags: ReadonlyArray<string>) =>
	defineConfig({
		input,
		output: {
			path: outputPath,
		},
		parser: {
			filters: {
				tags: {
					include: [apiTags.shared, ...tags],
				},
			},
		},
		plugins: [
			{
				name: '@hey-api/client-fetch',
				exportFromIndex: true,
				throwOnError: true,
			},
			{
				name: '@hey-api/typescript',
				enums: 'typescript',
			},
			{
				name: '@hey-api/sdk',
				operations: {
					strategy: 'byTags'
				}
			},
		],
	});
