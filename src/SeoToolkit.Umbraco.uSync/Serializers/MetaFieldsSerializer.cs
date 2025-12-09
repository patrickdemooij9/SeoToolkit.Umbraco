using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers
{
    [SyncSerializer("32ed1708-5bac-456d-9af1-6e63ada8635c", "Meta fields serializer", "MetaFieldSetting")]
    public class MetaFieldsSerializer : SyncSerializerRoot<DocumentTypeSettingsDto>, ISyncSerializer<DocumentTypeSettingsDto>
    {
        private readonly IMetaFieldsSettingsService _metaFieldsSettingsService;
        private readonly IContentTypeService _contentTypeService;
        private readonly SeoFieldCollection _seoFieldCollection;

        public MetaFieldsSerializer(ILogger<SyncSerializerRoot<DocumentTypeSettingsDto>> logger, IMetaFieldsSettingsService metaFieldsSettingsService, IContentTypeService contentTypeService, SeoFieldCollection seoFieldCollection) : base(logger)
        {
            _metaFieldsSettingsService = metaFieldsSettingsService;
            _contentTypeService = contentTypeService;
            _seoFieldCollection = seoFieldCollection;
        }

        public override Task DeleteItemAsync(DocumentTypeSettingsDto item)
        {
            _metaFieldsSettingsService.Delete(item.Content.Key);
            return Task.CompletedTask;
        }

        public override Task<DocumentTypeSettingsDto?> FindItemAsync(Guid key)
        {
            return Task.FromResult(_metaFieldsSettingsService.Get(key));
        }

        public override Task<DocumentTypeSettingsDto?> FindItemAsync(string alias)
        {
            var contentType = _contentTypeService.Get(alias);
            if (contentType is null)
            {
                return Task.FromResult<DocumentTypeSettingsDto?>(null);
            }
            return Task.FromResult(_metaFieldsSettingsService.Get(contentType.Key));
        }

        public override string ItemAlias(DocumentTypeSettingsDto item)
        {
            return item.Content.Alias;
        }

        public override Guid ItemKey(DocumentTypeSettingsDto item)
        {
            return item.Content.Key;
        }

        public override Task SaveItemAsync(DocumentTypeSettingsDto item)
        {
            _metaFieldsSettingsService.Set(item);
            return Task.CompletedTask;
        }

        protected override async Task<SyncAttempt<DocumentTypeSettingsDto>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
        {
            var item = await FindItemAsync(node);
            if (item is null)
            {
                var contentTypeKey = node.Element("Key").ValueOrDefault(Guid.Empty);
                if (contentTypeKey == Guid.Empty)
                {
                    return SyncAttempt<DocumentTypeSettingsDto>.Fail(node.GetAlias(), ChangeType.Fail, "No valid content type key");
                }
                var contentType = _contentTypeService.Get(contentTypeKey);
                if (contentType is null)
                {
                    return SyncAttempt<DocumentTypeSettingsDto>.Fail(node.GetAlias(), ChangeType.Fail, "Content type not found");
                }
                item = new DocumentTypeSettingsDto
                {
                    Content = contentType
                };
            }

            var infoNode = node.Element("Info");
            if (infoNode is null)
            {
                return SyncAttempt<DocumentTypeSettingsDto>.Fail(node.GetAlias(), ChangeType.Fail, "No Info node");
            }

            var inheritanceKey = infoNode.Element("InheritanceKey").ValueOrDefault<Guid?>(null);
            if (inheritanceKey.HasValue)
            {
                var inheritanceContentType = _contentTypeService.Get(inheritanceKey.Value);
                if (inheritanceContentType is not null)
                {
                    item.Inheritance = inheritanceContentType;
                }
            }

            foreach (var fieldNode in infoNode.Element("Fields")?.Elements("Field") ?? [])
            {
                var fieldAlias = fieldNode.Element("Alias").ValueOrDefault(string.Empty);
                var seoField = _seoFieldCollection.Get(fieldAlias);
                if (seoField is null)
                {
                    continue;
                }
                var fieldValue = new DocumentTypeValueDto
                {
                    UseInheritedValue = fieldNode.Element("UseInheritedValue").ValueOrDefault(false),
                };
                var value = fieldNode.Element("Value").ValueOrDefault<string?>(null);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    fieldValue.Value = seoField.Editor.ValueConverter.ConvertDatabaseToObject(JsonConvert.DeserializeObject(value));
                }
                if (!item.Fields.TryAdd(seoField, fieldValue))
                {
                    item.Fields[seoField] = fieldValue;
                }
            }

            return SyncAttempt<DocumentTypeSettingsDto>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
        }

        protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(DocumentTypeSettingsDto item, SyncSerializerOptions options)
        {
            var node = new XElement(ItemType,
                new XAttribute("Key", item.Content.Key),
                new XAttribute("Alias", item.Content.Alias)
            );

            var info = new XElement("Info");
            if (item.Inheritance is not null)
            {
                info.Add(new XElement("InheritanceKey", item.Inheritance.Key));
            }
            var fieldsNode = new XElement("Fields");
            foreach (var field in item.Fields)
            {
                var fieldNode = new XElement("Field",
                    new XElement("Alias", field.Key.Alias),
                    new XElement("UseInheritedValue", field.Value.UseInheritedValue),
                    new XElement("Value", JsonConvert.SerializeObject(field.Value.Value))
                );
                fieldsNode.Add(fieldNode);
            }
            info.Add(fieldsNode);
            node.Add(info);

            return Task.FromResult(SyncAttempt<XElement>.Succeed(ItemAlias(item), node, ChangeType.Export, []));
        }
    }
}
