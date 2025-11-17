using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerIdToGuidMigration : AsyncMigrationBase
    {
        public ScriptManagerIdToGuidMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
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
