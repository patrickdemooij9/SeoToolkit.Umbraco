using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class CommonIdToGuidMigration : AsyncMigrationBase
    {
        private readonly IDomainService _domainService;
        private readonly IContentTypeService _contentTypeService;

        private readonly Dictionary<int, Guid> _newDomainMapping;

        public CommonIdToGuidMigration(IMigrationContext context, IDomainService domainService, IContentTypeService contentTypeService) : base(context)
        {
            _domainService = domainService;
            _contentTypeService = contentTypeService;

            _newDomainMapping = new Dictionary<int, Guid>();
        }

        protected override Task MigrateAsync()
        {
            MigrateAllData();
            MigrateRobotsTxt();
            MigrateScriptManager();
            return Task.CompletedTask;
        }

        private void MigrateAllData()
        {
            var firstEntryData = GetFirstRecord("SeoToolkitDomainCollections", "Id");
            if (firstEntryData is not null && Guid.TryParse(firstEntryData.Id.ToString(), out Guid _))
            {
                return;
            }

            var domainCollectionData = Database.Fetch<OldSeoDomainCollectionEntity>(Sql().SelectAll().From<OldSeoDomainCollectionEntity>());
            var domainData = Database.Fetch<OldSeoDomainEntity>(Sql().SelectAll().From<OldSeoDomainEntity>());
            var domainSettingData = Database.Fetch<OldSeoDomainSettingEntity>(Sql().SelectAll().From<OldSeoDomainSettingEntity>());
            var seoKeyValueData = Database.Fetch<OldSeoKeyValueEntity>(Sql().SelectAll().From<OldSeoKeyValueEntity>());
            var seoSettingsData = Database.Fetch<OldSeoSettingsEntity>(Sql().SelectAll().From<OldSeoSettingsEntity>());

            // Rename old tables to back them up
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute("DROP TABLE IF EXISTS old_SeoToolkitDomains");
                Database.Execute("DROP TABLE IF EXISTS old_SeoToolkitDomainSettings");
                Database.Execute("DROP TABLE IF EXISTS old_SeoToolkitDomainCollections");
                Database.Execute("DROP TABLE IF EXISTS old_SeoToolkitSeoKeyValues");
                Database.Execute("DROP TABLE IF EXISTS old_SeoToolkitSeoSettings");

                Database.Execute("ALTER TABLE SeoToolkitDomains RENAME TO old_SeoToolkitDomains");
                Database.Execute("ALTER TABLE SeoToolkitDomainSettings RENAME TO old_SeoToolkitDomainSettings");
                Database.Execute("ALTER TABLE SeoToolkitDomainCollections RENAME TO old_SeoToolkitDomainCollections");
                Database.Execute("ALTER TABLE SeoToolkitSeoKeyValues RENAME TO old_SeoToolkitSeoKeyValues");
                Database.Execute("ALTER TABLE SeoToolkitSeoSettings RENAME TO old_SeoToolkitSeoSettings");

                Database.Execute("DROP INDEX IF EXISTS IX_SeoToolkitOldUrl");
                Database.Execute("DROP INDEX IF EXISTS IX_SeoToolkitRegex");
                Database.Execute("DROP INDEX IF EXISTS IX_SeoToolkitDomainsCollectionId");
                Database.Execute("DROP INDEX IF EXISTS IX_SeoToolkitSettingsCollectionId");
            }
            else
            {
                Database.Execute("DROP TABLE IF EXISTS SeoToolkitDomains");
                Database.Execute("DROP TABLE IF EXISTS SeoToolkitDomainSettings");
                Database.Execute("DROP TABLE IF EXISTS SeoToolkitDomainCollections");
                Database.Execute("DROP TABLE IF EXISTS SeoToolkitSeoKeyValues");
                Database.Execute("DROP TABLE IF EXISTS SeoToolkitSeoSettings");
            }

            Create.Table<SeoDomainCollectionEntity>().Do();
            Create.Table<SeoDomainEntity>().Do();
            Create.Table<SeoDomainSettingEntity>().Do();
            Create.Table<SeoKeyValueEntity>().Do();
            Create.Table<SeoSettingsEntity>().Do();

            foreach (var domainCollection in domainCollectionData)
            {
                var newId = Guid.NewGuid();
                var entity = new SeoDomainCollectionEntity
                {
                    Id = newId,
                    Name = domainCollection.Name
                };
                Database.Insert(entity);
                _newDomainMapping.Add(domainCollection.Id, newId);

                foreach (var domain in domainData.Where(x => x.CollectionId == domainCollection.Id))
                {
                    var umbracoDomain = _domainService.GetById(domain.DomainId);
                    if (umbracoDomain is null) continue;

                    var domainEntity = new SeoDomainEntity
                    {
                        Id = Guid.NewGuid(),
                        DomainId = umbracoDomain.Id,
                        CollectionId = newId
                    };
                    Database.Insert(domainEntity);
                }

                foreach (var setting in domainSettingData.Where(x => x.CollectionId == domainCollection.Id))
                {
                    var settingEntity = new SeoDomainSettingEntity
                    {
                        Id = Guid.NewGuid(),
                        Key = setting.Key,
                        Value = setting.Value,
                        CollectionId = newId
                    };
                    Database.Insert(settingEntity);
                }
            }

            foreach (var seoKeyValue in seoKeyValueData)
            {
                var domainValue = seoKeyValue.DomainId;
                Guid? domainId = null;

                Guid value = Guid.Empty; // Declare 'value' outside the if statement to ensure it's definitely assigned
                if (domainValue is int intParsed && _newDomainMapping.TryGetValue(intParsed, out value))
                {
                    domainId = value;
                }
                else if (domainValue is Guid guidParsed)
                {
                    domainId = guidParsed;
                }

                var seoKeyValueEntity = new SeoKeyValueEntity
                {
                    Id = Guid.NewGuid(),
                    Key = seoKeyValue.Key,
                    Value = seoKeyValue.Value,
                    DomainId = domainId
                };
                Database.Insert(seoKeyValueEntity);
            }

            foreach (var seoSettings in seoSettingsData)
            {
                IContentType? contentType = null;
                if (seoSettings.ContentTypeId is int contentTypeIdInt)
                {
                    contentType = _contentTypeService.Get(contentTypeIdInt);
                } else if (seoSettings.ContentTypeId is Guid guidParsed)
                {
                    contentType = _contentTypeService.Get(guidParsed);
                }
                if (contentType is null) continue;

                var seoSettingsEntity = new SeoSettingsEntity
                {
                    ContentTypeId = contentType.Key,
                    Enabled = seoSettings.Enabled
                };
                Database.Insert(seoSettingsEntity);
            }
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

        private void MigrateRobotsTxt()
        {
            if (!TableExists("SeoToolkitRobotsTxt")) return;

            var robotsTxtData = Database.Fetch<OldRobotsTxtEntity>(Sql().SelectAll().From<OldRobotsTxtEntity>());
            Database.Execute("DROP TABLE IF EXISTS SeoToolkitRobotsTxt");
            Create.Table<NewRobotsTxtEntity>().Do();

            // This needs to happen here because it needs the context of the new domain mapping
            foreach (var robotsTxt in robotsTxtData)
            {
                var domainGuid = robotsTxt.DomainId.HasValue && _newDomainMapping.TryGetValue(robotsTxt.DomainId.Value, out Guid value)
                    ? value : (Guid?)null;

                var robotsTxtEntity = new NewRobotsTxtEntity
                {
                    Id = robotsTxt.Id,
                    Content = robotsTxt.Content,
                    DomainId = domainGuid
                };
                Database.Insert(robotsTxtEntity);
            }
        }

        private void MigrateScriptManager()
        {
            if (!TableExists("SeoToolkitScript")) return;

            var scriptData = Database.Fetch<OldScriptEntity>(Sql().SelectAll().From<OldScriptEntity>());
            Database.Execute("DROP TABLE IF EXISTS SeoToolkitScript");
            Create.Table<NewScriptEntity>().Do();

            // This needs to happen here because it needs the context of the new domain mapping
            foreach (var script in scriptData)
            {
                var domainGuid = script.DomainId.HasValue && _newDomainMapping.TryGetValue(script.DomainId.Value, out Guid value)
                    ? value : (Guid?)null;

                var scriptEntity = new NewScriptEntity
                {
                    Id = script.Id,
                    Name = script.Name,
                    DefinitionAlias = script.DefinitionAlias,
                    Config = script.Config,
                    DomainId = domainGuid
                };
                Database.Insert(scriptEntity);
            }
        }

        [TableName("SeoToolkitDomainCollections")]
        private class OldSeoDomainCollectionEntity
        {
            [Column("Id")]
            public int Id { get; set; }

            [Column("Name")]
            public string Name { get; set; }
        }

        [TableName("SeoToolkitDomains")]
        private class OldSeoDomainEntity
        {
            [Column("Id")]
            public int Id { get; set; }

            [Column("DomainId")]
            public int DomainId { get; set; }

            [Column("CollectionId")]
            public int CollectionId { get; set; }
        }

        [TableName("SeoToolkitDomainSettings")]
        private class OldSeoDomainSettingEntity
        {
            [Column("Id")]
            public int Id { get; set; }

            [Column("SettingKey")]
            public string Key { get; set; }

            [Column("SettingValue")]
            public string Value { get; set; }

            [Column("CollectionId")]
            public int CollectionId { get; set; }
        }

        [TableName("SeoToolkitSeoKeyValues")]
        private class OldSeoKeyValueEntity
        {
            [Column("Key")]
            public string Key { get; set; }

            [Column("Value")]
            public string Value { get; set; }

            [Column("DomainId")]
            public object? DomainId { get; set; }
        }

        [TableName("SeoToolkitSeoSettings")]
        private class OldSeoSettingsEntity
        {
            [Column("ContentTypeId")]
            public object ContentTypeId { get; set; }

            [Column("Enabled")]
            public bool Enabled { get; set; }
        }

        [TableName("SeoToolkitRobotsTxt")]
        [PrimaryKey("Id", AutoIncrement = true)]
        private class OldRobotsTxtEntity
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("Content")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string Content { get; set; }

            [Column("DomainId")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public int? DomainId { get; set; }
        }

        [TableName("SeoToolkitRobotsTxt")]
        [PrimaryKey("Id", AutoIncrement = true)]
        private class NewRobotsTxtEntity
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("Content")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string Content { get; set; }

            [Column("DomainId")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public Guid? DomainId { get; set; }
        }

        [TableName("SeoToolkitScript")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        private class OldScriptEntity
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("Name")]
            public string Name { get; set; }

            [Column("DefinitionAlias")]
            public string DefinitionAlias { get; set; }

            [Column("Config")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string Config { get; set; }

            [Column("DomainId")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public int? DomainId { get; set; }
        }

        [TableName("SeoToolkitScript")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        private class NewScriptEntity
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("Name")]
            public string Name { get; set; }

            [Column("DefinitionAlias")]
            public string DefinitionAlias { get; set; }

            [Column("Config")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string Config { get; set; }

            [Column("DomainId")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public Guid? DomainId { get; set; }
        }
    }
}
