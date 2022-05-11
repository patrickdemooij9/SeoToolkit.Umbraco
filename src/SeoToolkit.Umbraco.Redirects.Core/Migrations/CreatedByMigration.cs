using System;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    internal class CreatedByMigration : MigrationBase
    {
        public CreatedByMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("SeoToolkitRedirects", "CreatedBy"))
            {
                Alter.Table("SeoToolkitRedirects").AddColumn("CreatedBy").AsInt32().Nullable().Do();
                Update.Table("SeoToolkitRedirects").Set(new {CreatedBy = -1}).AllRows().Do();
                Alter.Table("SeoToolkitRedirects").AlterColumn("CreatedBy").AsInt32().NotNullable().Do();
            }
        }
    }
}
