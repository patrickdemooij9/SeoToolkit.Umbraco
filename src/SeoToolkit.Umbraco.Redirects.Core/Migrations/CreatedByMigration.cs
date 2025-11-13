using NPoco;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using System;
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
                if (DatabaseType == DatabaseType.SQLite)
                {
                    Database.Execute("DROP INDEX IX_SeoToolkitOldUrl");
                    Database.Execute("DROP INDEX IX_SeoToolkitRegex");
                    Database.Execute("ALTER TABLE SeoToolkitRedirects ADD CreatedBy INT NULL");
                    Database.Execute("UPDATE SeoToolkitRedirects SET CreatedBy = -1");

                    MigrationHelper.RecreateTable<RedirectEntity>(Database, Create, Sql(), "SeoToolkitRedirects");
                    return;
                }

                Alter.Table("SeoToolkitRedirects").AddColumn("CreatedBy").AsInt32().Nullable().Do();
                Update.Table("SeoToolkitRedirects").Set(new {CreatedBy = -1}).AllRows().Do();
                Alter.Table("SeoToolkitRedirects").AlterColumn("CreatedBy").AsInt32().NotNullable().Do();
            }
        }
    }
}
