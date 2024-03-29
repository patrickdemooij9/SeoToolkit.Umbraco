﻿using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders
{
    public class InheritedValueFieldProvider : IFieldProvider
    {
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;

        public InheritedValueFieldProvider(IMetaFieldsSettingsService documentTypeSettingsService)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
        }

        public IEnumerable<FieldItemViewModel> GetFieldItems()
        {
            return new[] {new FieldItemViewModel("Inherited value", "custom-inheritedValue", true)};
        }

        public object HandleFieldItem(FieldsItem fieldsItem, IPublishedContent content, string fieldAlias)
        {
            var currentDocumentTypeSettings = _documentTypeSettingsService.Get(content.ContentType.Id);
            
            //Should be fine, but always good to check
            if (currentDocumentTypeSettings?.Inheritance is null)
                return null;

            var inheritance = currentDocumentTypeSettings.Inheritance;
            while (inheritance != null)
            {
                //If this happens then we have an infinite loop.
                if (inheritance.Id == content.ContentType.Id)
                    return null;

                var inheritedSettings = _documentTypeSettingsService.Get(inheritance.Id);
                var documentTypeValue = inheritedSettings.Get(fieldAlias);
                if (documentTypeValue != null && documentTypeValue.UseInheritedValue)
                    inheritance = inheritedSettings.Inheritance;
                else
                    return documentTypeValue?.Value;
            }

            return null;
        }
    }
}
