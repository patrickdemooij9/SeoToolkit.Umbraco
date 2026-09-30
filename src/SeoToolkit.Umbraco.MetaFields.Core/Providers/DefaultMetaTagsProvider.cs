using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core.Events;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.MetaFields.Core.Providers
{
    public class DefaultMetaTagsProvider : IMetaTagsProvider
    {
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _seoFieldCollection;
        private readonly IMetaFieldsValueService _seoValueService;
        private readonly SeoConverterCollection _seoConverterCollection;
        private readonly ILogger<DefaultMetaTagsProvider> _logger;
        private readonly IProfiler _profiler;
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IEventAggregator _eventAggregator;

        public DefaultMetaTagsProvider(IMetaFieldsSettingsService documentTypeSettingsService,
            SeoFieldCollection seoFieldCollection,
            IMetaFieldsValueService seoValueService,
            SeoConverterCollection seoConverterCollection,
            ILogger<DefaultMetaTagsProvider> logger,
            IProfiler profiler,
            ISeoSettingsService seoSettingsService,
            IContentTypeService contentTypeService,
            IEventAggregator eventAggregator)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
            _seoFieldCollection = seoFieldCollection;
            _seoValueService = seoValueService;
            _seoConverterCollection = seoConverterCollection;
            _logger = logger;
            _profiler = profiler;
            _seoSettingsService = seoSettingsService;
            _contentTypeService = contentTypeService;
            _eventAggregator = eventAggregator;
        }

        public MetaTagsModel Get(IPublishedContent content, bool includeUserValues)
        {
            using (_profiler.Step("Get MetaTags"))
            {
                var allFields = _seoFieldCollection.GetAll().ToArray();

                //Make sure that the fields are set, otherwise the values cannot be set!
                var metaTags = new MetaTagsModel(allFields.ToDictionary(it => it, it => (object)null));
                _eventAggregator.Publish(new BeforeMetaTagsNotification(content, metaTags));

                var contentType = _contentTypeService.Get(content.ContentType.Key);
                var settings = _documentTypeSettingsService.Get(content.ContentType.Key);
                if (_seoSettingsService.IsEnabled(contentType) != true)
                    return null;
                var userValues = includeUserValues ? _seoValueService.GetUserValues(content.Key) : null;
                var fields = allFields.Select(it =>
                {
                    //If set by event, make sure not to override it with the fallback value
                    if (metaTags.GetValue<object>(it.Alias) != null)
                        return null;

                    object intermediateObject = null;
                    if (userValues?.ContainsKey(it.Alias) is true && userValues[it.Alias] != null)
                    {
                        var result = it.EditEditor.ValueConverter.ConvertDatabaseToObject(userValues[it.Alias]);
                        if (!it.EditEditor.ValueConverter.IsEmpty(result))
                            intermediateObject = result;
                    }

                    if (intermediateObject is null && it.AllowDocumentTypeFallback && settings != null)
                    {
                        var documentTypeValue = settings.Get(it.Alias);
                        if (documentTypeValue != null && documentTypeValue.UseInheritedValue)
                        {
                            var inheritance = settings.Inheritance;
                            while (inheritance != null)
                            {
                                var inheritedSettings = _documentTypeSettingsService.Get(inheritance.Key);
                                documentTypeValue = inheritedSettings?.Get(it.Alias);
                                if (documentTypeValue != null && documentTypeValue.UseInheritedValue)
                                    inheritance = inheritedSettings.Inheritance;
                                else
                                    break;
                            }
                        }
                        intermediateObject = documentTypeValue?.Value;
                    }

                    // For fields that don't fall back to the document type, still run the value
                    // converter on the field's own (possibly empty) value. This lets converters that
                    // merge in extra data - e.g. the schema converter adding document type schemas -
                    // run even when the content has no value of its own.
                    if (intermediateObject is null && !it.AllowDocumentTypeFallback)
                    {
                        var ownValue = userValues?.ContainsKey(it.Alias) is true ? userValues[it.Alias] : null;
                        intermediateObject = it.EditEditor.ValueConverter.ConvertDatabaseToObject(ownValue);
                    }

                    if (intermediateObject is null)
                        return new SeoValue(it, null);
                    var fromType = intermediateObject.GetType();
                    var converter = _seoConverterCollection.GetConverter(fromType, it.FieldType);
                    if (converter is not null)
                        return new SeoValue(it, converter.Convert(intermediateObject, content, it.Alias));

                    if (fromType != it.FieldType)
                    {
                        _logger.LogWarning("No converter found for conversion {fromType} to {fieldType}", fromType, it.FieldType);
                    }
                    return new SeoValue(it, intermediateObject);
                }).WhereNotNull().ToArray();

                foreach (var fieldValue in fields)
                {
                    metaTags.SetValue(fieldValue.Field.Alias, fieldValue.Value);
                }

                _eventAggregator.Publish(new AfterMetaTagsNotification(content, metaTags));

                return metaTags;
            }
        }

        public MetaTagsModel GetEmpty()
        {
            var allFields = _seoFieldCollection.GetAll().ToArray();

            return new MetaTagsModel(allFields.ToDictionary(it => it, it => (object)null));
        }
    }
}
