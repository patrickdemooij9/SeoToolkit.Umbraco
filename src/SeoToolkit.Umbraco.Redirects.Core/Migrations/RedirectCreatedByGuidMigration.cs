using SeoToolkit.Umbraco.Redirects.Core.Migrations.Entities;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectCreatedByGuidMigration : AsyncMigrationBase
    {
        private readonly IUserService _userService;

        public RedirectCreatedByGuidMigration(IMigrationContext context, IUserService userService) : base(context)
        {
            _userService = userService;
        }

        protected override async Task MigrateAsync()
        {
            var firstEntryData = GetFirstRecord("SeoToolkitRedirects", "CreatedBy");
            if (firstEntryData is not null && Guid.TryParse(firstEntryData.CreatedBy.ToString(), out Guid _))
            {
                return;
            }

            var redirects = Database.Fetch<RedirectCreatedByGuidEntity>(Sql().SelectAll().From<RedirectCreatedByGuidEntity>());
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute("ALTER TABLE SeoToolkitRedirects RENAME COLUMN CreatedBy TO CreatedBy_Old");
                Database.Execute("ALTER TABLE SeoToolkitRedirects ADD CreatedBy TEXT NULL");
            }
            else
            {
                Database.Execute("exec sp_rename 'SeoToolkitRedirects.CreatedBy', 'CreatedBy_Old', 'COLUMN'");
                Database.Execute("ALTER TABLE SeoToolkitRedirects ADD CreatedBy UNIQUEIDENTIFIER NULL");
            }

            foreach (var redirect in redirects)
            {
                var createdByKey = Database.Fetch<Guid?>("SELECT [key] FROM umbracoUser where id = @0", [redirect.CreatedBy]).FirstOrDefault();
                if (createdByKey is null) continue;

                Database.Execute("UPDATE SeoToolkitRedirects SET CreatedBy = @0 WHERE [Key] = @1", createdByKey, redirect.Key);
            }
            Database.Execute("ALTER TABLE SeoToolkitRedirects DROP COLUMN CreatedBy_Old");
            return;
        }

        private dynamic GetFirstRecord(string tableName, string columnName)
        {
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                return Database.FirstOrDefault<dynamic>(
                    $"SELECT {columnName} FROM {tableName} LIMIT 1"
                );
            }

            return Database.FirstOrDefault<dynamic>(
                $"SELECT TOP 1 {columnName} FROM {tableName}"
            );
        }
    }
}
