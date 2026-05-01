using NPoco;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Migrations.Entities;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using System;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    internal class IsEnabledMigration : AsyncMigrationBase
    {
        public IsEnabledMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!ColumnExists("SeoToolkitRedirects", "IsEnabled"))
            {
                if (DatabaseType == DatabaseType.SQLite)
                {
                    Database.Execute("DROP INDEX IF EXISTS IX_SeoToolkitOldUrl");
                    Database.Execute("DROP INDEX IF EXISTS IX_SeoToolkitRegex");
                    Database.Execute("ALTER TABLE SeoToolkitRedirects ADD IsEnabled BIT NULL");
                    Database.Execute("UPDATE SeoToolkitRedirects SET IsEnabled = 1");

                    MigrationHelper.RecreateTable<IsEnabledEntity>(Database, Create, Sql(), "SeoToolkitRedirects");
                    return Task.CompletedTask;
                }

                Alter.Table("SeoToolkitRedirects").AddColumn("IsEnabled").AsBoolean().Nullable().Do();
                Update.Table("SeoToolkitRedirects").Set(new { IsEnabled = true}).AllRows().Do();
                Alter.Table("SeoToolkitRedirects").AlterColumn("IsEnabled").AsBoolean().NotNullable().Do();
            }
            return Task.CompletedTask;
        }
    }
}
