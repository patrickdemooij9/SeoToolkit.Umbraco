using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectGuidIdMigration : AsyncMigrationBase
    {
        private readonly IKeyValueService _keyValueService;
        private readonly IContentService _contentService;

        public RedirectGuidIdMigration(IMigrationContext context, IKeyValueService keyValueService, IContentService contentService) : base(context)
        {
            _keyValueService = keyValueService;
            _contentService = contentService;
        }

        protected override Task MigrateAsync()
        {
            // We have a dependency on the common migration to have run first, otherwise the cleanup will be a big mess
            MigrationHelper.EnsureMigration("SeoToolkit_Common_Migration", 6, _keyValueService);

            if (ColumnExists("SeoToolkitRedirects", "Key"))
            {
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD [Key] UNIQUEIDENTIFIER NULL");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD NewNodeKey UNIQUEIDENTIFIER NULL");
            var redirects = Database.Fetch<RedirectEntity>(Sql().SelectAll().From<RedirectEntity>());
            foreach (var entry in redirects)
            {
                if (DatabaseType == NPoco.DatabaseType.SQLite)
                {
                    Database.Execute("UPDATE SeoToolkitRedirects SET [Key] = @0 WHERE Id = @1", Guid.NewGuid(), entry.Id);
                }

                if (entry.NewNodeId.HasValue)
                {
                    var node = _contentService.GetById(entry.NewNodeId.Value);
                    if (node != null)
                    {
                        Database.Execute("UPDATE SeoToolkitRedirects SET NewNodeKey = @0 WHERE Id = @1", node.Key, entry.Id);
                    }
                }
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute("DROP INDEX IX_SeoToolkitOldUrl");
                Database.Execute("DROP INDEX IX_SeoToolkitRegex");

                MigrationHelper.RecreateTable<RedirectEntity>(Database, Create, Sql(), "SeoToolkitRedirects");
                return Task.CompletedTask;
            }

            Database.Execute("UPDATE SeoToolkitRedirects SET [Key] = NEWID()");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ALTER COLUMN [Key] UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitRedirects DROP CONSTRAINT pk_SeoToolkitRedirects");
            Database.Execute("ALTER TABLE SeoToolkitRedirects ADD CONSTRAINT pk_SeoToolkitRedirects PRIMARY KEY ([Key])");
            return Task.CompletedTask;
        }
    }
}
