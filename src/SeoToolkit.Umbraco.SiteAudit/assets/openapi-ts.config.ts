import { defaultPlugins, defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	input: 'http://localhost:57441/umbraco/openapi/seoToolkit.json',
	output: {
		path: 'src/api',
	},
	plugins: [
		...defaultPlugins,
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