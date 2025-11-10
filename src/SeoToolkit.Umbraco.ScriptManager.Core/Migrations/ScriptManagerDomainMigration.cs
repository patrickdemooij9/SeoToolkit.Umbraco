using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerDomainMigration : AsyncMigrationBase
    {
        public ScriptManagerDomainMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitScript", "DomainId"))
            {
                return Task.CompletedTask;
            }

            Database.Execute($"ALTER TABLE SeoToolkitScript ADD COLUMN DomainId INT NULL");
            return Task.CompletedTask;
        }
    }
}
