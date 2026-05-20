using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Migrations.Entities;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Collections.TopoGraph;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectGuidIdMigration : AsyncMigrationBase
    {
        private readonly IKeyValueService _keyValueService;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;

        public RedirectGuidIdMigration(IMigrationContext context, IKeyValueService keyValueService, IContentService contentService, IMediaService mediaService) : base(context)
        {
            _keyValueService = keyValueService;
            _contentService = contentService;
            _mediaService = mediaService;
        }

        protected override Task MigrateAsync()
        {
            // We have a dependency on the common migration to have run first, otherwise the cleanup will be a big mess
            MigrationHelper.EnsureMigration("SeoToolkit_Common_Migration", 6, _keyValueService);

            if (ColumnExists("SeoToolkitRedirects", "Key"))
            {
                return Task.CompletedTask;
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute("ALTER TABLE SeoToolkitRedirects ADD \"Key\" TEXT NULL");
                Database.Execute("ALTER TABLE SeoToolkitRedirects ADD NewNodeKey TEXT NULL");
            }
            else
            {
                Database.Execute("ALTER TABLE SeoToolkitRedirects ADD [Key] UNIQUEIDENTIFIER NULL");
                Database.Execute("ALTER TABLE SeoToolkitRedirects ADD NewNodeKey UNIQUEIDENTIFIER NULL");
            }

            var redirects = Database.Fetch<RedirectCreatedByGuidEntity>(Sql().SelectAll().From<RedirectCreatedByGuidEntity>());
            foreach (var entry in redirects)
            {
                if (DatabaseType == NPoco.DatabaseType.SQLite)
                {
                    Database.Execute("UPDATE SeoToolkitRedirects SET \"Key\" = @0 WHERE Id = @1", Guid.NewGuid(), entry.Id);
                }

                if (entry.NewNodeId.HasValue)
                {
                    if (entry.NewNodeCultureId.HasValue)
                    {
                        var node = _contentService.GetById(entry.NewNodeId.Value);
                        if (node != null)
                        {
                            Database.Execute("UPDATE SeoToolkitRedirects SET NewNodeKey = @0 WHERE Id = @1", node.Key, entry.Id);
                        }
                    }
                    else
                    {
                        var mediaNode = _mediaService.GetById(entry.NewNodeId.Value);
                        if (mediaNode != null)
                        {
                            Database.Execute("UPDATE SeoToolkitRedirects SET NewNodeKey = @0 WHERE Id = @1", mediaNode.Key, entry.Id);
                        }
                    }
                }
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                // For SQLite, the columns have been added and populated above.
                // RecreateTable is intentionally avoided here: if InsertBulk were to fail after
                // the table rename, the newly-created empty table would survive (ColumnExists("Key")
                // would return true) and all redirects would appear deleted on the next startup.
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
