# SeoToolkit.Umbraco.Deploy

Umbraco Deploy connectors for [SeoToolkit](https://github.com/patrickdemooij9/SeoToolkit.Umbraco). Lets Umbraco Deploy carry SeoToolkit settings and per-node SEO data between environments — via `.uda` disk artifacts, queue-for-transfer/restore, and content import/export.

## What it does

- **Settings** are written to `.uda` disk artifacts whenever you save them in the backoffice, and can be transferred/restored on demand like any other Deploy entity.
- **Per-node SEO data** (MetaFields values and sitemap content overrides) automatically **rides along** with content transfers and restores — when you transfer a document, its SeoToolkit data comes with it. No separate action required.

## Entity types

Eight `GuidUdi` entity types, all prefixed `seotoolkit-`:

| UDI entity type | Data |
|---|---|
| `seotoolkit-seo-setting` | Per-document-type SEO enable toggle |
| `seotoolkit-metafields-setting` | MetaFields document-type field settings |
| `seotoolkit-sitemap-page-type` | Sitemap per-document-type settings |
| `seotoolkit-script` | Script Manager scripts |
| `seotoolkit-domain-collection` | SeoToolkit domain collections |
| `seotoolkit-key-values` | Global / per-domain key-value settings |
| `seotoolkit-metafields-value` | Per-node MetaFields values (rides along with content) |
| `seotoolkit-sitemap-content` | Per-node sitemap overrides (rides along with content) |

The first six are settings-like: disk-registered and available for queue-for-transfer / restore / import-export. The last two are content-like: not disk-registered, attached to document exports as dependencies.

## Configuration

Disable individual entity types from all deploy operations via the `SeoToolkit:Deploy` config section:

```json
{
  "SeoToolkit": {
    "Deploy": {
      "DisabledEntityTypes": [ "seotoolkit-script", "seotoolkit-key-values" ]
    }
  }
}
```

A disabled connector produces no artifacts and skips processing.

## Behaviour and caveats

- **Missing target entities are skipped, not failed.** If a document type, node, or script definition referenced by an artifact doesn't exist in the target environment, that artifact is logged and skipped — a deploy never fails wholesale because of a missing SeoToolkit dependency.
- **Domain names, not ids.** Domain collections store Umbraco domain **names** in the artifact (portable), and resolve them back to local domain ids on import. Names that don't exist in the target are silently dropped, since environment hostnames are expected to differ.
- **Key/values deploy by overwrite.** Transferred keys overwrite the matching keys in the target; keys that exist only in the target are never deleted.
- **appsettings-based SeoToolkit config** (e.g. `SeoToolkit:Global`) is intentionally out of scope — deploy that through your normal configuration transformation pipeline.

## Known follow-ups

- Backoffice tree queue-for-transfer UX (the entity actions on SeoToolkit's management trees) is not wired up; import/export and restore work regardless.
- The Redirects and RobotsTxt modules are out of scope for this initial release.
