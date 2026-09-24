import { apiTags, defineModuleConfigAt } from '../../openapi-ts.shared';

// src/api also holds the hand-written Umbraco Deploy helpers, so generate into a subfolder.
export default defineModuleConfigAt('src/api/generated', apiTags.deploy);
