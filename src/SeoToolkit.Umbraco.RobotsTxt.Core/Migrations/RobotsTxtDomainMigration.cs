using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class RobotsTxtDomainMigration : AsyncMigrationBase
    {
        public RobotsTxtDomainMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitRobotsTxt", "DomainId"))
            {
                return Task.CompletedTask;
            }

            Alter.Table("SeoToolkitRobotsTxt").AddColumn("DomainId").AsInt32().Nullable().Do();
            return Task.CompletedTask;
        }
    }
}
