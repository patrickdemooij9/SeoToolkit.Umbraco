using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectGuidIdMigration : AsyncMigrationBase
    {
        public RedirectGuidIdMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitRedirects", "Key"))
            {
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD [Key] UNIQUEIDENTIFIER NULL");
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute("DROP INDEX IX_SeoToolkitOldUrl");
                Database.Execute("DROP INDEX IX_SeoToolkitRegex");
                foreach (var entry in Database.Fetch<RedirectEntity>(Sql().SelectAll().From<RedirectEntity>()))
                {
                    Database.Execute("UPDATE SeoToolkitRedirects SET [Key] = @0 WHERE Id = @1",
                        Guid.NewGuid(), entry.Id);
                }

                MigrationHelper.RecreateTable<RedirectEntity>(Database, Create, Sql(), "SeoToolkitRedirects");
                return Task.CompletedTask;
            }

            Database.Execute("UPDATE SeoToolkitRedirects SET [Key] = NEWID()");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ALTER COLUMN [Key] UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitRedirects DROP CONSTRAINT pk_SeoToolkitRedirects");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD CONSTRAINT pk_SeoToolkitRedirects PRIMARY KEY ([Key])");
            return Task.CompletedTask;
        }
    }
}
