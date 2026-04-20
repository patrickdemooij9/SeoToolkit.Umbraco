using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class SchemaEntryTableMigration : MigrationBase
    {
        public SchemaEntryTableMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("SeoToolkitSchemaEntry"))
            {
                Create.Table<SchemaEntryEntity>().Do();
            }

            MigrateExistingSchemaValues();
        }

        private void MigrateExistingSchemaValues()
        {
            var schemaValues = Database.Fetch<MetaFieldsValueEntity>(
                Sql().SelectAll().From<MetaFieldsValueEntity>()
                    .Where<MetaFieldsValueEntity>(it => it.Alias == "schema"));

            foreach (var schemaValue in schemaValues)
            {
                if (string.IsNullOrWhiteSpace(schemaValue.UserValue))
                    continue;

                // Skip if already migrated (value is a JSON array of GUIDs)
                if (IsAlreadyMigrated(schemaValue.UserValue))
                    continue;

                try
                {
                    var schemas = ParseOldSchemaValue(schemaValue.UserValue);
                    if (schemas == null || schemas.Length == 0)
                        continue;

                    var guids = new List<Guid>();
                    foreach (var schema in schemas)
                    {
                        if (schema?.SchemaAlias is null)
                            continue;

                        var id = Guid.NewGuid();
                        Database.Insert(new SchemaEntryEntity
                        {
                            Id = id,
                            OwnerType = "content",
                            OwnerKey = schemaValue.NodeKey,
                            SchemaAlias = schema.SchemaAlias,
                            PropertiesJson = JsonConvert.SerializeObject(schema.Properties)
                        });
                        guids.Add(id);
                    }

                    schemaValue.UserValue = JsonConvert.SerializeObject(guids);
                    Database.Update(schemaValue);
                }
                catch
                {
                    // If parsing fails, leave the value as-is to avoid data loss
                }
            }
        }

        private static bool IsAlreadyMigrated(string userValue)
        {
            try
            {
                var token = JToken.Parse(userValue);
                if (token is JArray arr && arr.Count > 0)
                {
                    // If the first element is a GUID string, it's already migrated
                    var first = arr[0];
                    if (first.Type == JTokenType.String && Guid.TryParse(first.Value<string>(), out _))
                        return true;

                    // If the first element is an empty array or the array is empty, it's migrated
                    if (arr.Count == 0)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static SchemaEditorModel[] ParseOldSchemaValue(string userValue)
        {
            try
            {
                var token = JToken.Parse(userValue);

                if (token is JObject obj && obj["schemas"] is JToken schemasToken)
                    return schemasToken.ToObject<SchemaEditorModel[]>();

                if (token is JArray arr)
                    return arr.ToObject<SchemaEditorModel[]>();

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
