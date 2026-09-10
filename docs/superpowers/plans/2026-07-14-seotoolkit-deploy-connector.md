# SeoToolkit.Umbraco.Deploy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A new `SeoToolkit.Umbraco.Deploy` NuGet package that lets Umbraco Deploy import/export SeoToolkit global + per-document-type settings and carry per-node SEO data along with content transfers/restores.

**Architecture:** One project containing Deploy artifacts (`DeployArtifactBase<GuidUdi>`), auto-discovered `IServiceConnector` implementations sharing a common base class, a composer/component that registers UDI types + disk (`.uda`) + transfer entity types (Commerce Deploy pattern), and an `ArtifactExportedNotification` handler that appends per-node SEO artifacts as dependencies of document artifacts.

**Tech Stack:** .NET 10 (`net10.0`), Umbraco CMS 18, `Umbraco.Deploy.Infrastructure [18.0.0, 18.999)`, Newtonsoft.Json (matches SeoToolkit Core), NUnit 3 + Moq (existing test project).

**Spec:** `docs/superpowers/specs/2026-07-14-seotoolkit-deploy-connector-design.md`

## Global Constraints

- TFM `net10.0`, `<ImplicitUsings>enable</ImplicitUsings>`, `<Nullable>enable</Nullable>` (match `SeoToolkit.Umbraco.uSync.csproj`).
- Package version `1.0.0-beta1` via `src/Directory.Build.props` variable `SeoToolkitVersionDeploy`.
- Only Deploy package referenced: `Umbraco.Deploy.Infrastructure` `[18.0.0, 18.999)`. Never reference `Umbraco.Deploy.OnPrem`/`Cloud`/`UI`.
- All UDI entity types are `GuidUdi`, prefixed `seotoolkit-`.
- Settings-like connectors run `ProcessPasses = [2]`; per-node connectors run `ProcessPasses = [7]` (built-ins: doc types 0, media 3, content 4–6).
- Missing document type / node on import → log warning and skip; never throw and fail the whole deploy.
- Use `Newtonsoft.Json.JsonConvert` for value blobs (SeoToolkit Core convention), not System.Text.Json.
- All new code in namespaces under `SeoToolkit.Umbraco.Deploy.*`.
- Working branch: `feature/deploy-connector` (already created). Commit after every task.
- Build command (run from repo root): `dotnet build src/SeoToolkit.Umbraco.sln`. Test command: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~Deploy"` (Playwright tests in that project need a running site — always use this filter).
- Reference sources for API shapes (read-only, do not modify): `~/Source/GitHub/Umbraco/Umbraco.Commerce.Deploy` and `~/Source/github/umbraco/Umbraco-Deploy`.

---

### Task 1: Project scaffold

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/SeoToolkit.Umbraco.Deploy.csproj`
- Modify: `src/Directory.Build.props` (version variable + VersionPrefix condition)
- Modify: `src/SeoToolkit.Umbraco.sln` (add project via `dotnet sln add`)
- Modify: `src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj` (add project reference)

**Interfaces:**
- Consumes: nothing.
- Produces: buildable empty project `SeoToolkit.Umbraco.Deploy` that later tasks add code to; test project can reference `SeoToolkit.Umbraco.Deploy` types.

- [ ] **Step 1: Create the csproj**

Create `src/SeoToolkit.Umbraco.Deploy/SeoToolkit.Umbraco.Deploy.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net10.0</TargetFrameworks>
    <Product>SeoToolkit Deploy</Product>
    <PackageId>SeoToolkit.Umbraco.Deploy</PackageId>
    <Title>SeoToolkit Deploy</Title>
    <Description>Connectors to transfer SeoToolkit settings and content data with Umbraco Deploy</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageTags>umbraco plugin package</PackageTags>
    <RootNamespace>SeoToolkit.Umbraco.Deploy</RootNamespace>
    <PackageProjectUrl>https://github.com/patrickdemooij9/SeoToolkit.Umbraco</PackageProjectUrl>
    <RepositoryUrl>https://github.com/patrickdemooij9/SeoToolkit.Umbraco</RepositoryUrl>
    <PackageIconUrl>https://raw.githubusercontent.com/patrickdemooij9/SeoToolkit.Umbraco/main/package/SeoToolkitIcon.png</PackageIconUrl>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Umbraco.Deploy.Infrastructure" Version="[18.0.0,18.999)" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SeoToolkit.Umbraco\SeoToolkit.Umbraco.csproj" />
  </ItemGroup>

</Project>
```

Note: referencing the `SeoToolkit.Umbraco` meta project (which references all feature Cores) follows the uSync project precedent exactly. No explicit `<Version>` — `Directory.Build.props` supplies it.

- [ ] **Step 2: Add the version variable to Directory.Build.props**

In `src/Directory.Build.props`, after the line `<SeoToolkitVersionUsync>1.0.0-beta1</SeoToolkitVersionUsync>` add:

```xml
	<SeoToolkitVersionDeploy>1.0.0-beta1</SeoToolkitVersionDeploy>
```

and after the line `<VersionPrefix Condition="$([System.String]::Copy('$(MSBuildProjectName)').Contains('.uSync'))">$(SeoToolkitVersionUsync)</VersionPrefix>` add:

```xml
	<VersionPrefix Condition="$([System.String]::Copy('$(MSBuildProjectName)').EndsWith('.Deploy'))">$(SeoToolkitVersionDeploy)</VersionPrefix>
```

(`EndsWith` not `Contains` — nothing else ends with `.Deploy`, and `Contains` would also be safe today but `EndsWith` is precise.)

- [ ] **Step 3: Add project to solution and test project**

```bash
cd src
dotnet sln SeoToolkit.Umbraco.sln add SeoToolkit.Umbraco.Deploy/SeoToolkit.Umbraco.Deploy.csproj
```

In `src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj`, in the existing `<ItemGroup>` with the project reference, add:

```xml
    <ProjectReference Include="..\..\SeoToolkit.Umbraco.Deploy\SeoToolkit.Umbraco.Deploy.csproj" />
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build src/SeoToolkit.Umbraco.sln`
Expected: Build succeeded (the new empty project restores `Umbraco.Deploy.Infrastructure` and compiles). If NuGet cannot find an 18.x `Umbraco.Deploy.Infrastructure` package, check available versions with `dotnet package search Umbraco.Deploy.Infrastructure --prerelease` and pin the floor to the lowest published 18.x version.

- [ ] **Step 5: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/Directory.Build.props src/SeoToolkit.Umbraco.sln src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj
git commit -m "feat(deploy): scaffold SeoToolkit.Umbraco.Deploy project"
```

---

### Task 2: Constants, dependency helper, deploy settings, composer + component skeleton

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/SeoToolkitDeployConstants.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/SeoToolkitArtifactDependency.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Configuration/SeoToolkitDeploySettings.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComposer.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComponent.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SeoToolkitDeployConstantsTests.cs`

**Interfaces:**
- Consumes: `Umbraco.Cms.Core.Deploy.ArtifactDependency`, `UdiParser`, `IUmbracoBuilder`.
- Produces (used by every later task):
  - `SeoToolkitDeployConstants.UdiEntityType.SeoSetting|MetaFieldsSetting|SitemapPageType|Script|DomainCollection|KeyValues|MetaFieldsValue|SitemapContent` (string consts)
  - `SeoToolkitDeployConstants.RootKeyValuesGuid` (`Guid`)
  - `SeoToolkitArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Exist)` : `ArtifactDependency`
  - `SeoToolkitDeploySettings { string[] DisabledEntityTypes }` bound from config section `SeoToolkit:Deploy`
  - `SeoToolkitDeployComponent` with private `RegisterUdiTypes()` (disk/transfer wiring added in Tasks 11–12)

- [ ] **Step 1: Write the failing test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SeoToolkitDeployConstantsTests.cs`:

```csharp
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SeoToolkitDeployConstantsTests
    {
        [Test]
        public void UdiEntityTypes_AreAllSeoToolkitPrefixed()
        {
            var all = new[]
            {
                SeoToolkitDeployConstants.UdiEntityType.SeoSetting,
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting,
                SeoToolkitDeployConstants.UdiEntityType.SitemapPageType,
                SeoToolkitDeployConstants.UdiEntityType.Script,
                SeoToolkitDeployConstants.UdiEntityType.DomainCollection,
                SeoToolkitDeployConstants.UdiEntityType.KeyValues,
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue,
                SeoToolkitDeployConstants.UdiEntityType.SitemapContent,
            };
            Assert.That(all, Is.Unique);
            Assert.That(all, Is.All.Matches<string>(s => s.StartsWith("seotoolkit-")));
        }

        [Test]
        public void SeoToolkitArtifactDependency_DefaultsToExistNotOrdering()
        {
            var udi = new global::Umbraco.Cms.Core.GuidUdi(
                SeoToolkitDeployConstants.UdiEntityType.SeoSetting, System.Guid.NewGuid());
            var dep = new SeoToolkitArtifactDependency(udi);
            Assert.Multiple(() =>
            {
                Assert.That(dep.Mode, Is.EqualTo(ArtifactDependencyMode.Exist));
                Assert.That(dep.Ordering, Is.False);
            });
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~SeoToolkitDeployConstantsTests"`
Expected: FAIL to compile — `SeoToolkitDeployConstants` does not exist.

- [ ] **Step 3: Implement constants, dependency helper, settings**

Create `src/SeoToolkit.Umbraco.Deploy/SeoToolkitDeployConstants.cs`:

```csharp
namespace SeoToolkit.Umbraco.Deploy
{
    public static class SeoToolkitDeployConstants
    {
        public static class UdiEntityType
        {
            public const string SeoSetting = "seotoolkit-seo-setting";
            public const string MetaFieldsSetting = "seotoolkit-metafields-setting";
            public const string SitemapPageType = "seotoolkit-sitemap-page-type";
            public const string Script = "seotoolkit-script";
            public const string DomainCollection = "seotoolkit-domain-collection";
            public const string KeyValues = "seotoolkit-key-values";
            public const string MetaFieldsValue = "seotoolkit-metafields-value";
            public const string SitemapContent = "seotoolkit-sitemap-content";
        }

        /// <summary>
        /// Well-known GUID used as the UDI id for the single root (no-domain) key/values artifact.
        /// </summary>
        public static readonly Guid RootKeyValuesGuid = new("5e0a7f2c-9d4b-4c6a-8e1f-3b2a6c9d0e51");
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/SeoToolkitArtifactDependency.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Umbraco.Deploy
{
    public class SeoToolkitArtifactDependency : ArtifactDependency
    {
        public SeoToolkitArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Exist)
            : base(udi, false, mode)
        {
        }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Configuration/SeoToolkitDeploySettings.cs`:

```csharp
namespace SeoToolkit.Umbraco.Deploy.Configuration
{
    /// <summary>
    /// Bound from the "SeoToolkit:Deploy" configuration section.
    /// </summary>
    public class SeoToolkitDeploySettings
    {
        /// <summary>
        /// SeoToolkit Deploy UDI entity types (e.g. "seotoolkit-script") to exclude from
        /// deploy operations. Disabled connectors return no artifacts and skip processing.
        /// </summary>
        public string[] DisabledEntityTypes { get; set; } = [];
    }
}
```

- [ ] **Step 4: Implement composer + component skeleton**

Create `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComposer.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<SeoToolkitDeploySettings>()
                .Bind(builder.Config.GetSection("SeoToolkit:Deploy"));

            builder.Components().Append<SeoToolkitDeployComponent>();
        }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComponent.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComponent : IAsyncComponent
    {
        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            RegisterUdiTypes();
            return Task.CompletedTask;
        }

        public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
            => Task.CompletedTask;

        private static void RegisterUdiTypes()
        {
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.Script, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.KeyValues, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, UdiType.GuidUdi);
        }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~SeoToolkitDeployConstantsTests"`
Expected: PASS (2 tests).

- [ ] **Step 6: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): add constants, dependency helper, settings, composer skeleton"
```

---

### Task 3: Service connector base class

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitEntityServiceConnectorBase.cs`

**Interfaces:**
- Consumes: `ServiceConnectorBase<TArtifact, GuidUdi, TEntity>` from `Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors`; `DeployArtifactBase<GuidUdi>` from `Umbraco.Deploy.Infrastructure.Artifacts`; `SeoToolkitDeploySettings` via `IOptionsMonitor`.
- Produces (every connector in Tasks 4–10 inherits this):

```csharp
public abstract class SeoToolkitEntityServiceConnectorBase<TArtifact, TEntity>
    : ServiceConnectorBase<TArtifact, GuidUdi, TEntity>
    where TArtifact : DeployArtifactBase<GuidUdi>
    where TEntity : class
{
    protected SeoToolkitEntityServiceConnectorBase(IOptionsMonitor<SeoToolkitDeploySettings> settings);
    public abstract string UdiEntityType { get; }
    protected bool IsDisabled { get; }                       // reads settings.CurrentValue.DisabledEntityTypes
    public abstract string GetEntityName(TEntity entity);
    protected abstract GuidUdi GetEntityUdi(TEntity entity); // new GuidUdi(UdiEntityType, key)
    public abstract Task<TEntity?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default);
    public abstract IAsyncEnumerable<TEntity> GetEntitiesAsync(CancellationToken cancellationToken = default);
    public abstract Task<TArtifact?> GetArtifactAsync(GuidUdi? udi, TEntity? entity, CancellationToken cancellationToken = default);
    // base implements: GetArtifactAsync(entity/udi overloads), GetRangeAsync x2, ExpandRangeAsync, ProcessInitAsync
}
```

- [ ] **Step 1: Implement the base class**

This is a direct adaptation of `UmbracoCommerceEntityServiceConnectorBase` (`~/Source/GitHub/Umbraco/Umbraco.Commerce.Deploy/src/Umbraco.Commerce.Deploy/Connectors/ServiceConnectors/UmbracoCommerceEntityServiceConnectorBase.cs`) with the store-specific logic removed and a disabled-entity-types guard added.

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitEntityServiceConnectorBase.cs`:

```csharp
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    public abstract class SeoToolkitEntityServiceConnectorBase<TArtifact, TEntity>(
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : ServiceConnectorBase<TArtifact, GuidUdi, TEntity>
        where TArtifact : DeployArtifactBase<GuidUdi>
        where TEntity : class
    {
        public abstract string UdiEntityType { get; }

        public override string[] ValidOpenSelectors => ["this", "this-and-descendants", "descendants"];

        protected bool IsDisabled
            => settings.CurrentValue.DisabledEntityTypes.Contains(UdiEntityType, StringComparer.OrdinalIgnoreCase);

        public abstract string GetEntityName(TEntity entity);

        protected abstract GuidUdi GetEntityUdi(TEntity entity);

        public abstract Task<TEntity?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default);

        public abstract IAsyncEnumerable<TEntity> GetEntitiesAsync(CancellationToken cancellationToken = default);

        public abstract Task<TArtifact?> GetArtifactAsync(GuidUdi? udi, TEntity? entity, CancellationToken cancellationToken = default);

        public override Task<TArtifact> GetArtifactAsync(
            TEntity entity,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
            => GetArtifactAsync(GetEntityUdi(entity), entity, cancellationToken)!;

        public override async Task<TArtifact?> GetArtifactAsync(
            GuidUdi? udi,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            EnsureType(udi);
            if (IsDisabled)
            {
                return null;
            }

            TEntity? entity = await GetEntityAsync(udi.Guid, cancellationToken).ConfigureAwait(false);
            return entity == null ? null : await GetArtifactAsync(udi, entity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(
            GuidUdi udi,
            string selector,
            CancellationToken cancellationToken = default)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(udi, selector);
                return new NamedUdiRange(udi, OpenUdiName, selector);
            }

            TEntity? entity = await GetEntityAsync(udi.Guid, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));
            }

            return new NamedUdiRange(GetEntityUdi(entity), GetEntityName(entity), selector);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(
            string entityType,
            string sid,
            string selector,
            CancellationToken cancellationToken = default)
        {
            if (sid == "-1")
            {
                EnsureOpenSelector(selector);
                return new NamedUdiRange(Udi.Create(UdiEntityType), OpenUdiName, selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
            {
                throw new ArgumentException("Invalid identifier.", nameof(sid));
            }

            TEntity? entity = await GetEntityAsync(result, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));
            }

            return new NamedUdiRange(GetEntityUdi(entity), GetEntityName(entity), selector);
        }

        public override async IAsyncEnumerable<GuidUdi?> ExpandRangeAsync(
            UdiRange range,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureType(range.Udi);
            if (IsDisabled)
            {
                yield break;
            }

            if (range.Udi.IsRoot)
            {
                EnsureSelector(range.Udi, range.Selector);

                await foreach (TEntity entity in GetEntitiesAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield return GetEntityUdi(entity);
                }
            }
            else
            {
                TEntity? entity = await GetEntityAsync(((GuidUdi)range.Udi).Guid, cancellationToken).ConfigureAwait(false);
                if (entity == null)
                {
                    yield break;
                }

                if (range.Selector != "this")
                {
                    throw new NotSupportedException("Unexpected selector \"" + range.Selector + "\".");
                }

                yield return GetEntityUdi(entity);
            }
        }

        public override async Task<ArtifactDeployState<TArtifact, TEntity>> ProcessInitAsync(
            TArtifact artifact,
            IDeployContext context,
            CancellationToken cancellationToken = default)
        {
            EnsureType(artifact.Udi);

            TEntity? entity = await GetEntityAsync(artifact.Udi.Guid, cancellationToken).ConfigureAwait(false);

            return ArtifactDeployState.Create(artifact, entity, this, ProcessPasses[0]);
        }
    }
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/SeoToolkit.Umbraco.Deploy/SeoToolkit.Umbraco.Deploy.csproj`
Expected: Build succeeded. If `ServiceConnectorBase` abstract member signatures differ (e.g. `ExpandRangeAsync` return type or `OpenUdiName` being abstract), open `~/Source/github/umbraco/Umbraco-Deploy/src/Umbraco.Deploy.Infrastructure/Connectors/ServiceConnectors/ServiceConnectorBase.cs` and match exactly — the Commerce base above compiled against the same 18.x API, so it is the authority.

- [ ] **Step 3: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy
git commit -m "feat(deploy): add SeoToolkit entity service connector base"
```

---

### Task 4: SEO setting (enable-toggle) connector

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/Models/SeoSettingModel.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/SeoSettingArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitSeoSettingServiceConnector.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SeoSettingConnectorTests.cs`

**Interfaces:**
- Consumes: `ISeoSettingsService` (`SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService`): `Dictionary<Guid, bool> GetAll()`, `void ToggleSeoSettings(Guid contentTypeId, bool value)`; `IContentTypeService.Get(Guid)`; base class from Task 3.
- Produces:
  - `SeoSettingModel(Guid ContentTypeKey, bool Enabled)` (record)
  - `SeoSettingArtifact : DeployArtifactBase<GuidUdi>` with `bool Enabled { get; set; }`
  - `SeoToolkitSeoSettingServiceConnector` — `[UdiDefinition("seotoolkit-seo-setting", UdiType.GuidUdi)]`, `ProcessPasses = [2]`

- [ ] **Step 1: Write the failing tests**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SeoSettingConnectorTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SeoSettingConnectorTests
    {
        private Mock<ISeoSettingsService> _seoSettingsService = null!;
        private Mock<IContentTypeService> _contentTypeService = null!;
        private SeoToolkitSeoSettingServiceConnector _connector = null!;

        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [SetUp]
        public void SetUp()
        {
            _seoSettingsService = new Mock<ISeoSettingsService>();
            _contentTypeService = new Mock<IContentTypeService>();
            _connector = new SeoToolkitSeoSettingServiceConnector(
                _seoSettingsService.Object, _contentTypeService.Object, DefaultSettings());
        }

        [Test]
        public async Task GetArtifact_BuildsEnabledFlagAndDocTypeDependency()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Name).Returns("Home Page");
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            _seoSettingsService.Setup(s => s.GetAll()).Returns(new Dictionary<Guid, bool> { [contentTypeKey] = true });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, contentTypeKey);
            var artifact = await _connector.GetArtifactAsync(udi, new PassThroughCache());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.Enabled, Is.True);
                Assert.That(artifact.Udi, Is.EqualTo(udi));
                Assert.That(artifact.Dependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(Constants.UdiEntityType.DocumentType, contentTypeKey)));
            });
        }

        [Test]
        public async Task Process_Pass2_TogglesSeoSettings()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.SeoSettingArtifact(udi) { Enabled = true, Name = "Home Page" };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            _seoSettingsService.Verify(s => s.ToggleSeoSettings(contentTypeKey, true), Times.Once);
        }

        [Test]
        public async Task Process_Pass2_MissingContentType_SkipsWithoutThrowing()
        {
            var contentTypeKey = Guid.NewGuid();
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns((IContentType?)null);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.SeoSettingArtifact(udi) { Enabled = true, Name = "Gone" };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            Assert.DoesNotThrowAsync(() => _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2));
            _seoSettingsService.Verify(s => s.ToggleSeoSettings(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
```

Note: the connector constructor is `(ISeoSettingsService, IContentTypeService, IOptionsMonitor<SeoToolkitDeploySettings>)` — no logger. If `ServiceConnectorBase` demands one via its own constructor, add it and pass `NullLogger<T>.Instance` from `Microsoft.Extensions.Logging.Abstractions` in tests.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~SeoSettingConnectorTests"`
Expected: FAIL to compile — `SeoToolkitSeoSettingServiceConnector` / `SeoSettingArtifact` do not exist.

- [ ] **Step 3: Implement model, artifact, connector**

Create `src/SeoToolkit.Umbraco.Deploy/Models/SeoSettingModel.cs`:

```csharp
namespace SeoToolkit.Umbraco.Deploy.Models
{
    public record SeoSettingModel(Guid ContentTypeKey, bool Enabled, string ContentTypeName);
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/SeoSettingArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class SeoSettingArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public bool Enabled { get; set; }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitSeoSettingServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, UdiType.GuidUdi)]
    public class SeoToolkitSeoSettingServiceConnector(
        ISeoSettingsService seoSettingsService,
        IContentTypeService contentTypeService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<SeoSettingArtifact, SeoSettingModel>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.SeoSetting;

        public override string OpenUdiName => "All SeoToolkit SEO settings";

        public override int[] ProcessPasses => [2];

        public override string GetEntityName(SeoSettingModel entity) => entity.ContentTypeName;

        protected override GuidUdi GetEntityUdi(SeoSettingModel entity)
            => new(UdiEntityType, entity.ContentTypeKey);

        public override Task<SeoSettingModel?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!seoSettingsService.GetAll().TryGetValue(id, out var enabled))
            {
                return Task.FromResult<SeoSettingModel?>(null);
            }

            var contentType = contentTypeService.Get(id);
            return Task.FromResult<SeoSettingModel?>(
                contentType is null ? null : new SeoSettingModel(id, enabled, contentType.Name ?? contentType.Alias));
        }

        public override async IAsyncEnumerable<SeoSettingModel> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var (key, _) in seoSettingsService.GetAll())
            {
                var entity = await GetEntityAsync(key, cancellationToken).ConfigureAwait(false);
                if (entity is not null)
                {
                    yield return entity;
                }
            }
        }

        public override Task<SeoSettingArtifact?> GetArtifactAsync(
            GuidUdi? udi, SeoSettingModel? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<SeoSettingArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.DocumentType, entity.ContentTypeKey)),
            };

            return Task.FromResult<SeoSettingArtifact?>(new SeoSettingArtifact(udi, dependencies)
            {
                Name = entity.ContentTypeName,
                Enabled = entity.Enabled,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<SeoSettingArtifact, SeoSettingModel> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var contentTypeKey = state.Artifact.Udi.Guid;
            if (contentTypeService.Get(contentTypeKey) is null)
            {
                // Target environment doesn't have the document type; skip rather than fail the deploy.
                return Task.CompletedTask;
            }

            seoSettingsService.ToggleSeoSettings(contentTypeKey, state.Artifact.Enabled);
            return Task.CompletedTask;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~SeoSettingConnectorTests"`
Expected: PASS (3 tests). If `PassThroughCache` is not public in `Umbraco.Cms.Core.Deploy`, use `Mock.Of<IContextCache>()` instead.

- [ ] **Step 5: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): add SEO setting toggle connector"
```

---

### Task 5: Sitemap page-type connector

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/SitemapPageTypeArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitSitemapPageTypeServiceConnector.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SitemapPageTypeConnectorTests.cs`

**Interfaces:**
- Consumes: `ISitemapService` (`SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService`): `SitemapPageSettings? GetPageTypeSettings(Guid)`, `SitemapPageSettings[] GetAll()`, `void SetPageTypeSettings(SitemapPageSettings)`; `SitemapPageSettings { Guid ContentTypeGuid; bool HideFromSitemap; string ChangeFrequency; double? Priority }`; `IContentTypeService`; base from Task 3.
- Produces: `SitemapPageTypeArtifact : DeployArtifactBase<GuidUdi>` with `bool HideFromSitemap`, `string? ChangeFrequency`, `double? Priority`; connector `[UdiDefinition("seotoolkit-sitemap-page-type", UdiType.GuidUdi)]`, `ProcessPasses = [2]`.

- [ ] **Step 1: Write the failing tests**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SitemapPageTypeConnectorTests.cs` (same fixture shape as Task 4 — `Mock<ISitemapService>`, `Mock<IContentTypeService>`, default settings monitor):

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SitemapPageTypeConnectorTests
    {
        private Mock<ISitemapService> _sitemapService = null!;
        private Mock<IContentTypeService> _contentTypeService = null!;
        private SeoToolkitSitemapPageTypeServiceConnector _connector = null!;

        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [SetUp]
        public void SetUp()
        {
            _sitemapService = new Mock<ISitemapService>();
            _contentTypeService = new Mock<IContentTypeService>();
            _connector = new SeoToolkitSitemapPageTypeServiceConnector(
                _sitemapService.Object, _contentTypeService.Object, DefaultSettings());
        }

        [Test]
        public async Task RoundTrip_PreservesAllSitemapPageTypeFields()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Name).Returns("News Page");
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            _sitemapService.Setup(s => s.GetPageTypeSettings(contentTypeKey)).Returns(new SitemapPageSettings
            {
                ContentTypeGuid = contentTypeKey,
                HideFromSitemap = true,
                ChangeFrequency = "weekly",
                Priority = 0.4,
            });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, contentTypeKey);
            var artifact = await _connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());
            Assert.That(artifact, Is.Not.Null);

            SitemapPageSettings? saved = null;
            _sitemapService.Setup(s => s.SetPageTypeSettings(It.IsAny<SitemapPageSettings>()))
                .Callback<SitemapPageSettings>(s => saved = s);

            var state = await _connector.ProcessInitAsync(artifact!, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.ContentTypeGuid, Is.EqualTo(contentTypeKey));
                Assert.That(saved.HideFromSitemap, Is.True);
                Assert.That(saved.ChangeFrequency, Is.EqualTo("weekly"));
                Assert.That(saved.Priority, Is.EqualTo(0.4));
            });
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~SitemapPageTypeConnectorTests"`
Expected: FAIL to compile.

- [ ] **Step 3: Implement artifact + connector**

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/SitemapPageTypeArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class SitemapPageTypeArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public bool HideFromSitemap { get; set; }

        public string? ChangeFrequency { get; set; }

        public double? Priority { get; set; }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitSitemapPageTypeServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, UdiType.GuidUdi)]
    public class SeoToolkitSitemapPageTypeServiceConnector(
        ISitemapService sitemapService,
        IContentTypeService contentTypeService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<SitemapPageTypeArtifact, SitemapPageSettings>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.SitemapPageType;

        public override string OpenUdiName => "All SeoToolkit sitemap page type settings";

        public override int[] ProcessPasses => [2];

        public override string GetEntityName(SitemapPageSettings entity)
            => contentTypeService.Get(entity.ContentTypeGuid)?.Name ?? entity.ContentTypeGuid.ToString();

        protected override GuidUdi GetEntityUdi(SitemapPageSettings entity)
            => new(UdiEntityType, entity.ContentTypeGuid);

        public override Task<SitemapPageSettings?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(sitemapService.GetPageTypeSettings(id));

        public override async IAsyncEnumerable<SitemapPageSettings> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in sitemapService.GetAll())
            {
                yield return entity;
            }
        }

        public override Task<SitemapPageTypeArtifact?> GetArtifactAsync(
            GuidUdi? udi, SitemapPageSettings? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<SitemapPageTypeArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.DocumentType, entity.ContentTypeGuid)),
            };

            return Task.FromResult<SitemapPageTypeArtifact?>(new SitemapPageTypeArtifact(udi, dependencies)
            {
                Name = GetEntityName(entity),
                HideFromSitemap = entity.HideFromSitemap,
                ChangeFrequency = entity.ChangeFrequency,
                Priority = entity.Priority,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<SitemapPageTypeArtifact, SitemapPageSettings> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var contentTypeKey = state.Artifact.Udi.Guid;
            if (contentTypeService.Get(contentTypeKey) is null)
            {
                return Task.CompletedTask;
            }

            sitemapService.SetPageTypeSettings(new SitemapPageSettings
            {
                ContentTypeGuid = contentTypeKey,
                HideFromSitemap = state.Artifact.HideFromSitemap,
                ChangeFrequency = state.Artifact.ChangeFrequency ?? string.Empty,
                Priority = state.Artifact.Priority,
            });
            return Task.CompletedTask;
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~SitemapPageTypeConnectorTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): add sitemap page-type settings connector"
```

---

### Task 6: MetaFields document-type settings connector (with UDI extraction)

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/UdiJsonHelper.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/MetaFieldsSettingArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitMetaFieldsSettingServiceConnector.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/UdiJsonHelperTests.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/MetaFieldsSettingConnectorTests.cs`

**Interfaces:**
- Consumes: `IMetaFieldsSettingsService` (`SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings`): `DocumentTypeSettingsDto? Get(Guid)`, `DocumentTypeSettingsDto[] GetAll()`, `void Set(DocumentTypeSettingsDto)`; `DocumentTypeSettingsDto { IContentType Content; Dictionary<ISeoField, DocumentTypeValueDto> Fields; IContentType Inheritance }`; `DocumentTypeValueDto { object Value; bool UseInheritedValue }`; `SeoFieldCollection.Get(string alias)` → `ISeoField` (has `Alias`, `Editor.ValueConverter.ConvertDatabaseToObject(object)`); `IContentTypeService`. Serialization mirrors `src/SeoToolkit.Umbraco.uSync/Serializers/MetaFieldsSerializer.cs:107-158`.
- Produces:
  - `static class UdiJsonHelper { static IEnumerable<Udi> FindUdis(string? json) }` — regex-scans a JSON string for `umb://document/…`, `umb://media/…`, `umb://member/…` UDIs (used again by Task 9).
  - `MetaFieldsSettingArtifact : DeployArtifactBase<GuidUdi>` with `GuidUdi? InheritanceUdi { get; set; }` and `List<MetaFieldsSettingField> Fields { get; set; }` where `MetaFieldsSettingField { string Alias; bool UseInheritedValue; string? Value }` (Value = Newtonsoft-serialized JSON).
  - Connector `[UdiDefinition("seotoolkit-metafields-setting", UdiType.GuidUdi)]`, `ProcessPasses = [2]`.

- [ ] **Step 1: Write the failing UdiJsonHelper test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/UdiJsonHelperTests.cs`:

```csharp
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class UdiJsonHelperTests
    {
        [Test]
        public void FindUdis_FindsDocumentAndMediaUdis_IgnoresDuplicatesAndGarbage()
        {
            var json = """
                {"image":"umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11",
                 "link":"umb://document/2b7e8d0aa4514f7e9d3c1e5f6a7b8c92",
                 "again":"umb://document/2b7e8d0aa4514f7e9d3c1e5f6a7b8c92",
                 "text":"not a udi umb://document/xyz"}
                """;

            var udis = UdiJsonHelper.FindUdis(json).ToArray();

            Assert.That(udis, Has.Length.EqualTo(2));
            Assert.That(udis.Select(u => u.ToString()), Does.Contain("umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11"));
        }

        [Test]
        public void FindUdis_NullOrEmpty_ReturnsEmpty()
        {
            Assert.That(UdiJsonHelper.FindUdis(null), Is.Empty);
            Assert.That(UdiJsonHelper.FindUdis(""), Is.Empty);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~UdiJsonHelperTests"`
Expected: FAIL to compile — `UdiJsonHelper` does not exist.

- [ ] **Step 3: Implement UdiJsonHelper**

Create `src/SeoToolkit.Umbraco.Deploy/UdiJsonHelper.cs`:

```csharp
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace SeoToolkit.Umbraco.Deploy
{
    public static partial class UdiJsonHelper
    {
        [GeneratedRegex(@"umb://(document|media|member)/([0-9a-fA-F]{32})")]
        private static partial Regex UdiRegex();

        /// <summary>
        /// Scans a JSON blob for embedded document/media/member UDIs so they can be added
        /// as artifact dependencies. Returns distinct UDIs.
        /// </summary>
        public static IEnumerable<Udi> FindUdis(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                yield break;
            }

            var seen = new HashSet<string>();
            foreach (Match match in UdiRegex().Matches(json))
            {
                if (seen.Add(match.Value) && UdiParser.TryParse(match.Value, out Udi? udi) && udi is not null)
                {
                    yield return udi;
                }
            }
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~UdiJsonHelperTests"`
Expected: PASS (2 tests).

- [ ] **Step 5: Write the failing connector test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/MetaFieldsSettingConnectorTests.cs`. Mocking note: `ISeoField` and its `Editor.ValueConverter` chain are interfaces in `SeoToolkit.Umbraco.MetaFields.Core` (`Interfaces/ISeoField.cs`, editor/converter interfaces alongside) — check the exact interface names with `grep -rn "interface ISeoField" src/SeoToolkit.Umbraco.MetaFields.Core` before writing mocks; the shape used below (`field.Editor.ValueConverter.ConvertDatabaseToObject(object)`) is taken from the uSync serializer.

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class MetaFieldsSettingConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        private static Mock<ISeoField> MakeField(string alias)
        {
            var field = new Mock<ISeoField>();
            field.SetupGet(f => f.Alias).Returns(alias);
            // Value converter that round-trips the raw value unchanged:
            field.SetupGet(f => f.Editor.ValueConverter.ConvertDatabaseToObject(It.IsAny<object>()))
                 .Returns((object o) => o); // adjust to Setup(...) on the method if property-chain setup fails
            return field;
        }

        [Test]
        public async Task GetArtifact_SerializesFieldsAndExtractsUdiDependencies()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Name).Returns("Article");
            contentType.SetupGet(c => c.Alias).Returns("article");

            var field = MakeField("title");
            var dto = new DocumentTypeSettingsDto
            {
                Content = contentType.Object,
                Fields = new Dictionary<ISeoField, DocumentTypeValueDto>
                {
                    [field.Object] = new DocumentTypeValueDto
                    {
                        UseInheritedValue = false,
                        Value = "umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11",
                    },
                },
            };

            var settingsService = new Mock<IMetaFieldsSettingsService>();
            settingsService.Setup(s => s.Get(contentTypeKey)).Returns(dto);
            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            var fieldCollection = new SeoFieldCollection(() => new[] { field.Object });

            var connector = new SeoToolkitMetaFieldsSettingServiceConnector(
                settingsService.Object, contentTypeService.Object, fieldCollection, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, contentTypeKey);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.Fields, Has.Count.EqualTo(1));
                Assert.That(artifact.Fields[0].Alias, Is.EqualTo("title"));
                Assert.That(artifact.Fields[0].Value, Does.Contain("umb://media/"));
                Assert.That(artifact.Dependencies.Select(d => d.Udi.EntityType),
                    Does.Contain(Constants.UdiEntityType.Media));
                Assert.That(artifact.Dependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(Constants.UdiEntityType.DocumentType, contentTypeKey)));
            });
        }

        [Test]
        public async Task Process_Pass2_RebuildsDtoAndSaves()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Alias).Returns("article");

            var field = MakeField("title");
            var settingsService = new Mock<IMetaFieldsSettingsService>();
            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            var fieldCollection = new SeoFieldCollection(() => new[] { field.Object });

            var connector = new SeoToolkitMetaFieldsSettingServiceConnector(
                settingsService.Object, contentTypeService.Object, fieldCollection, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsSettingArtifact(udi)
            {
                Name = "Article",
                Fields =
                [
                    new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsSettingField
                    {
                        Alias = "title",
                        UseInheritedValue = false,
                        Value = "\"Hello\"",
                    },
                ],
            };

            DocumentTypeSettingsDto? saved = null;
            settingsService.Setup(s => s.Set(It.IsAny<DocumentTypeSettingsDto>()))
                .Callback<DocumentTypeSettingsDto>(d => saved = d);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Content.Key, Is.EqualTo(contentTypeKey));
                Assert.That(saved.Fields, Has.Count.EqualTo(1));
                Assert.That(saved.Fields.Single().Value.Value, Is.EqualTo("Hello"));
            });
        }
    }
}
```

Note: if `DocumentTypeSettingsDto` namespace differs, it is at `src/SeoToolkit.Umbraco.MetaFields.Core/Models/MetaFieldsSettings/Business/DocumentTypeSettingsDto.cs` — check the namespace declaration in that file (uSync imports `...Models.DocumentTypeSettings.Business`, so the folder and namespace may not match; use the namespace from the file). Also `SeoFieldCollection` constructor takes `Func<IEnumerable<ISeoField>>` (`Collections/SeoFieldCollection.cs:13`).

- [ ] **Step 6: Run tests to verify they fail**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~MetaFieldsSettingConnectorTests"`
Expected: FAIL to compile.

- [ ] **Step 7: Implement artifact + connector**

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/MetaFieldsSettingArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class MetaFieldsSettingArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public GuidUdi? InheritanceUdi { get; set; }

        public List<MetaFieldsSettingField> Fields { get; set; } = [];
    }

    public class MetaFieldsSettingField
    {
        public required string Alias { get; set; }

        public bool UseInheritedValue { get; set; }

        /// <summary>Newtonsoft-serialized JSON of the field value (same wire format as the uSync serializer).</summary>
        public string? Value { get; set; }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitMetaFieldsSettingServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, UdiType.GuidUdi)]
    public class SeoToolkitMetaFieldsSettingServiceConnector(
        IMetaFieldsSettingsService metaFieldsSettingsService,
        IContentTypeService contentTypeService,
        SeoFieldCollection seoFieldCollection,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<MetaFieldsSettingArtifact, DocumentTypeSettingsDto>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting;

        public override string OpenUdiName => "All SeoToolkit meta field settings";

        public override int[] ProcessPasses => [2];

        public override string GetEntityName(DocumentTypeSettingsDto entity)
            => entity.Content.Name ?? entity.Content.Alias;

        protected override GuidUdi GetEntityUdi(DocumentTypeSettingsDto entity)
            => new(UdiEntityType, entity.Content.Key);

        public override Task<DocumentTypeSettingsDto?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(metaFieldsSettingsService.Get(id));

        public override async IAsyncEnumerable<DocumentTypeSettingsDto> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in metaFieldsSettingsService.GetAll())
            {
                yield return entity;
            }
        }

        public override Task<MetaFieldsSettingArtifact?> GetArtifactAsync(
            GuidUdi? udi, DocumentTypeSettingsDto? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<MetaFieldsSettingArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.DocumentType, entity.Content.Key)),
            };

            GuidUdi? inheritanceUdi = null;
            if (entity.Inheritance is not null)
            {
                inheritanceUdi = new GuidUdi(UdiEntityType, entity.Inheritance.Key);
                dependencies.Add(new SeoToolkitArtifactDependency(inheritanceUdi));
            }

            var fields = new List<MetaFieldsSettingField>();
            foreach (var (seoField, valueDto) in entity.Fields)
            {
                var json = valueDto.Value is null ? null : JsonConvert.SerializeObject(valueDto.Value);
                foreach (var referencedUdi in UdiJsonHelper.FindUdis(json))
                {
                    dependencies.Add(new SeoToolkitArtifactDependency(referencedUdi));
                }

                fields.Add(new MetaFieldsSettingField
                {
                    Alias = seoField.Alias,
                    UseInheritedValue = valueDto.UseInheritedValue,
                    Value = json,
                });
            }

            return Task.FromResult<MetaFieldsSettingArtifact?>(new MetaFieldsSettingArtifact(udi, dependencies)
            {
                Name = GetEntityName(entity),
                InheritanceUdi = inheritanceUdi,
                Fields = fields,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<MetaFieldsSettingArtifact, DocumentTypeSettingsDto> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var contentType = contentTypeService.Get(state.Artifact.Udi.Guid);
            if (contentType is null)
            {
                return Task.CompletedTask;
            }

            var dto = state.Entity ?? new DocumentTypeSettingsDto { Content = contentType };
            dto.Content = contentType;

            if (state.Artifact.InheritanceUdi is not null)
            {
                dto.Inheritance = contentTypeService.Get(state.Artifact.InheritanceUdi.Guid);
            }

            foreach (var field in state.Artifact.Fields)
            {
                var seoField = seoFieldCollection.Get(field.Alias);
                if (seoField is null)
                {
                    continue; // field type not installed in target; skip
                }

                var valueDto = new DocumentTypeValueDto { UseInheritedValue = field.UseInheritedValue };
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    valueDto.Value = seoField.Editor.ValueConverter.ConvertDatabaseToObject(
                        JsonConvert.DeserializeObject(field.Value));
                }

                if (!dto.Fields.TryAdd(seoField, valueDto))
                {
                    dto.Fields[seoField] = valueDto;
                }
            }

            metaFieldsSettingsService.Set(dto);
            return Task.CompletedTask;
        }
    }
}
```

Note: `DocumentTypeSettingsDto.Fields` initialization — if the DTO does not initialize `Fields` in its constructor, initialize it when creating the new DTO (check the DTO source at `src/SeoToolkit.Umbraco.MetaFields.Core/Models/MetaFieldsSettings/Business/DocumentTypeSettingsDto.cs`). Import the namespace exactly as declared in that file.

- [ ] **Step 8: Run tests to verify they pass**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~MetaFieldsSettingConnectorTests"`
Expected: PASS (2 tests). Moq cannot set up chained property gets on interfaces unless each level is an interface property — if `MakeField` fails at runtime, set up intermediate mocks explicitly (`Mock<ISeoFieldEditor>`, `Mock<ISeoValueConverter>` — check actual interface names in `SeoToolkit.Umbraco.MetaFields.Core/Interfaces/`).

- [ ] **Step 9: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): add meta fields document-type settings connector"
```

---

### Task 7: Script Manager connector

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/ScriptArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitScriptServiceConnector.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/ScriptConnectorTests.cs`

**Interfaces:**
- Consumes: `IScriptManagerService` (`SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services`): `Script? Get(Guid)`, `IEnumerable<Script> GetAll(Guid? domainId)`, `Script Save(Script)`; `Script { Guid? Key; string Name; IScriptDefinition Definition; Dictionary<string,string> Config; Guid? DomainId; int SortOrder }`; `ScriptDefinitionCollection` (`...ScriptManager.Core.Collections`, `BuilderCollectionBase<IScriptDefinition>`; find by `.FirstOrDefault(d => d.Alias == alias)` — it has no `Get(alias)` helper); `IScriptDefinition { string Name; string Alias; ... }`.
- Produces: `ScriptArtifact : DeployArtifactBase<GuidUdi>` with `string DefinitionAlias`, `Dictionary<string,string> Config`, `GuidUdi? DomainCollectionUdi`, `int SortOrder`; connector `[UdiDefinition("seotoolkit-script", UdiType.GuidUdi)]`, `ProcessPasses = [2]`.

Note: `Script.DomainId` is the **SeoToolkit domain collection GUID** (portable across environments), not an Umbraco domain int — it maps directly to a `seotoolkit-domain-collection` UDI dependency (connector for that type arrives in Task 8; the string constant already exists).

- [ ] **Step 1: Write the failing tests**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/ScriptConnectorTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class ScriptConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [Test]
        public async Task RoundTrip_PreservesScriptFieldsAndResolvesDefinition()
        {
            var scriptKey = Guid.NewGuid();
            var domainCollectionId = Guid.NewGuid();
            var definition = new Mock<IScriptDefinition>();
            definition.SetupGet(d => d.Alias).Returns("googleAnalytics");

            var scriptService = new Mock<IScriptManagerService>();
            scriptService.Setup(s => s.Get(scriptKey)).Returns(new Script
            {
                Key = scriptKey,
                Name = "GA4",
                Definition = definition.Object,
                Config = new Dictionary<string, string> { ["measurementId"] = "G-123" },
                DomainId = domainCollectionId,
                SortOrder = 3,
            });

            // ScriptDefinitionCollection ctor: (Func<IEnumerable<IScriptDefinition>>, ISettingsService<ScriptManagerConfigModel>)
            // — check src/SeoToolkit.Umbraco.ScriptManager.Core/Collections/ScriptDefinitionCollection.cs and mock the
            // settings service argument with Moq.
            var definitions = CreateDefinitionCollection(definition.Object);

            var connector = new SeoToolkitScriptServiceConnector(scriptService.Object, definitions, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.Script, scriptKey);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.DefinitionAlias, Is.EqualTo("googleAnalytics"));
                Assert.That(artifact.Config["measurementId"], Is.EqualTo("G-123"));
                Assert.That(artifact.SortOrder, Is.EqualTo(3));
                Assert.That(artifact.DomainCollectionUdi,
                    Is.EqualTo(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, domainCollectionId)));
                Assert.That(artifact.Dependencies.Select(d => d.Udi), Does.Contain(artifact.DomainCollectionUdi));
            });

            Script? saved = null;
            scriptService.Setup(s => s.Save(It.IsAny<Script>())).Returns<Script>(s => { saved = s; return s; });

            var state = await connector.ProcessInitAsync(artifact!, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Key, Is.EqualTo(scriptKey));
                Assert.That(saved.Definition.Alias, Is.EqualTo("googleAnalytics"));
                Assert.That(saved.DomainId, Is.EqualTo(domainCollectionId));
            });
        }

        private static ScriptDefinitionCollection CreateDefinitionCollection(params IScriptDefinition[] items)
        {
            // Implement per the actual ScriptDefinitionCollection constructor (it takes a
            // Func<IEnumerable<IScriptDefinition>> plus ISettingsService<ScriptManagerConfigModel>).
            var settingsService = new Mock<SeoToolkit.Umbraco.Common.Core.Services.SettingsService
                .ISettingsService<SeoToolkit.Umbraco.ScriptManager.Core.Config.Models.ScriptManagerConfigModel>>();
            return new ScriptDefinitionCollection(() => items, settingsService.Object);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~ScriptConnectorTests"`
Expected: FAIL to compile.

- [ ] **Step 3: Implement artifact + connector**

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/ScriptArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class ScriptArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public required string DefinitionAlias { get; set; }

        public Dictionary<string, string> Config { get; set; } = [];

        public GuidUdi? DomainCollectionUdi { get; set; }

        public int SortOrder { get; set; }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitScriptServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.Script, UdiType.GuidUdi)]
    public class SeoToolkitScriptServiceConnector(
        IScriptManagerService scriptManagerService,
        ScriptDefinitionCollection scriptDefinitions,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<ScriptArtifact, Script>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.Script;

        public override string OpenUdiName => "All SeoToolkit scripts";

        public override int[] ProcessPasses => [2];

        public override string GetEntityName(Script entity) => entity.Name;

        protected override GuidUdi GetEntityUdi(Script entity)
            => new(UdiEntityType, entity.Key ?? Guid.Empty);

        public override Task<Script?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(scriptManagerService.Get(id));

        public override async IAsyncEnumerable<Script> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            // GetAll is per-domain; enumerate the no-domain scripts plus every known script by key.
            foreach (var script in scriptManagerService.GetAll(null))
            {
                yield return script;
            }
        }

        public override Task<ScriptArtifact?> GetArtifactAsync(
            GuidUdi? udi, Script? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<ScriptArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection();
            GuidUdi? domainCollectionUdi = null;
            if (entity.DomainId is not null)
            {
                domainCollectionUdi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, entity.DomainId.Value);
                dependencies.Add(new SeoToolkitArtifactDependency(domainCollectionUdi));
            }

            return Task.FromResult<ScriptArtifact?>(new ScriptArtifact(udi, dependencies)
            {
                Name = entity.Name,
                DefinitionAlias = entity.Definition.Alias,
                Config = entity.Config ?? [],
                DomainCollectionUdi = domainCollectionUdi,
                SortOrder = entity.SortOrder,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<ScriptArtifact, Script> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var definition = scriptDefinitions.FirstOrDefault(d => d.Alias == state.Artifact.DefinitionAlias);
            if (definition is null)
            {
                return Task.CompletedTask; // script definition type not installed in target; skip
            }

            var script = state.Entity ?? new Script();
            script.Key = state.Artifact.Udi.Guid;
            script.Name = state.Artifact.Name;
            script.Definition = definition;
            script.Config = state.Artifact.Config;
            script.DomainId = state.Artifact.DomainCollectionUdi?.Guid;
            script.SortOrder = state.Artifact.SortOrder;

            scriptManagerService.Save(script);
            return Task.CompletedTask;
        }
    }
}
```

Caveat for the implementer: `ScriptManagerService.Save` treats `Key != null` as an update (`ScriptManagerService.cs:34-53`) — a transferred script that doesn't exist yet in the target arrives with a non-null `Key`, and `Update` on a missing row will no-op or fail. Check `ScriptRepository.Update`; if it cannot upsert, detect existence first (`state.Entity is null`) and call the repository/`Save` accordingly — if `Save` can't insert with a preset Key, extend `IScriptRepository`/`Save` minimally in `ScriptManager.Core` (additive only) so an insert with a caller-supplied Key is possible, and cover it with a test.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~ScriptConnectorTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): add script manager connector"
```

---

### Task 8: Domain collection + key/values connectors

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/DomainCollectionArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/KeyValuesArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Models/KeyValuesModel.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitDomainCollectionServiceConnector.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitKeyValuesServiceConnector.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/DomainCollectionConnectorTests.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/KeyValuesConnectorTests.cs`

**Interfaces:**
- Consumes:
  - `ISeoDomainsService` (`SeoToolkit.Umbraco.Common.Core.Services.Domains`): `SeoDomainCollection[] GetAll()`, `SeoDomainCollection? Get(Guid)`, `Guid Save(SeoDomainCollection)`, `void Delete(Guid)`; `SeoDomainCollection { Guid? Id; string Name; List<int> DomainIds; Dictionary<string,string> Settings }`.
  - `IDomainService` (`Umbraco.Cms.Core.Services`) to map Umbraco domain int ids ⇄ domain names (`GetAllAsync(true)` → `IDomain { Id, DomainName, RootContentId }`).
  - `ISeoKeyValueRepository` (`SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository`): `Dictionary<string,string> Get(Guid? domainId)`, `void Set(string key, string value, Guid? domainId)`, `void Delete(string key, Guid? domainId)`. (The `ISeoKeyValueService` only exposes single-key read, so the connector uses the repository.)
- Produces:
  - `DomainCollectionArtifact : DeployArtifactBase<GuidUdi>` with `List<string> DomainNames`, `Dictionary<string,string> Settings`.
  - `KeyValuesModel(Guid ArtifactGuid, Guid? DomainCollectionId, Dictionary<string,string> Values)` (record) — `ArtifactGuid` is `SeoToolkitDeployConstants.RootKeyValuesGuid` when `DomainCollectionId` is null, else the collection id.
  - `KeyValuesArtifact : DeployArtifactBase<GuidUdi>` with `GuidUdi? DomainCollectionUdi`, `Dictionary<string,string> Values`.
  - Connectors: `[UdiDefinition("seotoolkit-domain-collection", ...)]` and `[UdiDefinition("seotoolkit-key-values", ...)]`, both `ProcessPasses = [2]`.

Portability note: `SeoDomainCollection.DomainIds` holds **Umbraco int domain ids** — the only non-portable identifiers in scope. The artifact serializes them as domain names; import resolves names back to ids via `IDomainService` and silently drops names that don't exist in the target (environment-specific hostnames are expected to differ).

- [ ] **Step 1: Write the failing DomainCollection test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/DomainCollectionConnectorTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class DomainCollectionConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        private static IDomain MakeDomain(int id, string name)
        {
            var domain = new Mock<IDomain>();
            domain.SetupGet(d => d.Id).Returns(id);
            domain.SetupGet(d => d.DomainName).Returns(name);
            return domain.Object;
        }

        [Test]
        public async Task RoundTrip_MapsDomainIdsToNamesAndBack()
        {
            var collectionId = Guid.NewGuid();
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.Get(collectionId)).Returns(new SeoDomainCollection
            {
                Id = collectionId,
                Name = "Main site",
                DomainIds = [11, 22],
                Settings = new Dictionary<string, string> { ["someSetting"] = "true" },
            });

            var domainService = new Mock<IDomainService>();
            domainService.Setup(s => s.GetAllAsync(true)).ReturnsAsync(
            [
                MakeDomain(11, "example.com"),
                MakeDomain(22, "example.co.uk"),
                MakeDomain(33, "other.com"),
            ]);

            var connector = new SeoToolkitDomainCollectionServiceConnector(
                domainsService.Object, domainService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, collectionId);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.DomainNames, Is.EquivalentTo(new[] { "example.com", "example.co.uk" }));

            SeoDomainCollection? saved = null;
            domainsService.Setup(s => s.Save(It.IsAny<SeoDomainCollection>()))
                .Callback<SeoDomainCollection>(c => saved = c).Returns(collectionId);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Id, Is.EqualTo(collectionId));
                Assert.That(saved.Name, Is.EqualTo("Main site"));
                Assert.That(saved.DomainIds, Is.EquivalentTo(new[] { 11, 22 }));
                Assert.That(saved.Settings["someSetting"], Is.EqualTo("true"));
            });
        }
    }
}
```

Note: check `IDomainService` for the exact getter — Umbraco 18 exposes `GetAllAsync(bool includeWildcards)`; if the signature differs (e.g. sync `GetAll`), match the actual CMS 18 API (`~/Source/github/umbraco/Umbraco-CMS/src/Umbraco.Core/Services/IDomainService.cs`).

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~DomainCollectionConnectorTests"`
Expected: FAIL to compile.

- [ ] **Step 3: Implement DomainCollection artifact + connector**

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/DomainCollectionArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class DomainCollectionArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        /// <summary>Umbraco domain names (portable across environments, unlike int domain ids).</summary>
        public List<string> DomainNames { get; set; } = [];

        public Dictionary<string, string> Settings { get; set; } = [];
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitDomainCollectionServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, UdiType.GuidUdi)]
    public class SeoToolkitDomainCollectionServiceConnector(
        ISeoDomainsService seoDomainsService,
        IDomainService domainService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<DomainCollectionArtifact, SeoDomainCollection>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.DomainCollection;

        public override string OpenUdiName => "All SeoToolkit domain collections";

        public override int[] ProcessPasses => [2];

        public override string GetEntityName(SeoDomainCollection entity) => entity.Name;

        protected override GuidUdi GetEntityUdi(SeoDomainCollection entity)
            => new(UdiEntityType, entity.Id ?? Guid.Empty);

        public override Task<SeoDomainCollection?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(seoDomainsService.Get(id));

        public override async IAsyncEnumerable<SeoDomainCollection> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in seoDomainsService.GetAll())
            {
                yield return entity;
            }
        }

        public override async Task<DomainCollectionArtifact?> GetArtifactAsync(
            GuidUdi? udi, SeoDomainCollection? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return null;
            }

            var allDomains = (await domainService.GetAllAsync(true).ConfigureAwait(false)).ToArray();
            var domainNames = entity.DomainIds
                .Select(id => allDomains.FirstOrDefault(d => d.Id == id)?.DomainName)
                .Where(name => name is not null)
                .Select(name => name!)
                .ToList();

            return new DomainCollectionArtifact(udi, new ArtifactDependencyCollection())
            {
                Name = entity.Name,
                DomainNames = domainNames,
                Settings = entity.Settings,
            };
        }

        public override async Task ProcessAsync(
            ArtifactDeployState<DomainCollectionArtifact, SeoDomainCollection> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return;
            }

            var allDomains = (await domainService.GetAllAsync(true).ConfigureAwait(false)).ToArray();
            var domainIds = state.Artifact.DomainNames
                .Select(name => allDomains.FirstOrDefault(d => string.Equals(d.DomainName, name, StringComparison.OrdinalIgnoreCase))?.Id)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            seoDomainsService.Save(new SeoDomainCollection
            {
                Id = state.Artifact.Udi.Guid,
                Name = state.Artifact.Name,
                DomainIds = domainIds,
                Settings = state.Artifact.Settings,
            });
        }
    }
}
```

Caveat: verify `ISeoDomainsService.Save` honours a caller-supplied `Id` for a collection that doesn't exist yet in the target (`SeoDomainsService.cs` in `Common.Core/Services/Domains/`). If it generates a new GUID on insert, extend the service/repository minimally (additive) so the GUID round-trips, and cover with a test.

- [ ] **Step 4: Run DomainCollection test to verify it passes**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~DomainCollectionConnectorTests"`
Expected: PASS.

- [ ] **Step 5: Write the failing KeyValues test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/KeyValuesConnectorTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class KeyValuesConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [Test]
        public async Task RootKeyValues_UseWellKnownGuid_AndSetOnImport()
        {
            var repository = new Mock<ISeoKeyValueRepository>();
            repository.Setup(r => r.Get((Guid?)null))
                .Returns(new Dictionary<string, string> { ["siteName"] = "My Site" });
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.GetAll()).Returns([]);

            var connector = new SeoToolkitKeyValuesServiceConnector(
                repository.Object, domainsService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.KeyValues, SeoToolkitDeployConstants.RootKeyValuesGuid);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.Values["siteName"], Is.EqualTo("My Site"));
            Assert.That(artifact.DomainCollectionUdi, Is.Null);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            repository.Verify(r => r.Set("siteName", "My Site", null), Times.Once);
        }
    }
}
```

- [ ] **Step 6: Run test to verify it fails**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~KeyValuesConnectorTests"`
Expected: FAIL to compile.

- [ ] **Step 7: Implement KeyValues model, artifact, connector**

Create `src/SeoToolkit.Umbraco.Deploy/Models/KeyValuesModel.cs`:

```csharp
namespace SeoToolkit.Umbraco.Deploy.Models
{
    /// <param name="ArtifactGuid">RootKeyValuesGuid for the no-domain set, else the domain collection id.</param>
    public record KeyValuesModel(Guid ArtifactGuid, Guid? DomainCollectionId, Dictionary<string, string> Values, string Name);
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/KeyValuesArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class KeyValuesArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public GuidUdi? DomainCollectionUdi { get; set; }

        public Dictionary<string, string> Values { get; set; } = [];
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitKeyValuesServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.KeyValues, UdiType.GuidUdi)]
    public class SeoToolkitKeyValuesServiceConnector(
        ISeoKeyValueRepository keyValueRepository,
        ISeoDomainsService seoDomainsService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<KeyValuesArtifact, KeyValuesModel>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.KeyValues;

        public override string OpenUdiName => "All SeoToolkit key/values";

        public override int[] ProcessPasses => [2];

        public override string GetEntityName(KeyValuesModel entity) => entity.Name;

        protected override GuidUdi GetEntityUdi(KeyValuesModel entity)
            => new(UdiEntityType, entity.ArtifactGuid);

        public override Task<KeyValuesModel?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == SeoToolkitDeployConstants.RootKeyValuesGuid)
            {
                var rootValues = keyValueRepository.Get((Guid?)null);
                return Task.FromResult<KeyValuesModel?>(
                    new KeyValuesModel(id, null, rootValues, "SeoToolkit key/values (global)"));
            }

            var collection = seoDomainsService.Get(id);
            if (collection is null)
            {
                return Task.FromResult<KeyValuesModel?>(null);
            }

            var values = keyValueRepository.Get(id);
            return Task.FromResult<KeyValuesModel?>(
                new KeyValuesModel(id, id, values, $"SeoToolkit key/values ({collection.Name})"));
        }

        public override async IAsyncEnumerable<KeyValuesModel> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var root = await GetEntityAsync(SeoToolkitDeployConstants.RootKeyValuesGuid, cancellationToken).ConfigureAwait(false);
            if (root is not null && root.Values.Count > 0)
            {
                yield return root;
            }

            foreach (var collection in seoDomainsService.GetAll())
            {
                if (collection.Id is null)
                {
                    continue;
                }

                var entity = await GetEntityAsync(collection.Id.Value, cancellationToken).ConfigureAwait(false);
                if (entity is not null && entity.Values.Count > 0)
                {
                    yield return entity;
                }
            }
        }

        public override Task<KeyValuesArtifact?> GetArtifactAsync(
            GuidUdi? udi, KeyValuesModel? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<KeyValuesArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection();
            GuidUdi? domainCollectionUdi = null;
            if (entity.DomainCollectionId is not null)
            {
                domainCollectionUdi = new GuidUdi(
                    SeoToolkitDeployConstants.UdiEntityType.DomainCollection, entity.DomainCollectionId.Value);
                dependencies.Add(new SeoToolkitArtifactDependency(domainCollectionUdi));
            }

            return Task.FromResult<KeyValuesArtifact?>(new KeyValuesArtifact(udi, dependencies)
            {
                Name = entity.Name,
                DomainCollectionUdi = domainCollectionUdi,
                Values = entity.Values,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<KeyValuesArtifact, KeyValuesModel> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            Guid? domainId = state.Artifact.DomainCollectionUdi?.Guid;
            foreach (var (key, value) in state.Artifact.Values)
            {
                keyValueRepository.Set(key, value, domainId);
            }

            return Task.CompletedTask;
        }
    }
}
```

- [ ] **Step 8: Run tests to verify they pass**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~KeyValuesConnectorTests|FullyQualifiedName~DomainCollectionConnectorTests"`
Expected: PASS.

- [ ] **Step 9: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): add domain collection and key/values connectors"
```

---

### Task 9: Per-node connectors — MetaFields values (with Core additions) + Sitemap content

**Files:**
- Modify: `src/SeoToolkit.Umbraco.MetaFields.Core/Repositories/SeoValueRepository/IMetaFieldsValueRepository.cs` (add 2 methods)
- Modify: `src/SeoToolkit.Umbraco.MetaFields.Core/Repositories/SeoValueRepository/MetaFieldsDatabaseRepository.cs` (implement them)
- Create: `src/SeoToolkit.Umbraco.Deploy/Models/MetaFieldsNodeValuesModel.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/MetaFieldsValueArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Artifacts/SitemapContentArtifact.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitMetaFieldsValueServiceConnector.cs`
- Create: `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitSitemapContentServiceConnector.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/MetaFieldsValueConnectorTests.cs`
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SitemapContentConnectorTests.cs`

**Interfaces:**
- Consumes: `IMetaFieldsValueRepository` (existing: `Dictionary<string, object> GetAllValues(Guid nodeId, string culture)`, `Add/Update/Exists(Guid, string alias, string culture, object value)`); `ISitemapService` (`SitemapContentSettings? GetContentSettings(Guid)`, `SitemapContentSettings[] GetAllContentSettings()`, `void SetContentSettings(SitemapContentSettings)`); `IContentService.GetById(Guid)` (`Umbraco.Cms.Core.Services`) to verify the node exists and name artifacts.
- Produces:
  - **Core additions** on `IMetaFieldsValueRepository`:
    - `Dictionary<string, Dictionary<string, object>> GetAllValues(Guid nodeId)` — culture → (alias → value); invariant culture is the empty string key.
    - `IEnumerable<Guid> GetAllNodeKeys()` — distinct node keys that have any values.
  - `MetaFieldsNodeValuesModel(Guid NodeKey, string NodeName, Dictionary<string, Dictionary<string, object>> Values)` (record)
  - `MetaFieldsValueArtifact : DeployArtifactBase<GuidUdi>` with `Dictionary<string, Dictionary<string, string?>> Values` (culture → alias → Newtonsoft JSON string)
  - `SitemapContentArtifact : DeployArtifactBase<GuidUdi>` with `bool ExcludeFromSitemap`, `string? ChangeFrequency`, `double? Priority`
  - Both connectors: `ProcessPasses = [7]`; UDI guid = node key; dependency on `new GuidUdi(Constants.UdiEntityType.Document, nodeKey)` (`Exist`).

- [ ] **Step 1: Write the failing Core repository test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/MetaFieldsValueConnectorTests.cs` (starts with the connector tests; the Core repo methods are exercised through mocks here — the repository implementation itself is straightforward NPoco and is verified by build + the existing repo patterns):

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class MetaFieldsValueConnectorTests
    {
        private Mock<IMetaFieldsValueRepository> _valueRepository = null!;
        private Mock<IContentService> _contentService = null!;
        private SeoToolkitMetaFieldsValueServiceConnector _connector = null!;

        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [SetUp]
        public void SetUp()
        {
            _valueRepository = new Mock<IMetaFieldsValueRepository>();
            _contentService = new Mock<IContentService>();
            _connector = new SeoToolkitMetaFieldsValueServiceConnector(
                _valueRepository.Object, _contentService.Object, DefaultSettings());
        }

        private IContent SetUpContent(Guid nodeKey, string name = "Some Page")
        {
            var content = new Mock<IContent>();
            content.SetupGet(c => c.Key).Returns(nodeKey);
            content.SetupGet(c => c.Name).Returns(name);
            _contentService.Setup(s => s.GetById(nodeKey)).Returns(content.Object);
            return content.Object;
        }

        [Test]
        public async Task GetArtifact_IncludesAllCulturesAndDocumentDependency()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Hello" },
                ["da-DK"] = new() { ["title"] = "Hej", ["description"] = "umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11" },
            });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = await _connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.Values, Has.Count.EqualTo(2));
                Assert.That(artifact.Values["da-DK"], Has.Count.EqualTo(2));
                Assert.That(artifact.Dependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)));
                Assert.That(artifact.Dependencies.Select(d => d.Udi.EntityType),
                    Does.Contain(Constants.UdiEntityType.Media));
            });
        }

        [Test]
        public async Task Process_Pass7_WritesValuesPerCultureViaAddOrUpdate()
        {
            var nodeKey = Guid.NewGuid();
            SetUpContent(nodeKey);
            _valueRepository.Setup(r => r.Exists(nodeKey, "title", "")).Returns(false);
            _valueRepository.Setup(r => r.Exists(nodeKey, "title", "da-DK")).Returns(true);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi)
            {
                Name = "Some Page",
                Values = new Dictionary<string, Dictionary<string, string?>>
                {
                    [""] = new() { ["title"] = "\"Hello\"" },
                    ["da-DK"] = new() { ["title"] = "\"Hej\"" },
                },
            };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            _valueRepository.Verify(r => r.Add(nodeKey, "title", "", "Hello"), Times.Once);
            _valueRepository.Verify(r => r.Update(nodeKey, "title", "da-DK", "Hej"), Times.Once);
        }

        [Test]
        public async Task Process_Pass7_MissingNode_SkipsWithoutThrowing()
        {
            var nodeKey = Guid.NewGuid();
            _contentService.Setup(s => s.GetById(nodeKey)).Returns((IContent?)null);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.MetaFieldsValueArtifact(udi)
            {
                Name = "Gone",
                Values = new Dictionary<string, Dictionary<string, string?>> { [""] = new() { ["title"] = "\"x\"" } },
            };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            Assert.DoesNotThrowAsync(() => _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7));
            _valueRepository.Verify(r => r.Add(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
```

Note on `Process_Pass7` value shape: the artifact stores each value as a Newtonsoft JSON string; on import the connector deserializes with `JsonConvert.DeserializeObject(json)` before writing, matching how `MetaFieldsDatabaseRepository` serializes on write (`UserValue = JsonConvert.SerializeObject(value)` — `MetaFieldsDatabaseRepository.cs:83`). `JsonConvert.DeserializeObject("\"Hello\"")` yields the string `"Hello"`, so `Verify(... "Hello")` passes.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~MetaFieldsValueConnectorTests"`
Expected: FAIL to compile — connector, artifact and the new `GetAllValues(Guid)` repository overload don't exist.

- [ ] **Step 3: Add the Core repository methods**

In `src/SeoToolkit.Umbraco.MetaFields.Core/Repositories/SeoValueRepository/IMetaFieldsValueRepository.cs`, after the existing `Dictionary<string, object> GetAllValues(Guid nodeId, string culture);` add:

```csharp
        /// <summary>
        /// Gets all user values for a node across all cultures. Outer key is the culture
        /// (empty string for invariant), inner key is the field alias.
        /// </summary>
        Dictionary<string, Dictionary<string, object>> GetAllValues(Guid nodeId);

        /// <summary>
        /// Gets the distinct node keys that have any user values stored.
        /// </summary>
        IEnumerable<Guid> GetAllNodeKeys();
```

In `src/SeoToolkit.Umbraco.MetaFields.Core/Repositories/SeoValueRepository/MetaFieldsDatabaseRepository.cs`, after the existing `GetAllValues(Guid nodeId, string culture)` method add:

```csharp
        public Dictionary<string, Dictionary<string, object>> GetAllValues(Guid nodeId)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<MetaFieldsValueEntity>()
                    .Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId))
                .GroupBy(it => it.Culture ?? string.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue)));
        }

        public IEnumerable<Guid> GetAllNodeKeys()
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<MetaFieldsValueEntity>())
                .Select(it => it.NodeKey)
                .Distinct()
                .ToArray();
        }
```

(Match the existing file's query style — the existing `GetAllValues(Guid, string)` omits `.SelectAll().From<…>()` on the filter-only overload; keep whichever form compiles against the NPoco version in use. `JsonConvert` is already imported in this file.)

- [ ] **Step 4: Implement models, artifacts, connectors**

Create `src/SeoToolkit.Umbraco.Deploy/Models/MetaFieldsNodeValuesModel.cs`:

```csharp
namespace SeoToolkit.Umbraco.Deploy.Models
{
    /// <param name="Values">Culture (empty string = invariant) → field alias → raw value.</param>
    public record MetaFieldsNodeValuesModel(Guid NodeKey, string NodeName, Dictionary<string, Dictionary<string, object>> Values);
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/MetaFieldsValueArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class MetaFieldsValueArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        /// <summary>Culture (empty string = invariant) → field alias → Newtonsoft-serialized JSON value.</summary>
        public Dictionary<string, Dictionary<string, string?>> Values { get; set; } = [];
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Artifacts/SitemapContentArtifact.cs`:

```csharp
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class SitemapContentArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public bool ExcludeFromSitemap { get; set; }

        public string? ChangeFrequency { get; set; }

        public double? Priority { get; set; }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitMetaFieldsValueServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, UdiType.GuidUdi)]
    public class SeoToolkitMetaFieldsValueServiceConnector(
        IMetaFieldsValueRepository valueRepository,
        IContentService contentService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<MetaFieldsValueArtifact, MetaFieldsNodeValuesModel>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue;

        public override string OpenUdiName => "All SeoToolkit meta field values";

        public override int[] ProcessPasses => [7];

        public override string GetEntityName(MetaFieldsNodeValuesModel entity) => entity.NodeName;

        protected override GuidUdi GetEntityUdi(MetaFieldsNodeValuesModel entity)
            => new(UdiEntityType, entity.NodeKey);

        public override Task<MetaFieldsNodeValuesModel?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var values = valueRepository.GetAllValues(id);
            if (values.Count == 0)
            {
                return Task.FromResult<MetaFieldsNodeValuesModel?>(null);
            }

            var content = contentService.GetById(id);
            return Task.FromResult<MetaFieldsNodeValuesModel?>(
                content is null ? null : new MetaFieldsNodeValuesModel(id, content.Name ?? id.ToString(), values));
        }

        public override async IAsyncEnumerable<MetaFieldsNodeValuesModel> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var nodeKey in valueRepository.GetAllNodeKeys())
            {
                var entity = await GetEntityAsync(nodeKey, cancellationToken).ConfigureAwait(false);
                if (entity is not null)
                {
                    yield return entity;
                }
            }
        }

        public override Task<MetaFieldsValueArtifact?> GetArtifactAsync(
            GuidUdi? udi, MetaFieldsNodeValuesModel? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<MetaFieldsValueArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.Document, entity.NodeKey)),
            };

            var values = new Dictionary<string, Dictionary<string, string?>>();
            foreach (var (culture, fields) in entity.Values)
            {
                var cultureValues = new Dictionary<string, string?>();
                foreach (var (alias, value) in fields)
                {
                    var json = value is null ? null : JsonConvert.SerializeObject(value);
                    foreach (var referencedUdi in UdiJsonHelper.FindUdis(json))
                    {
                        dependencies.Add(new SeoToolkitArtifactDependency(referencedUdi));
                    }

                    cultureValues[alias] = json;
                }

                values[culture] = cultureValues;
            }

            return Task.FromResult<MetaFieldsValueArtifact?>(new MetaFieldsValueArtifact(udi, dependencies)
            {
                Name = entity.NodeName,
                Values = values,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<MetaFieldsValueArtifact, MetaFieldsNodeValuesModel> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 7 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var nodeKey = state.Artifact.Udi.Guid;
            if (contentService.GetById(nodeKey) is null)
            {
                return Task.CompletedTask; // node not (yet) in target; skip rather than fail
            }

            foreach (var (culture, fields) in state.Artifact.Values)
            {
                foreach (var (alias, json) in fields)
                {
                    if (json is null)
                    {
                        continue;
                    }

                    var value = JsonConvert.DeserializeObject(json);
                    if (value is null)
                    {
                        continue;
                    }

                    if (valueRepository.Exists(nodeKey, alias, culture))
                    {
                        valueRepository.Update(nodeKey, alias, culture, value);
                    }
                    else
                    {
                        valueRepository.Add(nodeKey, alias, culture, value);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
```

Create `src/SeoToolkit.Umbraco.Deploy/Connectors/ServiceConnectors/SeoToolkitSitemapContentServiceConnector.cs`:

```csharp
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, UdiType.GuidUdi)]
    public class SeoToolkitSitemapContentServiceConnector(
        ISitemapService sitemapService,
        IContentService contentService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<SitemapContentArtifact, SitemapContentSettings>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.SitemapContent;

        public override string OpenUdiName => "All SeoToolkit sitemap content settings";

        public override int[] ProcessPasses => [7];

        public override string GetEntityName(SitemapContentSettings entity)
            => contentService.GetById(entity.NodeKey)?.Name ?? entity.NodeKey.ToString();

        protected override GuidUdi GetEntityUdi(SitemapContentSettings entity)
            => new(UdiEntityType, entity.NodeKey);

        public override Task<SitemapContentSettings?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(sitemapService.GetContentSettings(id));

        public override async IAsyncEnumerable<SitemapContentSettings> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in sitemapService.GetAllContentSettings())
            {
                yield return entity;
            }
        }

        public override Task<SitemapContentArtifact?> GetArtifactAsync(
            GuidUdi? udi, SitemapContentSettings? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<SitemapContentArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.Document, entity.NodeKey)),
            };

            return Task.FromResult<SitemapContentArtifact?>(new SitemapContentArtifact(udi, dependencies)
            {
                Name = GetEntityName(entity),
                ExcludeFromSitemap = entity.ExcludeFromSitemap,
                ChangeFrequency = entity.ChangeFrequency,
                Priority = entity.Priority,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<SitemapContentArtifact, SitemapContentSettings> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 7 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var nodeKey = state.Artifact.Udi.Guid;
            if (contentService.GetById(nodeKey) is null)
            {
                return Task.CompletedTask;
            }

            sitemapService.SetContentSettings(new SitemapContentSettings
            {
                NodeKey = nodeKey,
                ExcludeFromSitemap = state.Artifact.ExcludeFromSitemap,
                ChangeFrequency = state.Artifact.ChangeFrequency ?? string.Empty,
                Priority = state.Artifact.Priority,
            });
            return Task.CompletedTask;
        }
    }
}
```

- [ ] **Step 5: Write the SitemapContent test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/SitemapContentConnectorTests.cs`:

```csharp
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SitemapContentConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [Test]
        public async Task RoundTrip_PreservesOverridesAndDependsOnDocument()
        {
            var nodeKey = Guid.NewGuid();
            var content = new Mock<IContent>();
            content.SetupGet(c => c.Key).Returns(nodeKey);
            content.SetupGet(c => c.Name).Returns("News item");
            var contentService = new Mock<IContentService>();
            contentService.Setup(s => s.GetById(nodeKey)).Returns(content.Object);

            var sitemapService = new Mock<ISitemapService>();
            sitemapService.Setup(s => s.GetContentSettings(nodeKey)).Returns(new SitemapContentSettings
            {
                NodeKey = nodeKey,
                ExcludeFromSitemap = true,
                ChangeFrequency = "daily",
                Priority = 0.9,
            });

            var connector = new SeoToolkitSitemapContentServiceConnector(
                sitemapService.Object, contentService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, nodeKey);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.Dependencies.Select(d => d.Udi),
                Does.Contain(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)));

            SitemapContentSettings? saved = null;
            sitemapService.Setup(s => s.SetContentSettings(It.IsAny<SitemapContentSettings>()))
                .Callback<SitemapContentSettings>(s => saved = s);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.NodeKey, Is.EqualTo(nodeKey));
                Assert.That(saved.ExcludeFromSitemap, Is.True);
                Assert.That(saved.ChangeFrequency, Is.EqualTo("daily"));
                Assert.That(saved.Priority, Is.EqualTo(0.9));
            });
        }
    }
}
```

- [ ] **Step 6: Run all Task 9 tests**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~MetaFieldsValueConnectorTests|FullyQualifiedName~SitemapContentConnectorTests"`
Expected: PASS (4 tests).

- [ ] **Step 7: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Umbraco.MetaFields.Core src/SeoToolkit.Tests
git commit -m "feat(deploy): add per-node meta fields value and sitemap content connectors"
```

---

### Task 10: Ride-along handler — attach per-node SEO artifacts to document exports

**Files:**
- Create: `src/SeoToolkit.Umbraco.Deploy/NotificationHandlers/SeoToolkitContentExportedHandler.cs`
- Modify: `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComposer.cs` (register handler)
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/ContentExportedHandlerTests.cs`

**Interfaces:**
- Consumes: `ArtifactExportedNotification : ObjectNotification<IArtifact>` (`Umbraco.Deploy.Core.Events`, property `Artifact` — confirm the property name in `~/Source/github/umbraco/Umbraco-Deploy/src/Umbraco.Deploy.Core/Events/ArtifactExportedNotification.cs`; `ObjectNotification<T>` exposes the object via a property, often `Target` — use whatever that file declares); `DocumentArtifact` (`Umbraco.Deploy.Infrastructure.Artifacts.Content`); `IMetaFieldsValueRepository.GetAllValues(Guid)` (Task 9); `ISitemapService.GetContentSettings(Guid)`; `INotificationAsyncHandler<T>` (`Umbraco.Cms.Core.Events`).
- Produces: `SeoToolkitContentExportedHandler : INotificationAsyncHandler<ArtifactExportedNotification>` — for document artifacts, appends `seotoolkit-metafields-value` / `seotoolkit-sitemap-content` dependencies (Exist, non-ordering) when that node has SEO data.

- [ ] **Step 1: Write the failing tests**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/ContentExportedHandlerTests.cs`:

```csharp
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Deploy.Core.Events;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class ContentExportedHandlerTests
    {
        private Mock<IMetaFieldsValueRepository> _valueRepository = null!;
        private Mock<ISitemapService> _sitemapService = null!;
        private SeoToolkitContentExportedHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _valueRepository = new Mock<IMetaFieldsValueRepository>();
            _sitemapService = new Mock<ISitemapService>();
            _handler = new SeoToolkitContentExportedHandler(_valueRepository.Object, _sitemapService.Object);
        }

        // NOTE: constructing DocumentArtifact and ArtifactExportedNotification — check the
        // constructors in the Deploy source; DocumentArtifact takes (GuidUdi?, IEnumerable<ArtifactDependency>?)
        // like other DeployArtifactBase subclasses, and the notification wraps the artifact.

        [Test]
        public async Task DocumentWithSeoData_GetsBothDependenciesAppended()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = new DocumentArtifact(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)) { Name = "Page" };
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "x" },
            });
            _sitemapService.Setup(s => s.GetContentSettings(nodeKey))
                .Returns(new SitemapContentSettings { NodeKey = nodeKey, ExcludeFromSitemap = true });

            await _handler.HandleAsync(new ArtifactExportedNotification(artifact), CancellationToken.None);

            var dependencyUdis = artifact.Dependencies.Select(d => d.Udi).ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(dependencyUdis,
                    Does.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey)));
                Assert.That(dependencyUdis,
                    Does.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, nodeKey)));
            });
        }

        [Test]
        public async Task DocumentWithoutSeoData_IsUntouched()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = new DocumentArtifact(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)) { Name = "Page" };
            _valueRepository.Setup(r => r.GetAllValues(nodeKey)).Returns([]);
            _sitemapService.Setup(s => s.GetContentSettings(nodeKey)).Returns((SitemapContentSettings?)null);

            await _handler.HandleAsync(new ArtifactExportedNotification(artifact), CancellationToken.None);

            Assert.That(artifact.Dependencies.Select(d => d.Udi.EntityType),
                Has.None.StartsWith("seotoolkit-"));
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~ContentExportedHandlerTests"`
Expected: FAIL to compile — handler does not exist. Fix any `DocumentArtifact`/`ArtifactExportedNotification` constructor mismatches against the Deploy source before proceeding (the test is the harness; its intent stands even if constructor arity changes).

- [ ] **Step 3: Implement the handler**

Create `src/SeoToolkit.Umbraco.Deploy/NotificationHandlers/SeoToolkitContentExportedHandler.cs`:

```csharp
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Events;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;

namespace SeoToolkit.Umbraco.Deploy.NotificationHandlers
{
    /// <summary>
    /// When a document artifact is exported (transfer, restore source, queue for transfer),
    /// appends the node's SeoToolkit per-node artifacts as Exist dependencies so they are
    /// pulled into the same deployment automatically.
    /// </summary>
    public class SeoToolkitContentExportedHandler(
        IMetaFieldsValueRepository valueRepository,
        ISitemapService sitemapService)
        : INotificationAsyncHandler<ArtifactExportedNotification>
    {
        public Task HandleAsync(ArtifactExportedNotification notification, CancellationToken cancellationToken)
        {
            // ObjectNotification<IArtifact> exposes the wrapped object; confirm the property
            // name (Artifact/Target) in the Deploy source and adjust.
            if (notification.Artifact is not DocumentArtifact documentArtifact
                || documentArtifact.Udi is not GuidUdi documentUdi)
            {
                return Task.CompletedTask;
            }

            var extraDependencies = new List<ArtifactDependency>();

            if (valueRepository.GetAllValues(documentUdi.Guid).Count > 0)
            {
                extraDependencies.Add(new SeoToolkitArtifactDependency(
                    new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, documentUdi.Guid)));
            }

            if (sitemapService.GetContentSettings(documentUdi.Guid) is not null)
            {
                extraDependencies.Add(new SeoToolkitArtifactDependency(
                    new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, documentUdi.Guid)));
            }

            if (extraDependencies.Count > 0)
            {
                // Dependencies setter re-orders by Udi; safe to reassign.
                documentArtifact.Dependencies = documentArtifact.Dependencies.Concat(extraDependencies).ToList();
            }

            return Task.CompletedTask;
        }
    }
}
```

**Checksum verification (spec §4 open item) — RESOLVED during execution:** `ArtifactImportExportService.ExportAsync` (Umbraco-Deploy source) publishes `ArtifactExportingNotification`, then serializes the artifact to the export stream, then publishes `ArtifactExportedNotification`. Dependencies mutated in the *Exported* handler are therefore written AFTER serialization and never reach the export — so the handler MUST use `ArtifactExportingNotification` (fires before serialization). `Dependencies` participates in the checksum (no `[ChecksumIgnore]`), and the checksum is `Lazy`, computed on first read during serialization — so mutating in the Exporting handler is included in the checksum. Handler implemented against `ArtifactExportingNotification`; its `Artifact` property is typed `IArtifactSignature` but the runtime object is the concrete `DocumentArtifact`, so we cast to mutate `Dependencies`. Whether Deploy's upstream graph resolver then pulls the added dependency artifacts into the same transfer remains a runtime-verification item.

- [ ] **Step 4: Register the handler in the composer**

In `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComposer.cs`, add to `Compose` (before `builder.Components()...`):

```csharp
            builder.AddNotificationAsyncHandler<ArtifactExportedNotification, SeoToolkitContentExportedHandler>();
```

with usings `Umbraco.Deploy.Core.Events`, `SeoToolkit.Umbraco.Deploy.NotificationHandlers`, and `Umbraco.Cms.Core.Notifications`/`Umbraco.Extensions` as needed for the `AddNotificationAsyncHandler` extension.

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~ContentExportedHandlerTests"`
Expected: PASS (2 tests).

- [ ] **Step 6: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Tests
git commit -m "feat(deploy): attach per-node SEO artifacts to document exports"
```

---

### Task 11: Disk (`.uda`) integration — Core notifications + disk refreshers

**Files:**
- Create: `src/SeoToolkit.Umbraco.ScriptManager.Core/Notifications/ScriptSavedNotification.cs`
- Create: `src/SeoToolkit.Umbraco.ScriptManager.Core/Notifications/ScriptDeletedNotification.cs`
- Modify: `src/SeoToolkit.Umbraco.ScriptManager.Core/Services/ScriptManagerService.cs` (publish them)
- Create: `src/SeoToolkit.Umbraco.Common.Core/Notifications/SeoDomainCollectionSavedNotification.cs`
- Create: `src/SeoToolkit.Umbraco.Common.Core/Notifications/SeoDomainCollectionDeletedNotification.cs`
- Create: `src/SeoToolkit.Umbraco.Common.Core/Notifications/SeoKeyValueSavedNotification.cs`
- Modify: `src/SeoToolkit.Umbraco.Common.Core/Services/Domains/SeoDomainsService.cs` (publish saved/deleted)
- Modify: whichever component writes key/values (find with `grep -rn "ISeoKeyValueRepository" src --include="*.cs" -l` — likely a management API controller in `Common.Core` or `SeoToolkit.Umbraco.Common`; publish `SeoKeyValueSavedNotification` after `Set`/`Delete`)
- Create: `src/SeoToolkit.Umbraco.Deploy/NotificationHandlers/SeoToolkitDiskRefresherHandlers.cs`
- Modify: `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComponent.cs` (register disk entity types)
- Modify: `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComposer.cs` (register handlers)
- Test: `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/DiskRefresherHandlerTests.cs`

**Interfaces:**
- Consumes:
  - `IEventAggregator` (`Umbraco.Cms.Core.Events`) to publish Core notifications.
  - Existing notifications: `SeoSettingSavedNotification { Guid ContentTypeGuid; bool IsEnabled }` (Common.Core), `MetaFieldSettingsSavedNotification { DocumentTypeSettingsDto Model }` (MetaFields.Core), `SitemapPageSettingsSavedNotification { SitemapPageSettings Model }` (Sitemap.Core).
  - `IDiskEntityService` (`Umbraco.Deploy.Infrastructure.Disk`): `RegisterDiskEntityType(string entityType)`, `Task WriteArtifactsAsync(IEnumerable<IArtifact>, ...)`, delete counterpart — copy exact call shapes from `UmbracoCommerceDeployComponent.WriteEntityArtifact`/`DeleteEntityArtifact` (bottom of `~/Source/GitHub/Umbraco/Umbraco.Commerce.Deploy/src/Umbraco.Commerce.Deploy/Composing/UmbracoCommerceDeployComponent.cs`).
  - `IServiceConnectorFactory.GetConnector(string entityType)` to build artifacts for disk writing.
- Produces:
  - New Core notifications (all `INotification`): `ScriptSavedNotification { Script Script }`, `ScriptDeletedNotification { Guid Key }`, `SeoDomainCollectionSavedNotification { SeoDomainCollection Collection }`, `SeoDomainCollectionDeletedNotification { Guid Id }`, `SeoKeyValueSavedNotification { Guid? DomainCollectionId }`.
  - One handler class per notification in `SeoToolkitDiskRefresherHandlers.cs`, each resolving the matching connector via `IServiceConnectorFactory`, building the artifact, and calling `IDiskEntityService.WriteArtifactsAsync` (or delete).

- [ ] **Step 1: Add the Core notifications**

Create `src/SeoToolkit.Umbraco.ScriptManager.Core/Notifications/ScriptSavedNotification.cs`:

```csharp
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Notifications
{
    public class ScriptSavedNotification : INotification
    {
        public Script Script { get; }

        public ScriptSavedNotification(Script script)
        {
            Script = script;
        }
    }
}
```

Create `src/SeoToolkit.Umbraco.ScriptManager.Core/Notifications/ScriptDeletedNotification.cs`:

```csharp
using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Notifications
{
    public class ScriptDeletedNotification : INotification
    {
        public Guid Key { get; }

        public ScriptDeletedNotification(Guid key)
        {
            Key = key;
        }
    }
}
```

Create the three Common.Core notifications with the same shape (`SeoDomainCollectionSavedNotification` holding `SeoDomainCollection Collection`, `SeoDomainCollectionDeletedNotification` holding `Guid Id`, `SeoKeyValueSavedNotification` holding `Guid? DomainCollectionId`), namespace `SeoToolkit.Umbraco.Common.Core.Notifications`, matching the style of `SeoSettingSavedNotification.cs` in the same folder (explicit `using System;` — these Core projects do not enable implicit usings).

- [ ] **Step 2: Publish from Core services**

`src/SeoToolkit.Umbraco.ScriptManager.Core/Services/ScriptManagerService.cs`: add an `IEventAggregator eventAggregator` constructor parameter (stored in a `_eventAggregator` field; update the constructor and add `using Umbraco.Cms.Core.Events;` + `using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;`). In `Save(Script script)` publish after the repository call, before `return`:

```csharp
            _eventAggregator.Publish(new ScriptSavedNotification(script));
```

In `Delete(Guid[] ids)` (and the obsolete `Delete(int[] ids)`), after each successful `_scriptRepository.Delete(script)`:

```csharp
                if (script.Key is not null)
                {
                    _eventAggregator.Publish(new ScriptDeletedNotification(script.Key.Value));
                }
```

`src/SeoToolkit.Umbraco.Common.Core/Services/Domains/SeoDomainsService.cs`: same pattern — inject `IEventAggregator`, publish `SeoDomainCollectionSavedNotification(collection)` at the end of `Save` (after the collection has its Id) and `SeoDomainCollectionDeletedNotification(domainId)` at the end of `Delete`.

Key/values: run `grep -rn "ISeoKeyValueRepository" src --include="*.cs" -l` — in the class(es) that call `Set`/`Delete` outside the repository itself (management controller or service), inject `IEventAggregator` and publish `SeoKeyValueSavedNotification(domainId)` after the write. If writes happen only in a controller, publish there.

Constructor changes to `ScriptManagerService`/`SeoDomainsService` are safe: both are only constructed via DI (verify with `grep -rn "new ScriptManagerService(\|new SeoDomainsService(" src --include="*.cs"` — expect no hits outside DI registration).

- [ ] **Step 3: Register disk entity types in the component**

In `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComponent.cs`, inject `IDiskEntityService diskEntityService` via a primary constructor and extend `InitializeAsync`:

```csharp
using Umbraco.Deploy.Infrastructure.Disk;

// class declaration becomes:
public class SeoToolkitDeployComponent(IDiskEntityService diskEntityService) : IAsyncComponent

// in InitializeAsync, after RegisterUdiTypes():
            InitializeDiskRefreshers();

// new method:
        private void InitializeDiskRefreshers()
        {
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SeoSetting);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.Script);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.DomainCollection);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.KeyValues);
        }
```

(Per-node types `MetaFieldsValue`/`SitemapContent` are deliberately NOT disk-registered — they are content-like data, spec §5.)

- [ ] **Step 4: Implement the disk refresher notification handlers**

Create `src/SeoToolkit.Umbraco.Deploy/NotificationHandlers/SeoToolkitDiskRefresherHandlers.cs`. Shared base + one handler per notification. The write path mirrors `UmbracoCommerceDeployComponent.WriteEntityArtifact`: resolve connector → `GetArtifactAsync(udi, contextCache)` → `diskEntityService.WriteArtifactsAsync([artifact])`; delete path builds the UDI and calls the disk delete API (copy the exact method name from the Commerce component).

```csharp
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Disk;

namespace SeoToolkit.Umbraco.Deploy.NotificationHandlers
{
    public abstract class SeoToolkitDiskRefresherHandlerBase(
        IDiskEntityService diskEntityService,
        IServiceConnectorFactory serviceConnectorFactory)
    {
        protected async Task WriteArtifactAsync(string entityType, Guid id, CancellationToken cancellationToken)
        {
            var udi = new GuidUdi(entityType, id);
            IServiceConnector connector = serviceConnectorFactory.GetConnector(udi.EntityType);
            IArtifact? artifact = await connector.GetArtifactAsync(udi, PassThroughCache.Instance, cancellationToken)
                .ConfigureAwait(false);
            if (artifact is not null)
            {
                // Copy the exact WriteArtifactsAsync/DeleteArtifacts signatures from
                // UmbracoCommerceDeployComponent (Commerce Deploy) — argument shapes may
                // include a collection and optional flags.
                await diskEntityService.WriteArtifactsAsync([artifact]).ConfigureAwait(false);
            }
        }

        protected void DeleteArtifact(string entityType, Guid id)
        {
            var udi = new GuidUdi(entityType, id);
            diskEntityService.DeleteArtifacts([udi]);
        }
    }

    public class SeoSettingDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SeoSettingSavedNotification>
    {
        public Task HandleAsync(SeoSettingSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, notification.ContentTypeGuid, cancellationToken);
    }

    public class MetaFieldsSettingDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<MetaFieldSettingsSavedNotification>
    {
        public Task HandleAsync(MetaFieldSettingsSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, notification.Model.Content.Key, cancellationToken);
    }

    public class SitemapPageTypeDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SitemapPageSettingsSavedNotification>
    {
        public Task HandleAsync(SitemapPageSettingsSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, notification.Model.ContentTypeGuid, cancellationToken);
    }

    public class ScriptDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<ScriptSavedNotification>,
          INotificationAsyncHandler<ScriptDeletedNotification>
    {
        public Task HandleAsync(ScriptSavedNotification notification, CancellationToken cancellationToken)
            => notification.Script.Key is null
                ? Task.CompletedTask
                : WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.Script, notification.Script.Key.Value, cancellationToken);

        public Task HandleAsync(ScriptDeletedNotification notification, CancellationToken cancellationToken)
        {
            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.Script, notification.Key);
            return Task.CompletedTask;
        }
    }

    public class DomainCollectionDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SeoDomainCollectionSavedNotification>,
          INotificationAsyncHandler<SeoDomainCollectionDeletedNotification>
    {
        public Task HandleAsync(SeoDomainCollectionSavedNotification notification, CancellationToken cancellationToken)
            => notification.Collection.Id is null
                ? Task.CompletedTask
                : WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, notification.Collection.Id.Value, cancellationToken);

        public Task HandleAsync(SeoDomainCollectionDeletedNotification notification, CancellationToken cancellationToken)
        {
            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, notification.Id);
            return Task.CompletedTask;
        }
    }

    public class KeyValuesDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SeoKeyValueSavedNotification>
    {
        public Task HandleAsync(SeoKeyValueSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(
                SeoToolkitDeployConstants.UdiEntityType.KeyValues,
                notification.DomainCollectionId ?? SeoToolkitDeployConstants.RootKeyValuesGuid,
                cancellationToken);
    }
}
```

(`PassThroughCache.Instance` — if there is no static instance, `new PassThroughCache()`; check `~/Source/github/umbraco/Umbraco-CMS/src/Umbraco.Core/Deploy/PassThroughCache.cs`.)

- [ ] **Step 5: Register the handlers in the composer**

In `SeoToolkitDeployComposer.Compose`, add:

```csharp
            builder.AddNotificationAsyncHandler<SeoSettingSavedNotification, SeoSettingDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<MetaFieldSettingsSavedNotification, MetaFieldsSettingDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SitemapPageSettingsSavedNotification, SitemapPageTypeDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<ScriptSavedNotification, ScriptDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<ScriptDeletedNotification, ScriptDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoDomainCollectionSavedNotification, DomainCollectionDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoDomainCollectionDeletedNotification, DomainCollectionDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoKeyValueSavedNotification, KeyValuesDiskRefresherHandler>();
```

- [ ] **Step 6: Write a handler test**

Create `src/SeoToolkit.Tests/SeoToolkit.Tests/Deploy/DiskRefresherHandlerTests.cs`:

```csharp
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Disk;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class DiskRefresherHandlerTests
    {
        [Test]
        public async Task SeoSettingSaved_WritesArtifactViaConnector()
        {
            var contentTypeKey = Guid.NewGuid();
            var artifact = Mock.Of<IArtifact>();
            var connector = new Mock<IServiceConnector>();
            connector.Setup(c => c.GetArtifactAsync(
                    It.Is<Udi>(u => u.EntityType == SeoToolkitDeployConstants.UdiEntityType.SeoSetting),
                    It.IsAny<IContextCache>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(artifact);

            var factory = new Mock<IServiceConnectorFactory>();
            factory.Setup(f => f.GetConnector(SeoToolkitDeployConstants.UdiEntityType.SeoSetting))
                .Returns(connector.Object);
            var diskService = new Mock<IDiskEntityService>();

            var handler = new SeoSettingDiskRefresherHandler(diskService.Object, factory.Object);
            await handler.HandleAsync(new SeoSettingSavedNotification(contentTypeKey, true), CancellationToken.None);

            diskService.Verify(d => d.WriteArtifactsAsync(
                It.Is<IEnumerable<IArtifact>>(a => a.Contains(artifact))), Times.Once);
        }
    }
}
```

(Adjust the `WriteArtifactsAsync` `Verify` to the real signature once copied from the Commerce component.)

- [ ] **Step 7: Run tests and full build**

Run: `dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~Deploy"`
Expected: all Deploy tests PASS.
Run: `dotnet build src/SeoToolkit.Umbraco.sln`
Expected: Build succeeded (Core projects compile with new notifications).

- [ ] **Step 8: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy src/SeoToolkit.Umbraco.ScriptManager.Core src/SeoToolkit.Umbraco.Common.Core src/SeoToolkit.Tests
git commit -m "feat(deploy): add disk (.uda) integration with save/delete notifications"
```

---

### Task 12: Transfer/restore registration, README, final verification

**Files:**
- Modify: `src/SeoToolkit.Umbraco.Deploy/Composing/SeoToolkitDeployComponent.cs` (transfer entity registration)
- Create: `src/SeoToolkit.Umbraco.Deploy/README.md` (packaged readme; also add `<PackageReadmeFile>` if desired — optional, matches no existing precedent so plain repo readme is fine)
- Test: full suite + solution build

**Interfaces:**
- Consumes: `ITransferEntityService.RegisterTransferEntityType(string entityType, DeployRegisteredEntityTypeDetailOptions options, ...)` (`Umbraco.Deploy.Infrastructure.Transfer`) — exact optional parameters (`tryParseUdiRangeFromNodeId`, `remoteTree`) per the interface docs; copy the minimal Commerce call shape.
- Produces: settings-like entity types registered for queue-for-transfer / restore / import-export.

- [ ] **Step 1: Register transfer entity types**

In `SeoToolkitDeployComponent`, inject `ITransferEntityService transferEntityService` and call from `InitializeAsync`:

```csharp
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Transfer;

        private void InitializeIntegratedEntities()
        {
            foreach (var entityType in new[]
            {
                SeoToolkitDeployConstants.UdiEntityType.SeoSetting,
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting,
                SeoToolkitDeployConstants.UdiEntityType.SitemapPageType,
                SeoToolkitDeployConstants.UdiEntityType.Script,
                SeoToolkitDeployConstants.UdiEntityType.DomainCollection,
                SeoToolkitDeployConstants.UdiEntityType.KeyValues,
            })
            {
                transferEntityService.RegisterTransferEntityType(
                    entityType,
                    new DeployRegisteredEntityTypeDetailOptions
                    {
                        SupportsQueueForTransfer = true,
                        SupportsRestore = true,
                        PermittedToRestore = true,
                        SupportsPartialRestore = true,
                        SupportsImportExport = true,
                    });
            }
        }
```

Verification note: `RegisterTransferEntityType` may require the tree-node-id parse function (`tryParseUdiRangeFromNodeId`) for queue-for-transfer to appear on SeoToolkit's backoffice trees. Check the interface's optional parameters and the Commerce call; if SeoToolkit's management-API trees don't surface Deploy entity actions in the new backoffice, keep the registration (it still enables **import/export and restore**) and note tree integration as a known follow-up in the README rather than blocking this task.

- [ ] **Step 2: Write the README**

Create `src/SeoToolkit.Umbraco.Deploy/README.md` covering: what the package does (settings via `.uda` disk artifacts + on-demand transfer; per-node SEO data rides along with content transfers automatically); the eight UDI entity types; the `SeoToolkit:Deploy:DisabledEntityTypes` config option with an appsettings example; the note that appsettings-based SeoToolkit config (`SeoToolkit:Global` etc.) is intentionally out of scope; environment-specific caveats (domain names are matched by name on import; key/values deploy by overwrite of transferred keys, never deleting target-only keys); and known follow-ups (backoffice tree queue-for-transfer UX, Redirects/RobotsTxt modules out of scope for v1).

- [ ] **Step 3: Full verification**

```bash
dotnet build src/SeoToolkit.Umbraco.sln
dotnet test src/SeoToolkit.Tests/SeoToolkit.Tests/SeoToolkit.Tests.csproj --filter "FullyQualifiedName~Deploy"
dotnet pack src/SeoToolkit.Umbraco.Deploy/SeoToolkit.Umbraco.Deploy.csproj -o /tmp/seotoolkit-deploy-pack
```

Expected: build succeeds, all Deploy tests pass, pack produces `SeoToolkit.Umbraco.Deploy.1.0.0-beta1.nupkg`.

- [ ] **Step 4: Commit**

```bash
git add src/SeoToolkit.Umbraco.Deploy
git commit -m "feat(deploy): register transfer entities and add package README"
```

---

## Runtime verification (post-plan, manual)

The unit tests cover connector logic with mocks. Before releasing, verify against a real pair of environments (or one site with Deploy's import/export):

1. Run the dev site (`SeoToolkit.Umbraco.Site`) with `Umbraco.Deploy.OnPrem` added, configure some MetaFields/Sitemap/Script settings, and confirm `.uda` files appear under `umbraco/Deploy/Revision` when saving settings.
2. Export/import a content node with per-node SEO values and confirm the values arrive (validates the ride-along handler + pass 7 end-to-end, including the two open items: checksum timing and third-party pass numbers).
3. If pass 7 is not honoured (per-node artifacts processed before documents exist), the artifacts still apply on the next transfer because missing nodes are skipped — but fix properly by switching `ProcessPasses` to a value Deploy accepts (check `WorkItem` pass scheduling in the Deploy source).

## Spec coverage self-review

- Spec §1 project/packaging/soft-fail → Tasks 1–3 (soft-fail via `DisabledEntityTypes` + `IsDisabled` guard).
- Spec §2 UDI types & artifacts (7 rows) → Tasks 2, 4–9 (key/values + domain collections are two entity types — the spec's last row covers both).
- Spec §3 connectors & passes → Tasks 3–9; skip-on-missing behaviour in every `ProcessAsync`.
- Spec §4 ride-along + checksum fallback → Task 10.
- Spec §5 disk + transfer + new Core notifications → Tasks 11–12.
- Spec §6 config → Tasks 2 (`DisabledEntityTypes`; the spec's ignore-lists for keys/scripts folded into this single mechanism — simpler v1, same purpose).
- Spec §7 testing → per-task NUnit tests incl. culture-variant values (Task 9), UDI extraction (Task 6), soft-fail guard (base class behaviour exercised via disabled check in every connector).
- Spec open items: ScriptManager GUID key — **resolved during planning** (`ScriptEntity.Key` exists); checksum timing → Task 10 step 3 note; pass-7 support → runtime verification §3.
