using System.Data.Common;
using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Moq;
using SeoToolkit.Umbraco.Redirects.Core.Migrations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;

namespace SeoToolkit.Tests;

[TestFixture]
public class RedirectGuidIdMigrationTests
{
    [Test]
    [Explicit("Requires Docker. Executes RedirectGuidIdMigration against a SQL Server test database.")]
    public async Task RedirectGuidIdMigration_ExecutesAndMutatesSchemaAndData()
    {
        var contentKey = Guid.NewGuid();
        var mediaKey = Guid.NewGuid();

        await using var container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", "Your_strong_Passw0rd!")
            .WithPortBinding(14333, 1433)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        await container.StartAsync();

        var masterConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = "localhost,14333",
            UserID = "sa",
            Password = "Your_strong_Passw0rd!",
            InitialCatalog = "master",
            TrustServerCertificate = true,
        }.ToString();

        await EnsureDatabaseExists(masterConnectionString, "SeoToolkitMigrationTests");

        var testDbConnectionString = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = "SeoToolkitMigrationTests",
        }.ToString();

        using var umbracoDb = CreateUmbracoDatabase(testDbConnectionString);

        SeedPreMigrationSchema(umbracoDb);
        SeedPreMigrationData(umbracoDb);

        var keyValueService = new Mock<IKeyValueService>();
        keyValueService
            .Setup(x => x.GetValue("Umbraco.Core.Upgrader.State+SeoToolkit_Common_Migration"))
            .Returns("state-6");

        var content = new Mock<IContent>();
        content.SetupGet(x => x.Key).Returns(contentKey);

        var media = new Mock<IMedia>();
        media.SetupGet(x => x.Key).Returns(mediaKey);

        var contentService = new Mock<IContentService>();
        contentService.Setup(x => x.GetById(10)).Returns(content.Object);

        var mediaService = new Mock<IMediaService>();
        mediaService.Setup(x => x.GetById(20)).Returns(media.Object);

        var migrationContext = new Mock<IMigrationContext>();
        migrationContext.SetupGet(x => x.Database).Returns(umbracoDb);
        migrationContext.SetupGet(x => x.DatabaseType).Returns(DatabaseType.SqlServer);

        var migration = new RedirectGuidIdMigration(
            migrationContext.Object,
            keyValueService.Object,
            contentService.Object,
            mediaService.Object);

        var migrateMethod = typeof(RedirectGuidIdMigration)
            .GetMethod("MigrateAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var task = (Task)migrateMethod.Invoke(migration, null)!;
        await task;

        Assert.That(umbracoDb.ExecuteScalar<int>("SELECT COUNT(*) FROM SeoToolkitRedirects WHERE [Key] IS NULL"), Is.EqualTo(0));
        Assert.That(umbracoDb.ExecuteScalar<int>("SELECT COUNT(*) FROM SeoToolkitRedirects WHERE NewNodeId = @0 AND NewNodeKey = @1", 10, contentKey), Is.EqualTo(1));
        Assert.That(umbracoDb.ExecuteScalar<int>("SELECT COUNT(*) FROM SeoToolkitRedirects WHERE NewNodeId = @0 AND NewNodeKey = @1", 20, mediaKey), Is.EqualTo(1));

        var keyPrimary = umbracoDb.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM sys.key_constraints kc
JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE kc.type = 'PK' AND OBJECT_NAME(kc.parent_object_id) = 'SeoToolkitRedirects' AND c.name = 'Key'");

        Assert.That(keyPrimary, Is.EqualTo(1));
    }

    private static async Task EnsureDatabaseExists(string masterConnectionString, string databaseName)
    {
        await using var conn = new SqlConnection(masterConnectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand($@"
IF DB_ID('{databaseName}') IS NULL
BEGIN
    CREATE DATABASE [{databaseName}]
END", conn);

        await cmd.ExecuteNonQueryAsync();
    }

    private static IUmbracoDatabase CreateUmbracoDatabase(string connectionString)
    {
        var umbracoDbType = typeof(IUmbracoDatabase).Assembly.GetType("Umbraco.Cms.Infrastructure.Persistence.UmbracoDatabase")
            ?? throw new InvalidOperationException("Could not find UmbracoDatabase type.");

        var providerFactory = (DbProviderFactory)SqlClientFactory.Instance;
        var constructors = umbracoDbType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .OrderBy(c => c.GetParameters().Length)
            .ToArray();

        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            var values = new object?[parameters.Length];
            var canUse = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterType = parameter.ParameterType;
                var name = parameter.Name?.ToLowerInvariant() ?? string.Empty;

                if (parameterType == typeof(string) && name.Contains("connection"))
                {
                    values[i] = connectionString;
                    continue;
                }

                if (parameterType == typeof(string) && name.Contains("provider"))
                {
                    values[i] = "Microsoft.Data.SqlClient";
                    continue;
                }

                if (parameterType == typeof(DbProviderFactory))
                {
                    values[i] = providerFactory;
                    continue;
                }

                if (parameterType == typeof(DatabaseType))
                {
                    values[i] = DatabaseType.SqlServer;
                    continue;
                }

                if (!parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null)
                {
                    values[i] = null;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    values[i] = parameter.DefaultValue;
                    continue;
                }

                canUse = false;
                break;
            }

            if (!canUse)
            {
                continue;
            }

            try
            {
                var instance = ctor.Invoke(values);
                if (instance is IUmbracoDatabase umbracoDatabase)
                {
                    return umbracoDatabase;
                }
            }
            catch
            {
                // try next constructor
            }
        }

        throw new InvalidOperationException("Unable to construct UmbracoDatabase using available constructors.");
    }

    private static void SeedPreMigrationSchema(IUmbracoDatabase db)
    {
        db.Execute(@"
IF OBJECT_ID('SeoToolkitRedirects', 'U') IS NOT NULL DROP TABLE SeoToolkitRedirects;

CREATE TABLE SeoToolkitRedirects (
    Id INT IDENTITY(1,1) NOT NULL,
    Domain INT NULL,
    CustomDomain NVARCHAR(255) NULL,
    IsRegex BIT NOT NULL,
    IsEnabled BIT NOT NULL,
    OldUrl NVARCHAR(2048) NOT NULL,
    NewUrl NVARCHAR(2048) NULL,
    NewNodeId INT NULL,
    NewNodeCultureId INT NULL,
    RedirectCode INT NOT NULL,
    LastUpdated DATETIME2 NOT NULL,
    CreatedBy INT NOT NULL,
    CONSTRAINT pk_SeoToolkitRedirects PRIMARY KEY (Id)
);

CREATE NONCLUSTERED INDEX IX_SeoToolkitOldUrl ON SeoToolkitRedirects (OldUrl, IsEnabled);
CREATE NONCLUSTERED INDEX IX_SeoToolkitRegex ON SeoToolkitRedirects (IsRegex);
");
    }

    private static void SeedPreMigrationData(IUmbracoDatabase db)
    {
        db.Execute(@"
INSERT INTO SeoToolkitRedirects (Domain, CustomDomain, IsRegex, IsEnabled, OldUrl, NewUrl, NewNodeId, NewNodeCultureId, RedirectCode, LastUpdated, CreatedBy)
VALUES
(NULL, NULL, 0, 1, '/old-1', '/new-1', NULL, NULL, 301, SYSUTCDATETIME(), -1),
(NULL, NULL, 0, 1, '/old-2', NULL, 10, 1033, 301, SYSUTCDATETIME(), -1),
(NULL, NULL, 0, 1, '/old-3', NULL, 20, NULL, 301, SYSUTCDATETIME(), -1);
");
    }
}
