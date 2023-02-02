using System;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.PostModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Mappers
{
    public class DocumentTypeSettingsMapper : IMapDefinition
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly SeoFieldCollection _seoFieldCollection;

        public DocumentTypeSettingsMapper(IContentTypeService contentTypeService, SeoFieldCollection seoFieldCollection)
        {
            _contentTypeService = contentTypeService;
            _seoFieldCollection = seoFieldCollection;
        }

        public void DefineMaps(IUmbracoMapper mapper)
        {
            mapper.Define<DocumentTypeSettingsPostViewModel, DocumentTypeSettingsDto>(
                (source, context) => new DocumentTypeSettingsDto(),
                (source, target, context) =>
                {
                    target.Content = _contentTypeService.Get(source.NodeId);
                    foreach (var item in source.Fields)
                    {
                        var field = _seoFieldCollection.Get(item.Key);
                        if (field is null)
                            continue;

                        var value = item.Value.Value;
                        if (field.Editor is ISeoFieldEditorProcessor processor)
                            value = processor.HandleValue(value);

                        target.Fields.Add(field, new DocumentTypeValueDto
                        {
                            UseInheritedValue = item.Value.UseInheritedValue,
                            Value = field.Editor.ValueConverter.ConvertEditorToDatabaseValue(value)
                        });
                    }
                    target.Inheritance = source.InheritanceId is null ? null : _contentTypeService.Get(source.InheritanceId.Value);
                });

            mapper.Define<MetaFieldsSettingsEntity, DocumentTypeSettingsDto>(
                (source, context) => new DocumentTypeSettingsDto(),
                (source, target, context) =>
                {
                    target.Content = _contentTypeService.Get(source.NodeId);
                    if (!string.IsNullOrWhiteSpace(source.Fields))
                    {
                        var fields = JsonConvert.DeserializeObject<MetaFieldsFieldEntity[]>(source.Fields);
                        foreach (var item in fields)
                        {
                            var field = _seoFieldCollection.Get(item.Alias);
                            if (field is null)
                                continue;

                            //TODO: Add UseInheritedValue if the item has inheritance and the item is empty (Newly created field)
                            target.Fields.Add(field, new DocumentTypeValueDto
                            {
                                UseInheritedValue = item.UseInheritedValue,
                                Value = field.Editor.ValueConverter.ConvertDatabaseToObject(item.Value)
                            });
                        }
                    }
                    target.Inheritance = source.InheritanceId is null ? null : _contentTypeService.Get(source.InheritanceId.Value);
                });

            mapper.Define<DocumentTypeSettingsDto, MetaFieldsSettingsEntity>(
                (source, context) => new MetaFieldsSettingsEntity(),
                (source, target, context) =>
                {
                    target.NodeId = source.Content.Id;
                    target.Fields = JsonConvert.SerializeObject(source.Fields?.Select(it => new MetaFieldsFieldEntity
                    {
                        Alias = it.Key.Alias,
                        Value = it.Value.Value,
                        UseInheritedValue = it.Value.UseInheritedValue
                    }).ToArray() ?? Array.Empty<MetaFieldsFieldEntity>());
                    target.InheritanceId = source.Inheritance?.Id;
                });
        }
    }
}
