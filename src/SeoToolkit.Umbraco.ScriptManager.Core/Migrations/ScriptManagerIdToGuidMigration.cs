using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Database;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerIdToGuidMigration : AsyncMigrationBase
    {
        private readonly IKeyValueService _keyValueService;

        public ScriptManagerIdToGuidMigration(IMigrationContext context, IKeyValueService keyValueService) : base(context)
        {
            _keyValueService = keyValueService;
        }

        protected override Task MigrateAsync()
        {
            // We have a dependency on the common migration to have run first, otherwise the cleanup will be a big mess
            MigrationHelper.EnsureMigration("SeoToolkit_Common_Migration", 6, _keyValueService);

            if (ColumnExists("SeoToolkitScript", "Key"))
            {
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitScript ADD [Key] UNIQUEIDENTIFIER NULL");
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                foreach (var entry in Database.Fetch<ScriptEntity>(Sql().SelectAll().From<ScriptEntity>()))
                {
                    Database.Execute("UPDATE SeoToolkitScript SET [Key] = @0 WHERE Id = @1",
                        Guid.NewGuid(), entry.Id);
                }

                MigrationHelper.RecreateTable<ScriptEntity>(Database, Create, Sql(), "SeoToolkitScript");
                return Task.CompletedTask;
            }

            Database.Execute("UPDATE SeoToolkitScript SET [Key] = NEWID()");
            Database.Execute("ALTER TABLE SeoToolkitScript ALTER COLUMN [Key] UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitScript DROP CONSTRAINT pk_SeoToolkitScript");
            Database.Execute("ALTER TABLE SeoToolkitScript ADD CONSTRAINT pk_SeoToolkitScript PRIMARY KEY ([Key])");
            return Task.CompletedTask;
        }
    }
}
