using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class RobotsTxtGuidIdMigration : AsyncMigrationBase
    {
        public RobotsTxtGuidIdMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitRobotsTxt", "Key"))
            {
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitRobotsTxt ADD Key UNIQUEIDENTIFIER NULL");
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                foreach (var entry in Database.Fetch<RobotsTxtEntity>(Sql().SelectAll().From<RobotsTxtEntity>()))
                {
                    Console.WriteLine(entry.Id);
                    Database.Execute("UPDATE SeoToolkitRobotsTxt SET Key = @0 WHERE Id = @1",
                        Guid.NewGuid(), entry.Id);
                }

                MigrationHelper.RecreateTable<RobotsTxtEntity>(Database, Create, Sql(), "SeoToolkitRobotsTxt");
                return Task.CompletedTask;
            }

            Database.Execute("UPDATE SeoToolkitRobotsTxt SET [Key] = NEWID()");
            Database.Execute("ALTER TABLE SeoToolkitRobotsTxt ALTER COLUMN [Key] UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitRobotsTxt DROP CONSTRAINT pk_SeoToolkitRobotsTxt");
            Database.Execute("ALTER TABLE SeoToolkitRobotsTxt ADD CONSTRAINT pk_SeoToolkitRobotsTxt PRIMARY KEY ([Key])");
            return Task.CompletedTask;
        }
    }
}
