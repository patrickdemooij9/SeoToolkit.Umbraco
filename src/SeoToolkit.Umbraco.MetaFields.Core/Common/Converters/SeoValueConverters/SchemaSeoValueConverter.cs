using Microsoft.Extensions.Logging;
using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
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
        private readonly ILogger<SchemaSeoValueConverter> _logger;

        public SchemaSeoValueConverter(SchemaResolverCollection schemaResolvers, ISchemaEntryService schemaEntryService, ILogger<SchemaSeoValueConverter> logger)
        {
            _schemaResolvers = schemaResolvers;
            _schemaEntryService = schemaEntryService;
            _logger = logger;
        }

        public Type FromValue => typeof(Guid[]);
        public Type ToValue => typeof(IThing[]);

        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            // Resolve the entries referenced by the field value.
            IEnumerable<SchemaEntryDto> allEntries = ResolveEntries(value);

            if (currentContent?.ContentType?.Key is Guid docTypeKey)
            {
                // Only auto-render document type schemas that are enabled for rendering.
                // Disabled ones remain available to be referenced explicitly on content level.
                var docTypeEntries = _schemaEntryService.GetAll(SchemaOwnerTypeConstants.DocumentType, docTypeKey)
                    .Where(entry => entry.RenderAutomatically);
                allEntries = docTypeEntries.Concat(allEntries);
            }

            // Website-wide schemas marked for rendering are added to every page.
            var websiteEntries = _schemaEntryService
                .GetAll(SchemaOwnerTypeConstants.Website, SchemaOwnerTypeConstants.WebsiteOwnerKey)
                .Where(entry => entry.RenderAutomatically);
            allEntries = websiteEntries.Concat(allEntries);

            // An entry can be reached through more than one path (e.g. a website schema that is
            // also explicitly referenced on the page); only render each distinct entry once.
            allEntries = allEntries.DistinctBy(entry => entry.Id);

            return ConvertEntries(allEntries, currentContent, 0);
        }

        /// <summary>
        /// Resolves a nested schema-editor property value. Unlike <see cref="Convert"/>, this
        /// only resolves the explicitly referenced entries and never concatenates the
        /// auto-rendering document type schemas.
        /// </summary>
        private IThing[] ConvertNested(object value, IPublishedContent currentContent, int level)
            => ConvertEntries(ResolveEntries(value), currentContent, level);

        private IEnumerable<SchemaEntryDto> ResolveEntries(object value)
        {
            var guids = value is Guid[] arr ? arr : Array.Empty<Guid>();

            return guids.Length > 0
                ? _schemaEntryService.GetByIds(guids)
                : Enumerable.Empty<SchemaEntryDto>();
        }

        private IThing[] ConvertEntries(IEnumerable<SchemaEntryDto> entries, IPublishedContent currentContent, int level)
        {
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
                    elementSelector: property => ResolveValue(entry.Properties, property, currentContent, level + 1));

                try
                {
                    var resolvedSchema = resolver.ToSchema(values);
                    if (resolvedSchema != null)
                        schemas.Add(resolvedSchema);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong while converting schema {alias}", entry.SchemaAlias);
                }
            }

            return schemas.ToArray();
        }

        private object ResolveValue(Dictionary<string, SchemaPropertyValue> properties, SchemaProperty propertyDef, IPublishedContent currentContent, int level)
        {
            if (level > 10) return null; //Most likely an infinite loop somewhere

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
                    return ConvertNested(obj, currentContent, level);
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
