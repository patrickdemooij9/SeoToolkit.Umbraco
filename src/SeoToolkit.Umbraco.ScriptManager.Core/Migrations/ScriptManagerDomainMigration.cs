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

            Alter.Table("SeoToolkitScript").AddColumn("DomainId").AsInt32().Nullable().Do();
            return Task.CompletedTask;
        }
    }
}
