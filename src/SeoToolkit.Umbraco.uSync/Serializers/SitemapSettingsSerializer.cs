using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers
{
    [SyncSerializer("5f802097-5321-4b8d-8bfd-ebe693b64f05", "RobotsTxt Serializer", "RobotsTxt")]
    public class SitemapSettingsSerializer : SyncSerializerRoot<SitemapPageSettings>, ISyncSerializer<SitemapPageSettings>
    {
        private readonly ISitemapService _sitemapService;

        public SitemapSettingsSerializer(ILogger<SyncSerializerRoot<SitemapPageSettings>> logger, ISitemapService sitemapService) : base(logger)
        {
            _sitemapService = sitemapService;
        }

        public override Task DeleteItemAsync(SitemapPageSettings item)
        {
            //TODO: Implement actual deletion
            return Task.CompletedTask;
        }

        public override Task<SitemapPageSettings?> FindItemAsync(Guid key)
        {
            return Task.FromResult(_sitemapService.GetPageTypeSettings(key));
        }

        public override Task<SitemapPageSettings?> FindItemAsync(string alias)
        {
            return Task.FromResult<SitemapPageSettings?>(null);
        }

        public override string ItemAlias(SitemapPageSettings item)
        {
            return item.ContentTypeGuid.ToString();
        }

        public override Guid ItemKey(SitemapPageSettings item)
        {
            return item.ContentTypeGuid;
        }

        public override Task SaveItemAsync(SitemapPageSettings item)
        {
            _sitemapService.SetPageTypeSettings(item);
            return Task.CompletedTask;
        }

        protected override async Task<SyncAttempt<SitemapPageSettings>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
        {
            var item = await FindItemAsync(node);
            item ??= new SitemapPageSettings
            {
                ContentTypeGuid = node.Element("Key").ValueOrDefault(Guid.NewGuid())
            };

            var infoNode = node.Element("Info");
            if (infoNode is null)
            {
                return SyncAttempt<SitemapPageSettings>.Fail(node.GetAlias(), ChangeType.Fail, "No Info node");
            }

            item.HideFromSitemap = infoNode.Element("HideFromSitemap").ValueOrDefault(false);
            item.ChangeFrequency = infoNode.Element("ChangeFrequency").ValueOrDefault<string?>(null);
            item.Priority = infoNode.Element("Priority").ValueOrDefault<double?>(null);

            return SyncAttempt<SitemapPageSettings>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
        }

        protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(SitemapPageSettings item, SyncSerializerOptions options)
        {
            var node = new XElement(ItemType,
                new XAttribute("Key", item.ContentTypeGuid)
            );

            var info = new XElement("Info",
                new XElement("HideFromSitemap", item.HideFromSitemap),
                new XElement("ChangeFrequency", item.ChangeFrequency),
                new XElement("Priority", item.Priority)
            );

            node.Add(info);

            return Task.FromResult(SyncAttempt<XElement>.Succeed(item.ContentTypeGuid.ToString(), node, ChangeType.Export, []));
        }
    }
}
