using Microsoft.AspNetCore.Http.HttpResults;
using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class MigrationHelper
    {
        public static void RecreateTable<T>(IUmbracoDatabase umbracoDatabase, ICreateBuilder create, Sql<ISqlContext> sql, string tableName)
        {
            umbracoDatabase.Execute($"DROP TABLE IF EXISTS old_{tableName}");
            umbracoDatabase.Execute($"ALTER TABLE {tableName} RENAME TO old_{tableName}");
            create.Table<T>().Do();
            umbracoDatabase.InsertBulk(umbracoDatabase.Fetch<T>(sql
                .SelectAll()
                .From($"old_{tableName}")));
        }

        public static void EnsureMigration(string module, int minVersion, IKeyValueService keyValueService)
        {
            var value = keyValueService.GetValue($"Umbraco.Core.Upgrader.State+{module}") ?? throw new Exception($"Migration for {module} not found, please ensure the module is installed correctly.");
            var version = int.Parse(value.Replace("state-", ""));

            if (version < minVersion)
            {
                throw new Exception($"Migration for {module} is outdated (found: {version}, required: {minVersion}), please run the migrations to ensure the module works correctly.");
            }
        }
    }
}
