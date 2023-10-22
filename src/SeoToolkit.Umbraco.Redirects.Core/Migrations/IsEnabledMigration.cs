using System;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    internal class IsEnabledMigration : MigrationBase
    {
        public IsEnabledMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("SeoToolkitRedirects", "IsEnabled"))
            {
                Alter.Table("SeoToolkitRedirects").AddColumn("IsEnabled").AsBoolean().Nullable().Do();
                Update.Table("SeoToolkitRedirects").Set(new { IsEnabled = true}).AllRows().Do();
                Alter.Table("SeoToolkitRedirects").AlterColumn("IsEnabled").AsBoolean().NotNullable().Do();
            }
        }
    }
}
