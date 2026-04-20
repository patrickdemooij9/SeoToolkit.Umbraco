using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
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
            if (value is not Guid[] guids || guids.Length == 0)
                return Array.Empty<IThing>();

            var entries = _schemaEntryService.GetByIds(guids);
            var schemas = new List<IThing>();

            foreach (var entry in entries)
            {
                if (entry?.SchemaAlias is null)
                    continue;

                var resolver = _schemaResolvers.FirstOrDefault(it => it.Alias.Equals(entry.SchemaAlias, StringComparison.OrdinalIgnoreCase));
                if (resolver is null)
                    continue;

                var values = resolver.Properties.ToDictionary(
                    keySelector: property => property.Alias,
                    elementSelector: property => ResolveValue(entry.Properties, property.Alias, currentContent));

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

        private static string ResolveValue(Dictionary<string, SchemaPropertyValue> properties, string alias, IPublishedContent currentContent)
        {
            if (properties is null || properties.TryGetValue(alias, out var property) == false || property is null)
                return string.Empty;

            if (property.IsReference)
                return ResolveReference(property.ReferenceKey, currentContent);

            return property.Value ?? string.Empty;
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
