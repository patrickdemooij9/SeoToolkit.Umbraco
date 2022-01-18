using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Providers
{
    public class DefaultMetaTagsProvider : IMetaTagsProvider
    {
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;
        private readonly SeoFieldCollection _seoFieldCollection;
        private readonly IMetaFieldsValueService _seoValueService;
        private readonly SeoConverterCollection _seoConverterCollection;
        private readonly ILogger<DefaultMetaTagsProvider> _logger;
        private readonly IProfiler _profiler;

        public event EventHandler<MetaTagsModel> BeforeMetaTagsGet;

        public DefaultMetaTagsProvider(IMetaFieldsSettingsService documentTypeSettingsService,
            SeoFieldCollection seoFieldCollection,
            IMetaFieldsValueService seoValueService,
            SeoConverterCollection seoConverterCollection,
            ILogger<DefaultMetaTagsProvider> logger,
            IProfiler profiler)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
            _seoFieldCollection = seoFieldCollection;
            _seoValueService = seoValueService;
            _seoConverterCollection = seoConverterCollection;
            _logger = logger;
            _profiler = profiler;
        }

        public MetaTagsModel Get(IPublishedContent content)
        {
            using (_profiler.Step("Get MetaTags"))
            {
                var allFields = _seoFieldCollection.GetAll().ToArray();

                //Make sure that the fields are set, otherwise the values cannot be set!
                var metaTags = new MetaTagsModel(allFields.ToDictionary(it => it, it => (object)null));
                BeforeMetaTagsGet?.Invoke(this, metaTags);

                var settings = _documentTypeSettingsService.Get(content.ContentType.Id);
                if (settings?.EnableSeoSettings != true)
                    return null;
                var userValues = _seoValueService.GetUserValues(content.Id);
                var fields = allFields.Select(it =>
                {
                    if (metaTags.GetValue<object>(it.Alias) != null)
                        return null;

                    object intermediateObject = null;
                    if (userValues?.ContainsKey(it.Alias) is true)
                    {
                        var result = it.EditEditor.ValueConverter.ConvertDatabaseToObject(userValues[it.Alias]);
                        if (!it.EditEditor.ValueConverter.IsEmpty(result))
                            intermediateObject = result;
                    }

                    if (intermediateObject is null)
                    {
                        var documentTypeValue = settings.Get(it.Alias);
                        if (documentTypeValue != null && documentTypeValue.UseInheritedValue)
                        {
                            var inheritance = settings.Inheritance;
                            while (inheritance != null)
                            {
                                var inheritedSettings = _documentTypeSettingsService.Get(inheritance.Id);
                                documentTypeValue = inheritedSettings.Get(it.Alias);
                                if (documentTypeValue != null && documentTypeValue.UseInheritedValue)
                                    inheritance = inheritedSettings.Inheritance;
                                else
                                    break;
                            }
                        }
                        intermediateObject = documentTypeValue?.Value;
                    }

                    if (intermediateObject is null)
                        return new SeoValue(it, null);
                    var converter = _seoConverterCollection.GetConverter(intermediateObject.GetType(), it.FieldType);
                    if (!(converter is null))
                        return new SeoValue(it, converter.Convert(intermediateObject, content, it.Alias));

                    _logger.LogWarning("No converter found for conversion {0} to {1}", intermediateObject.GetType(), it.FieldType);
                    return new SeoValue(it, intermediateObject);
                }).WhereNotNull().ToArray();

                foreach (var fieldValue in fields)
                {
                    metaTags.SetValue(fieldValue.Field.Alias, fieldValue.Value);
                }

                return metaTags;
            }
        }
    }
}
