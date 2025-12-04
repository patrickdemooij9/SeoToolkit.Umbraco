using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.MetaFields.Core.Migrations.Entities._5_0_0;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsGuidIdMigration : AsyncMigrationBase
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentService _contentService;
        private readonly IKeyValueService _keyValueService;

        public MetaFieldsGuidIdMigration(IMigrationContext context, IContentTypeService contentTypeService, IContentService contentService, IKeyValueService keyValueService) : base(context)
        {
            _contentTypeService = contentTypeService;
            _contentService = contentService;
            _keyValueService = keyValueService;
        }

        protected override Task MigrateAsync()
        {
            // We have a dependency on the common migration to have run first, otherwise the cleanup will be a big mess
            MigrationHelper.EnsureMigration("SeoToolkit_Common_Migration", 6, _keyValueService);

            MigrateMetaFieldsValueEntity();
            MigrateMetaFieldsSettingsEntity();
            return Task.CompletedTask;
        }

        private void MigrateMetaFieldsSettingsEntity()
        {
            if (ColumnExists("SeoToolkitMetaFieldsSettings", "NodeKey"))
            {
                return;
            }

            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsSettings ADD NodeKey UNIQUEIDENTIFIER NULL");
            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsSettings ADD InheritanceKey UNIQUEIDENTIFIER NULL");
            foreach (var entry in Database.Fetch<MetaFieldsSettingsEntity_5>(Sql().SelectAll().From<MetaFieldsSettingsEntity_5>()))
            {
                var content = _contentTypeService.Get(entry.NodeId);
                if (content is null)
                {
                    Database.Delete(entry);
                    continue;
                }
                if (entry.InheritanceId.HasValue)
                {
                    var inheritanceNode = _contentTypeService.Get(entry.InheritanceId.Value);
                    if (inheritanceNode != null)
                    {
                        Database.Execute("UPDATE SeoToolkitMetaFieldsSettings SET InheritanceKey = @0 WHERE InheritanceId = @1", inheritanceNode.Key, entry.InheritanceId.Value);
                    }
                }

                Database.Execute("UPDATE SeoToolkitMetaFieldsSettings SET NodeKey = @0 WHERE NodeId = @1",
                    content.Key, entry.NodeId);
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                MigrationHelper.RecreateTable<MetaFieldsSettingsEntity>(Database, Create, Sql(), "SeoToolkitMetaFieldsSettings");
                return;
            }

            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsSettings ALTER COLUMN NodeKey UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsSettings DROP CONSTRAINT pk_SeoToolkitMetaFieldsSettings");
            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsSettings ADD CONSTRAINT pk_SeoToolkitMetaFieldsSettings PRIMARY KEY (NodeKey)");
        }

        private void MigrateMetaFieldsValueEntity()
        {
            if (ColumnExists("SeoToolkitMetaFieldsValue", "NodeKey"))
            {
                return;
            }

            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsValue ADD NodeKey UNIQUEIDENTIFIER NULL");
            foreach (var entry in Database.Fetch<MetaFieldsValueEntity_5>(Sql().SelectAll().From<MetaFieldsValueEntity_5>()))
            {
                var content = _contentService.GetById(entry.NodeId);
                if (content is null)
                {
                    Database.Delete(entry);
                    continue;
                }

                Database.Execute("UPDATE SeoToolkitMetaFieldsValue SET NodeKey = @0 WHERE NodeId = @1",
                    content.Key, entry.NodeId);
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                MigrationHelper.RecreateTable<MetaFieldsValueEntity>(Database, Create, Sql(), "SeoToolkitMetaFieldsValue");
                return;
            }

            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsValue ALTER COLUMN NodeKey UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsValue DROP CONSTRAINT pk_SeoToolkitMetaFieldsValue");
            Database.Execute("ALTER TABLE SeoToolkitMetaFieldsValue ADD CONSTRAINT pk_SeoToolkitMetaFieldsValue PRIMARY KEY (NodeKey,Alias,Culture)");
        }
    }
}
