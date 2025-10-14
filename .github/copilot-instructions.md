# Copilot Instructions for SeoToolkit.Umbraco

## Project Overview
SeoToolkit is a modular SEO toolkit for Umbraco (v9–16), providing features like meta fields, sitemaps, robots.txt, script management, redirects, site audit, and 404 handling. Each feature is implemented as a separate package under `src/`, allowing selective use and independent development.

## Architecture & Structure
- **Monorepo**: All packages are in `src/`, each with its own `.csproj` and `assets/`.
- **Core/Shared Logic**: Shared logic is in `SeoToolkit.Umbraco.Common` and `SeoToolkit.Umbraco.Common.Core`.
- **Feature Packages**: Each feature (e.g., MetaFields, Sitemap, RobotsTxt) has its own folder and may have a `.Core` subpackage for business logic.
- **ManifestLoader.cs**: Each package has a `ManifestLoader.cs` for Umbraco integration.
- **Tests**: Tests are in `SeoToolkit.Tests/`.

## Build & Development
- **Build Solution**: Use `dotnet build src/SeoToolkit.Umbraco.sln`.
- **Run Tests**: Use `dotnet test src/SeoToolkit.Umbraco.sln`.
- **Debugging**: Attach to the running Umbraco instance or use standard .NET debugging tools.
- **NuGet**: Main package is published as `SeoToolkit.Umbraco`.

## Conventions & Patterns
- **Tag Helpers**: Add tag helpers in `_ViewImports.cshtml` and use `<render-script>`/`<meta-fields>` in templates (see README for details).
- **Multi-language/Multi-domain**: Supported out of the box; see feature packages for implementation.
- **Configuration**: Each feature can be configured via its own `.json` file at the root (e.g., `umbraco-marketplace-seotoolkit.umbraco.sitemap.json`).
- **Extensibility**: New features should follow the pattern of a dedicated package with a `ManifestLoader.cs` and optional `.Core` for logic.

## Integration Points
- **Umbraco**: All features integrate via Umbraco's package system and are discoverable through `ManifestLoader.cs`.
- **External Scripts**: Script Manager allows user-injected scripts via the UI.

## Key Files & Directories
- `src/SeoToolkit.Umbraco.sln`: Solution file for all packages
- `src/SeoToolkit.Umbraco.[Feature]/`: Feature packages
- `src/SeoToolkit.Umbraco.Common/`: Shared logic
- `src/SeoToolkit.Tests/`: Tests
- `*.json` at root: Feature configuration
- `README.md`: Feature usage and installation

## Examples
- To add a new SEO feature, create `SeoToolkit.Umbraco.[Feature]` and `SeoToolkit.Umbraco.[Feature].Core` following the existing structure.
- To add a new tag helper, update `_ViewImports.cshtml` and reference the relevant Core package.

## References
- [Documentation](https://seotoolkit.gitbook.io/useotoolkit/)
- [README.md](../README.md)

---
If you are unsure about a pattern or integration, check the corresponding feature package or ask for clarification.
