# AGENTS.md — SeoToolkit.Umbraco

This document provides high-signal context for AI agents working in this repository.

## 1) Repository purpose

SeoToolkit is a modular SEO toolkit for Umbraco (v9–17 package line; current code targets Umbraco 17 + .NET 10), with features such as:

- Meta fields
- Sitemap
- Robots.txt
- Script manager
- Redirects
- Site audit
- Not found handling

Each feature is split into a backoffice package and (usually) a `.Core` logic package.

---

## 2) Monorepo layout (important)

Root:

- `README.md` — installation and feature overview.
- `umbraco-marketplace-seotoolkit.umbraco*.json` — marketplace metadata per package.
- `.github/workflows/` — CI/release workflows.
- `src/` — all source projects.

`src/` key projects:

- Aggregated package:
  - `SeoToolkit.Umbraco` (main NuGet package, references all feature packages)
  - `SeoToolkit.Umbraco.Core` (core connectors/startup/license tree)
- Shared:
  - `SeoToolkit.Umbraco.Common`
  - `SeoToolkit.Umbraco.Common.Core`
- Feature pairs:
  - `SeoToolkit.Umbraco.MetaFields` + `.MetaFields.Core`
  - `SeoToolkit.Umbraco.Sitemap` + `.Sitemap.Core`
  - `SeoToolkit.Umbraco.RobotsTxt` + `.RobotsTxt.Core`
  - `SeoToolkit.Umbraco.ScriptManager` + `.ScriptManager.Core`
  - `SeoToolkit.Umbraco.Redirects` + `.Redirects.Core`
  - `SeoToolkit.Umbraco.SiteAudit` + `.SiteAudit.Core`
  - `SeoToolkit.Umbraco.NotFound` + `.NotFound.Core`
- Addon:
  - `SeoToolkit.Umbraco.uSync`
- Test + sample host:
  - `SeoToolkit.Tests/SeoToolkit.Tests`
  - `SeoToolkit.Umbraco.Site` (sample site/app host)

---

## 3) Core architectural patterns

### 3.1 Package split pattern

For each feature:

- `SeoToolkit.Umbraco.<Feature>` (backoffice integration + assets + `ManifestLoader.cs`)
- `SeoToolkit.Umbraco.<Feature>.Core` (business logic/services/repos/controllers/migrations/startup)

### 3.2 Umbraco integration points

Common recurring entry points:

- `ManifestLoader.cs` in each backoffice package
  - Registers `IPackageManifestReader`
  - Emits `backofficeEntryPoint` manifest JSON
  - JS entrypoints are under `/App_Plugins/SeoToolkit/entry/...`
- `Composer` classes implementing `IComposer`
  - Register services/repositories/config
  - Append components
  - Add collection providers
- Some features register middleware via `UmbracoPipelineOptions` filters (for example sitemap/robots/redirect flows).

### 3.3 Shared base controllers and route conventions

In `SeoToolkit.Umbraco.Common.Core/Controllers`:

- `SeoToolkitAuthenticatedControllerBase`
  - `[BackOfficeRoute("seoToolkit")]`
  - Backoffice authorization policy
- `SeoToolkitPublicControllerBase`
  - `[Route("/api/seoToolkit/{controller}")]`
- `SeoApiController`
  - Public route `[Route("/api/seo")]`
  - Controlled by global `EnableApiEndpoints` setting

When adding controllers, follow one of these base patterns depending on public vs backoffice scope.

### 3.4 Typical folder structure in `.Core` packages

Most `.Core` projects use consistent folders:

- `Controllers`, `Services`, `Repositories`
- `Models`, `Config`, `Interfaces`
- `Components`, `Notifications`, `Migrations`
- Optional: `Middleware`, `Caching`, `Helpers`, `Collections`, `Startup`, `TagHelpers`

Prefer existing folders/patterns over introducing new structural conventions.

---

## 4) Front-end/backoffice assets

Each backoffice package has `assets/` with TypeScript + Vite (and often Lit-based UI components).

Important points:

- Root workspace config: `src/package.json` (npm workspaces).
- Feature assets build into each package’s `wwwroot/entry/<feature>/...` via Vite.
- `ManifestLoader` JS path must match built asset output path.

If changing backoffice UI:

1. Update the feature `assets/` code.
2. Ensure Vite output path still matches manifest JS path.
3. Build assets before packaging.

---

## 5) Build, test, packaging commands

Primary solution commands:

- `dotnet build src/SeoToolkit.Umbraco.sln`
- `dotnet test src/SeoToolkit.Umbraco.sln`

Asset commands:

- `cd src && npm install`
- `cd src && npm run start` (build all feature assets)
- `cd src && npm run deploy` (workspace deploy scripts)

CI references:

- `.github/workflows/build-and-test.yml`
  - restore + build + test on solution
- `.github/workflows/AllPackages.yml`
  - builds JS assets, builds/tests solution, packs NuGets
- Feature-specific release workflows exist per package.

---

## 6) Configuration model

Runtime settings are primarily under `SeoToolkit` in appsettings, e.g. in:

- `src/SeoToolkit.Umbraco.Site/appsettings.json`

Main sections:

- `SeoToolkit:Global`
- `SeoToolkit:SiteAudit`
- `SeoToolkit:Sitemap`
- `SeoToolkit:ScriptManager`
- `SeoToolkit:RobotsTxt`
- `SeoToolkit:Redirects`
- `SeoToolkit:MetaFields`

Many modules support `DisabledModules` arrays for selective disabling.

---

## 7) Versioning and packaging facts

- Central versions are maintained in `src/Directory.Build.props`.
- Version prefix is assigned by project-name conditions (`.Common`, `.MetaFields`, etc).
- Main aggregate package (`SeoToolkit.Umbraco`) references all feature packages.

When bumping versions, update `Directory.Build.props` consistently.

---

## 8) Testing

Test project:

- `src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj`

Current tests are mostly unit-style using NUnit + Moq, focused on middleware/controllers/providers/repositories.

Representative tests include:

- `SitemapMiddlewareTests.cs`
- `RedirectTests.cs`
- `PageNotFoundFinderTests.cs`
- `RobotsSitemapProviderTests.cs`

---

## 9) Practical guidance for AI agents

1. Prefer minimal, feature-local changes.
2. If modifying one feature, inspect both:
   - `SeoToolkit.Umbraco.<Feature>`
   - `SeoToolkit.Umbraco.<Feature>.Core`
3. Keep manifest JS path and Vite output path aligned.
4. Reuse existing base controllers and collection/composer patterns.
5. For configuration changes, update typed config models and appsettings schema/usage consistently.
6. Validate with solution build + test commands.
7. Do not create new top-level architectural patterns when an existing one already fits.

---

## 10) Fast file map (high value)

- Solution: `src/SeoToolkit.Umbraco.sln`
- Shared core startup: `src/SeoToolkit.Umbraco.Common.Core/Startup/SeoToolkitComposer.cs`
- Shared manifest pattern: `src/SeoToolkit.Umbraco.Common/ManifestLoader.cs`
- Example feature composer: `src/SeoToolkit.Umbraco.Sitemap.Core/Composers/SitemapComposer.cs`
- Example middleware: `src/SeoToolkit.Umbraco.Sitemap.Core/Middleware/SitemapMiddleware.cs`
- Main package csproj: `src/SeoToolkit.Umbraco/SeoToolkit.Umbraco.csproj`
- Sample host config: `src/SeoToolkit.Umbraco.Site/appsettings.json`
- CI baseline: `.github/workflows/build-and-test.yml`
