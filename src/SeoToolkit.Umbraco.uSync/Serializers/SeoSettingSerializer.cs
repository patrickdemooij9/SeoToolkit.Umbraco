using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers
{
    [SyncSerializer("d8b89568-8bac-4715-88e4-c1cb568d2abf", "Seo Settings Serializer", "SeoSetting")]
    public class SeoSettingSerializer : SyncSerializerRoot<SeoSetting>, ISyncSerializer<SeoSetting>
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly IContentTypeService _contentTypeService;

        public SeoSettingSerializer(ILogger<SyncSerializerRoot<SeoSetting>> logger, ISeoSettingsService seoSettingsService, IContentTypeService contentTypeService) : base(logger)
        {
            _seoSettingsService = seoSettingsService;
            _contentTypeService = contentTypeService;
        }

        public override Task DeleteItemAsync(SeoSetting item)
        {
            return Task.CompletedTask;
        }

        public override async Task<SeoSetting?> FindItemAsync(Guid key)
        {
            var contentType = await _contentTypeService.GetAsync(key);
            if (contentType is null) return null;

            var isEnabled = _seoSettingsService.IsEnabled(contentType);
            return new SeoSetting
            {
                ContentTypeKey = key,
                IsEnabled = isEnabled
            };
        }

        public override Task<SeoSetting?> FindItemAsync(string alias)
        {
            var contentType = _contentTypeService.Get(alias);
            if (contentType is null)
                return Task.FromResult<SeoSetting?>(null);

            var isEnabled = _seoSettingsService.IsEnabled(contentType);
            return Task.FromResult<SeoSetting?>(new SeoSetting
            {
                ContentTypeKey = contentType.Key,
                IsEnabled = isEnabled
            });
        }

        public override string ItemAlias(SeoSetting item)
        {
            var contentType = _contentTypeService.Get(item.ContentTypeKey);
            return contentType?.Name ?? item.ContentTypeKey.ToString();
        }

        public override Guid ItemKey(SeoSetting item)
        {
            return item.ContentTypeKey;
        }

        public override Task SaveItemAsync(SeoSetting item)
        {
            _seoSettingsService.ToggleSeoSettings(item.ContentTypeKey, item.IsEnabled);
            return Task.CompletedTask;
        }

        protected override async Task<SyncAttempt<SeoSetting>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
        {
            var item = await FindItemAsync(node);
            item ??= new SeoSetting
            {
                ContentTypeKey = node.Element("Key").ValueOrDefault(Guid.Empty)
            };
            if (item.ContentTypeKey == Guid.Empty)
            {
                return SyncAttempt<SeoSetting>.Fail(node.GetAlias(), ChangeType.Fail, "No valid Key");
            }

            var infoNode = node.Element("Info");
            if (infoNode is null)
            {
                return SyncAttempt<SeoSetting>.Fail(node.GetAlias(), ChangeType.Fail, "No Info node");
            }

            item.IsEnabled = infoNode.Element("IsEnabled").ValueOrDefault(false);
            return SyncAttempt<SeoSetting>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
        }

        protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(SeoSetting item, SyncSerializerOptions options)
        {
            var node = new XElement(ItemType,
                new XAttribute("Key", item.ContentTypeKey),
                new XAttribute("Alias", ItemAlias(item))
            );

            var info = new XElement("Info",
                new XElement("IsEnabled", item.IsEnabled)
            );

            node.Add(info);

            return Task.FromResult(SyncAttempt<XElement>.Succeed(item.ContentTypeKey.ToString(), node, ChangeType.Export, []));
        }
    }
}
