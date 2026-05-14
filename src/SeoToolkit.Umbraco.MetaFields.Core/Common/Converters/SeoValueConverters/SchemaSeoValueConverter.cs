using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class SchemaSeoValueConverter : ISeoValueConverter
    {
        private readonly SchemaResolverCollection _schemaResolvers;
        private readonly ISchemaEntryService _schemaEntryService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SchemaSeoValueConverter(SchemaResolverCollection schemaResolvers, ISchemaEntryService schemaEntryService, IUmbracoContextFactory umbracoContextFactory)
        {
            _schemaResolvers = schemaResolvers;
            _schemaEntryService = schemaEntryService;
            _umbracoContextFactory = umbracoContextFactory;
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

        private string ResolveValue(Dictionary<string, SchemaPropertyValue> properties, Common.SchemaResolvers.SchemaProperty propertyDef, IPublishedContent currentContent)
        {
            if (properties is null || properties.TryGetValue(propertyDef.Alias, out var property) == false || property is null)
                return string.Empty;

            if (property.IsReference)
                return ResolveReference(property.ReferenceKey, currentContent);

            // Resolve media picker GUIDs to absolute URLs
            //TODO: Handle this correctly with the other converters
            /*if (propertyDef.PropertyEditor == "Umb.PropertyEditorUi.MediaPicker"
                && !string.IsNullOrWhiteSpace(property.Value)
                && Guid.TryParse(property.Value, out var mediaGuid))
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                var mediaItem = ctx.UmbracoContext.Media.GetById(true, mediaGuid);
                if (mediaItem != null)
                    return mediaItem.Url(mode: UrlMode.Absolute);
            }*/

            return property.Value?.ToString() ?? string.Empty;
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
