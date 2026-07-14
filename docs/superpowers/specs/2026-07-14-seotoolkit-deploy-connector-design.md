# SeoToolkit.Umbraco.Deploy — Umbraco Deploy Connector Design

**Date:** 2026-07-14
**Status:** Approved

## Goal

A new `SeoToolkit.Umbraco.Deploy` package that integrates SeoToolkit with Umbraco Deploy so that:

1. SeoToolkit configuration — DB-backed global settings (key/values, domain settings) and per-document-type settings (SEO enabled-toggle, MetaFields settings, Sitemap page-type settings) plus Script Manager scripts — can be imported/exported and deployed between environments.
2. Per-node SEO data (MetaFields values, Sitemap per-node overrides) travels automatically whenever a content node is transferred or restored.

Out of scope for v1: Redirects, Robots.txt, SiteAudit (runtime data), NotFound, AI modules, and appsettings-based configuration (`SeoToolkit:Global` etc. — belongs in config transforms, not Deploy).

## Reference implementations

- Umbraco Deploy source: `~/Source/github/umbraco/Umbraco-Deploy` (connector bases, artifacts, registration) and core interfaces in `Umbraco.Cms.Core.Deploy`.
- Umbraco Commerce Deploy: `~/Source/GitHub/Umbraco/Umbraco.Commerce.Deploy` — canonical third-party connector package (UDI registration, disk + transfer wiring, connector bases).
- `SeoToolkit.Umbraco.uSync` — existing serialization blueprint, especially `Serializers/MetaFieldsSerializer.cs` for round-tripping field values through each field's `ValueConverter`.

## 1. Project & packaging

- One new project `src/SeoToolkit.Umbraco.Deploy` (uSync-project precedent): `net10.0`, `Nullable` enabled.
- References: `Umbraco.Deploy.Infrastructure [18.0.0, 18.999)` (brings `Umbraco.Deploy.Core` transitively) + project references to `SeoToolkit.Umbraco.Common.Core`, `MetaFields.Core`, `Sitemap.Core`, `ScriptManager.Core`.
- Own version variable in `src/Directory.Build.props`, starting `1.0.0-beta1`.
- Not added to the `SeoToolkit.Umbraco` meta-package; it is an opt-in add-on.
- **Soft-fail:** every connector and notification handler checks SeoToolkit's `DisabledModules` config and no-ops for disabled modules, so a single package tolerates partial module use.

## 2. UDI entity types & artifacts

All `GuidUdi`. Entity type constants live in a `SeoToolkitDeployConstants` class; declared via `[UdiDefinition]` on each connector and registered with `UdiParser.RegisterUdiType` in the component (Commerce pattern).

| Entity type | Guid source | Artifact contents & dependencies |
|---|---|---|
| `seotoolkit-seo-setting` | content-type key | `Enabled` flag; dependency on the document type (`Exist`) |
| `seotoolkit-metafields-setting` | content-type key | fields list (alias / value / useInheritedValue — values round-tripped through each field's `ValueConverter`, as in the uSync serializer), inheritance content-type UDI; dependencies: doc type, inheritance doc type, UDIs embedded in field values |
| `seotoolkit-sitemap-page-type` | content-type key | hide flag, change frequency, priority; doc-type dependency |
| `seotoolkit-metafields-value` | node key | **all** aliases × cultures for the node in one artifact; dependencies: the document (`Exist`) + UDIs found in values (media/content refs) |
| `seotoolkit-sitemap-content` | node key | exclude flag, change frequency, priority; document dependency |
| `seotoolkit-script` | script entity key | script definition |
| `seotoolkit-keyvalue` (+ domain collections/settings) | entity GUID | key/value pairs; domain references serialized as **domain name + assigned node UDI** (the table stores Umbraco's *int* domain id, which is not portable — resolved via `IDomainService` on import) |

Artifacts derive from `DeployArtifactBase<GuidUdi>`. A `SeoToolkitArtifactDependency` helper mirrors `UmbracoCommerceArtifactDependency` (defaults `mode: Exist`, `ordering: false`).

## 3. Service connectors & passes

- Shared base `SeoToolkitEntityServiceConnectorBase<TArtifact, TEntity>` modeled on `UmbracoCommerceEntityServiceConnectorBase`: implements `GetArtifactAsync(udi)`, `GetRangeAsync`/`ExpandRangeAsync`, `ProcessInitAsync`; subclasses supply `UdiEntityType`, entity load/enumerate, artifact build/apply.
- Pass schedule (built-ins: doc types 0, media/member 3, content 4–6):
  - Settings-like entities (per-doctype, scripts, key/values): **pass 2** — after document types, before content.
  - Per-node entities (metafields values, sitemap content): **pass 7** — after documents exist.
- Import: missing document type or node → skip that artifact with a logged warning, never fail the whole deploy.

## 4. Per-node data riding along with content

`INotificationAsyncHandler<ArtifactExportedNotification>`: when the exported artifact is a document artifact, look up MetaFields values and Sitemap overrides for the node key and append `ArtifactDependency(seoUdi, ordering: false, Exist)` to the content artifact's dependencies. Deploy's dependency walker then includes the SEO artifacts in the same transfer/restore with no editor action.

Risk & fallback: verify dependencies are added before the artifact's lazy checksum is computed; if mutation at `ArtifactExported` time proves fragile, use `ArtifactExportingNotification` instead.

## 5. Disk (`.uda`) + transfer/restore registration

A `SeoToolkitDeployComposer` + `SeoToolkitDeployComponent` (Commerce pattern):

- **Disk:** register the settings-like entity types with `IDiskEntityService.RegisterDiskEntityType`; subscribe to SeoToolkit save/delete notifications and write/delete `.uda` files via the connector + `IDiskEntityService.WriteArtifactsAsync`. MetaFields, SeoSettings and Sitemap already publish saved notifications (used by uSync); **ScriptManager and key/value/domain services need save/delete notifications added to their Core services** (small additive Core changes).
- **Transfer/restore:** register the same settings-like types with `ITransferEntityService.RegisterTransferEntityType` (`SupportsQueueForTransfer`, `SupportsRestore`, `SupportsImportExport`), hooked to SeoToolkit's existing backoffice trees.
- Per-node entity types are transfer-only (no `.uda`) — they are content-like data.
- Connectors themselves are auto-discovered by Deploy's type loader; no manual collection registration needed.

## 6. Configuration

Optional `SeoToolkit:Deploy` section bound to a `SeoToolkitDeploySettings` options class (accessor pattern as in Commerce). v1 keeps it minimal: ignore-lists for key/value keys and script aliases that are environment-specific and must not be serialized or overwritten.

## 7. Testing

Unit tests in `SeoToolkit.Tests`:

- Artifact ↔ entity round-trip per connector (build artifact from entity, apply to empty target, compare).
- Culture-variant MetaFields values (multiple cultures + invariant).
- UDI extraction from field-value JSON (media/content references become dependencies and survive the round trip).
- Soft-fail: connectors no-op when the owning module is disabled.

## Open items to verify early in implementation

1. Whether the ScriptManager entity has a GUID key; if not, add one via an Umbraco `MigrationPlan` migration (GUID-conversion precedent exists in `Common.Core`).
2. Checksum/dependency-mutation timing on `ArtifactExportedNotification` (see §4 fallback).
3. That a third-party pass number of 7 is honoured by Deploy's pass scheduler.
