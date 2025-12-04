using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers
{
    /*[SyncSerializer("330bef6c-0e4a-4c8a-bebb-d0244ce94b10", "RobotsTxt Serializer", "RobotsTxt")]
    public class RobotsTxtSerializer : SyncSerializerRoot<RobotsTxtModel>, ISyncSerializer<RobotsTxtModel>
    {
        private readonly IRobotsTxtService _robotsTxtService;

        public RobotsTxtSerializer(ILogger<SyncSerializerRoot<RobotsTxtModel>> logger, IRobotsTxtService robotsTxtService) : base(logger)
        {
            _robotsTxtService = robotsTxtService;
        }

        public override Task DeleteItemAsync(RobotsTxtModel item)
        {
            return Task.CompletedTask;
        }

        public override Task<RobotsTxtModel?> FindItemAsync(Guid key)
        {
            return Task.FromResult(_robotsTxtService.Get(key));
        }

        public override Task<RobotsTxtModel?> FindItemAsync(string alias)
        {
            return Task.FromResult<RobotsTxtModel?>(null);
        }

        public override string ItemAlias(RobotsTxtModel item)
        {
            return item.Key.ToString();
        }

        public override bool IsValid(XElement node)
        {
            return node.Name.LocalName == ItemType
                && node.GetKey() != Guid.Empty;
        }

        public override Guid ItemKey(RobotsTxtModel item)
        {
            return item.Key;
        }

        public override Task SaveItemAsync(RobotsTxtModel item)
        {
            _robotsTxtService.Save(item);
            return Task.CompletedTask;
        }

        protected async override Task<SyncAttempt<RobotsTxtModel>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
        {
            var item = await FindItemAsync(node);
            item ??= new RobotsTxtModel
            {
                Key = node.Element("Key").ValueOrDefault(Guid.NewGuid())
            };

            var infoNode = node.Element("Info");
            if (infoNode is null)
            {
                return SyncAttempt<RobotsTxtModel>.Fail(node.GetAlias(), ChangeType.Fail, "No Info node");
            }

            item.Content = infoNode.Element("Content").ValueOrDefault(string.Empty);
            item.DomainId = infoNode.Element("DomainId").ValueOrDefault<Guid?>(null);

            return SyncAttempt<RobotsTxtModel>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
        }

        protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(RobotsTxtModel item, SyncSerializerOptions options)
        {
            var node = new XElement(ItemType,
                new XAttribute("Key", item.Key)
            );

            var info = new XElement("Info",
                new XElement("Content", item.Content),
                new XElement("DomainId", item.DomainId)
            );

            node.Add(info);

            return Task.FromResult(SyncAttempt<XElement>.Succeed(item.Key.ToString(), node, ChangeType.Export, []));
        }
    }*/
}
