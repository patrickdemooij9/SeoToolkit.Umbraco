# SeoToolkit.Umbraco.Deploy

Umbraco Deploy connectors for [SeoToolkit](https://github.com/patrickdemooij9/SeoToolkit.Umbraco). Lets Umbraco Deploy carry SeoToolkit settings and per-node SEO data between environments — via `.uda` disk artifacts, queue-for-transfer/restore, and content import/export.

## Requirements

Umbraco Deploy must be installed on the site (`Umbraco.Deploy.OnPrem`, or Umbraco Cloud). This package doesn't install it for you. Without it, the package does nothing: no handlers or backoffice actions are registered, so the site still starts.

## What it does

- **Settings** are written to `.uda` disk artifacts whenever you save them in the backoffice, and can be transferred/restored on demand like any other Deploy entity.
- **Per-node SEO data** (MetaFields values and sitemap content overrides) automatically **rides along** with content transfers and restores — when you transfer or export a document, its SeoToolkit data comes with it (written per-node as its own `.uda` and attached to the document as a dependency). No separate action required.

## Entity types

Eight `GuidUdi` entity types, all prefixed `seotoolkit-`:

| UDI entity type | Data |
|---|---|
| `seotoolkit-seo-setting` | Per-document-type SEO enable toggle |
| `seotoolkit-metafields-setting` | MetaFields document-type field settings |
| `seotoolkit-sitemap-page-type` | Sitemap per-document-type settings |
| `seotoolkit-script` | Script Manager scripts (opt-in, see Configuration) |
| `seotoolkit-domain-collection` | SeoToolkit domain collections |
| `seotoolkit-key-values` | Global / per-domain key-value settings |
| `seotoolkit-metafields-value` | Per-node MetaFields values (rides along with content) |
| `seotoolkit-sitemap-content` | Per-node sitemap overrides (rides along with content) |

The first six are settings-like: disk-registered and available for queue-for-transfer / restore / import-export. The last two are content-like: not disk-registered, attached to document exports as dependencies.

## Configuration

Each entity type can be enabled or disabled under the `SeoToolkit:Deploy` config section. A disabled entity type produces no artifacts and skips processing. These are the defaults:

```json
{
  "SeoToolkit": {
    "Deploy": {
      "SeoSettings": { "Enabled": true },
      "MetaFieldsSettings": { "Enabled": true },
      "MetaFieldsValues": { "Enabled": true },
      "SitemapPageTypes": { "Enabled": true },
      "SitemapContent": { "Enabled": true },
      "Scripts": { "Enabled": false },
      "DomainCollections": { "Enabled": true },
      "KeyValues": { "Enabled": true }
    }
  }
}
```

Scripts usually hold environment-specific settings (analytics ids, tag managers, etc.), so they are **disabled by default**. Set `Scripts:Enabled` to `true` to include them.

Restores are **convergent**: target key/values, per-node meta field values, and document-type field settings that are absent from the incoming artifact are deleted so the target mirrors the source.

## Behaviour and caveats

- **Missing target entities are skipped, not failed.** If a document type, node, or script definition referenced by an artifact doesn't exist in the target environment, that artifact is logged and skipped — a deploy never fails wholesale because of a missing SeoToolkit dependency.
- **Domain names, not ids.** Domain collections store Umbraco domain **names** in the artifact (portable), and resolve them back to local domain ids on import. Names that don't exist in the target are silently dropped, since environment hostnames are expected to differ.
- **Key/values converge.** Transferred keys overwrite the matching keys in the target, and keys that exist only in the target are deleted.
- **Per-node SEO data updates travel.** The per-node MetaFields values and sitemap overrides are attached to the document as `Match` dependencies, so Deploy compares their checksum and re-transfers them whenever the values change — not just on the first transfer.
- **Per-node SEO data stays in sync on disk.** Whenever a node's MetaFields values change, its `.uda` is rewritten; when the node's last value is removed, the `.uda` is deleted. The same applies to sitemap content overrides: the `.uda` is rewritten while the node has non-default settings and deleted when they are reset to default. Removing a node's data therefore propagates as a delete on restore, and removing *some* meta field values converges as well.
- **Disabling a connector can affect dependent deploys.** Scripts and key/values emit dependencies on `seotoolkit-domain-collection`; if `DomainCollections` is disabled, those dependencies cannot be satisfied and a deploy that includes domain-scoped scripts or key/values may error. Disable the domain-collection connector only if you also disable the connectors that depend on it.
- **appsettings-based SeoToolkit config** (e.g. `SeoToolkit:Global`) is intentionally out of scope — deploy that through your normal configuration transformation pipeline.

## Known follow-ups

- Backoffice tree queue-for-transfer UX (the entity actions on SeoToolkit's management trees) is not wired up; import/export and restore work regardless.
- The Redirects and RobotsTxt modules are out of scope for this initial release.
