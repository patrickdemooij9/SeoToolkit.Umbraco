// Owner types for schema entries. Must match SchemaOwnerTypeConstants on the server.
export const SCHEMA_OWNER_TYPE = {
  content: "content",
  documentType: "documentType",
  schemaEntry: "schemaEntry",
  website: "website",
} as const;

// Website-wide schemas use a single global collection anchored at the empty guid.
export const WEBSITE_OWNER_KEY = "00000000-0000-0000-0000-000000000000";
