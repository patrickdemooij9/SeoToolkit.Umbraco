using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

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

            //TODO: Implement SQLLite compatibility
            Database.Execute("ALTER TABLE SeoToolkitRobotsTxt ADD COLUMN [Key] UNIQUEIDENTIFIER NULL");
            Database.Execute("UPDATE SeoToolkitRobotsTxt SET [Key] = NEWID()");
            Database.Execute("ALTER TABLE SeoToolkitRobotsTxt ALTER COLUMN [Key] UNIQUEIDENTIFIER NOT NULL");
            return Task.CompletedTask;
        }
    }
}
