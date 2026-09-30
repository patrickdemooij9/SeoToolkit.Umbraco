using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class SchemaEntryTableMigration : AsyncMigrationBase
    {
        public SchemaEntryTableMigration(IMigrationContext context) : base(context)
        {
        }

        protected override async Task MigrateAsync()
        {
            if (!TableExists("SeoToolkitSchemaEntry"))
            {
                Create.Table<SchemaEntryEntity>().Do();
            }

            await MigrateExistingSchemaValuesAsync();
        }

        private async Task MigrateExistingSchemaValuesAsync()
        {
            var schemaValues = await Database.FetchAsync<MetaFieldsValueEntity>(
                Sql().SelectAll().From<MetaFieldsValueEntity>()
                    .Where<MetaFieldsValueEntity>(it => it.Alias == SeoFieldAliasConstants.Schema));

            foreach (var schemaValue in schemaValues)
            {
                if (string.IsNullOrWhiteSpace(schemaValue.UserValue))
                    continue;

                try
                {
                    var userValue = JsonConvert.DeserializeObject<string>(schemaValue.UserValue);

                    var id = Guid.NewGuid();
                    await Database.InsertAsync(new SchemaEntryEntity
                    {
                        Id = id,
                        OwnerType = "content",
                        OwnerKey = schemaValue.NodeKey,
                        SchemaAlias = "rawJson",
                        PropertiesJson = JsonConvert.SerializeObject(new Dictionary<string, SchemaPropertyValue>()
                        {
                            { "json", new SchemaPropertyValue { Value = userValue } }
                        })
                    });

                    schemaValue.UserValue = JsonConvert.SerializeObject(new[] { id });
                    await Database.UpdateAsync(schemaValue);
                }
                catch
                {
                    // If parsing fails, leave the value as-is to avoid data loss
                }
            }
        }
    }
}
