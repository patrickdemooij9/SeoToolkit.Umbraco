using System;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.PostModels;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Mappers
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
                    target.EnableSeoSettings = source.EnableSeoSettings;
                    foreach (var item in source.Fields)
                    {
                        var field = _seoFieldCollection.Get(item.Key);
                        if (field is null)
                            continue;

                        target.Fields.Add(field, new DocumentTypeValueDto
                        {
                            UseInheritedValue = item.Value.UseInheritedValue,
                            Value = field.Editor.ValueConverter.ConvertEditorToDatabaseValue(item.Value.Value)
                        });
                    }
                    target.Inheritance = source.InheritanceId is null ? null : _contentTypeService.Get(source.InheritanceId.Value);
                });

            mapper.Define<DocumentTypeSettingsEntity, DocumentTypeSettingsDto>(
                (source, context) => new DocumentTypeSettingsDto(),
                (source, target, context) =>
                {
                    target.Content = _contentTypeService.Get(source.NodeId);
                    target.EnableSeoSettings = source.EnableSeoSettings;
                    if (!string.IsNullOrWhiteSpace(source.Fields))
                    {
                        var fields = JsonConvert.DeserializeObject<DocumentTypeFieldEntity[]>(source.Fields);
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

            mapper.Define<DocumentTypeSettingsDto, DocumentTypeSettingsEntity>(
                (source, context) => new DocumentTypeSettingsEntity(),
                (source, target, context) =>
                {
                    target.NodeId = source.Content.Id;
                    target.EnableSeoSettings = source.EnableSeoSettings;
                    target.Fields = JsonConvert.SerializeObject(source.Fields?.Select(it => new DocumentTypeFieldEntity
                    {
                        Alias = it.Key.Alias,
                        Value = it.Value.Value,
                        UseInheritedValue = it.Value.UseInheritedValue
                    }).ToArray() ?? Array.Empty<DocumentTypeFieldEntity>());
                    target.InheritanceId = source.Inheritance?.Id;
                });
        }
    }
}
