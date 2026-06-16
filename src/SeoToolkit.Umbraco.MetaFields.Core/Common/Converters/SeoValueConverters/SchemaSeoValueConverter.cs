using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class SchemaSeoValueConverter : ISeoValueConverter
    {
        private readonly SchemaResolverCollection _schemaResolvers;
        private readonly ISchemaEntryService _schemaEntryService;

        public SchemaSeoValueConverter(SchemaResolverCollection schemaResolvers, ISchemaEntryService schemaEntryService)
        {
            _schemaResolvers = schemaResolvers;
            _schemaEntryService = schemaEntryService;
        }

        public Type FromValue => typeof(Guid[]);
        public Type ToValue => typeof(IThing[]);

        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            var guids = value is Guid[] arr ? arr : Array.Empty<Guid>();

            // Load content-level entries
            var contentEntries = guids.Length > 0
                ? _schemaEntryService.GetByIds(guids)
                : Enumerable.Empty<SchemaEntryDto>();

            // Additively combine with document type schemas
            IEnumerable<SchemaEntryDto> allEntries = contentEntries;
            if (currentContent?.ContentType?.Key is Guid docTypeKey)
            {
                var docTypeEntries = _schemaEntryService.GetAll("documentType", docTypeKey);
                allEntries = docTypeEntries.Concat(contentEntries);
            }

            var schemas = new List<IThing>();

            foreach (var entry in allEntries)
            {
                if (entry?.SchemaAlias is null)
                    continue;

                var resolver = _schemaResolvers.FirstOrDefault(it => it.Alias.Equals(entry.SchemaAlias, StringComparison.OrdinalIgnoreCase));
                if (resolver is null)
                    continue;

                var values = resolver.Properties.ToDictionary(
                    keySelector: property => property.Alias,
                    elementSelector: property => ResolveValue(entry.Properties, property, currentContent));

                try
                {
                    var resolvedSchema = resolver.ToSchema(values);
                    if (resolvedSchema != null)
                        schemas.Add(resolvedSchema);
                }
                catch
                {
                    // ignore invalid schema data, continue resolving other schemas
                }
            }

            return schemas.ToArray();
        }

        private object ResolveValue(Dictionary<string, SchemaPropertyValue> properties, Common.SchemaResolvers.SchemaProperty propertyDef, IPublishedContent currentContent)
        {
            if (properties is null || properties.TryGetValue(propertyDef.Alias, out var property) == false || property is null)
                return null;

            if (property.IsReference)
                return ResolveReference(property.ReferenceKey, currentContent);

            if (propertyDef.ValueConverter != null)
            {
                var obj = propertyDef.ValueConverter.ConvertDatabaseToObject(property.Value);
                if (obj is IPublishedContent content)
                    return content.Url(mode: UrlMode.Absolute);
                if (propertyDef.ValueConverter is SchemaEditorValueConverter)
                {
                    return Convert(obj, currentContent, propertyDef.Alias);
                }
                return obj;
            }

            return property.Value;
        }

        private static string ResolveReference(string referenceKey, IPublishedContent currentContent)
        {
            if (currentContent is null)
                return string.Empty;

            return referenceKey switch
            {
                "[PageName]" => currentContent.Name,
                "[PageUrl]" => currentContent.Url(mode: UrlMode.Absolute),
                "[SiteName]" => currentContent.Root()?.Name ?? string.Empty,
                "[SiteUrl]" => currentContent.Root()?.Url(mode: UrlMode.Absolute) ?? string.Empty,
                _ => string.Empty
            };
        }
    }
}
